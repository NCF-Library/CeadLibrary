using CeadLibrary.Generics;
using CeadLibrary.IO;
using System.Runtime.InteropServices;

namespace CeadLibrary.Writers
{
    /// <summary>
    /// Generic IO reader to manage relocations and string references
    /// </summary>
    public class RltWriter : CeadWriter, IDisposable
    {
        private readonly Dictionary<string, Action> _tasks = new();
        private readonly Dictionary<int, List<long>> _ptrsMap = new();
        private readonly Dictionary<string, List<Action>> _strings = new();
        private readonly Dictionary<string, List<Action>> _reserved = new();

        public RltWriter(Stream stream) : base(stream)
        {

        }

        public void RegisterPtr(long offset, int rltSection = 0)
        {
            if (!_ptrsMap.ContainsKey(rltSection)) {
                _ptrsMap.Add(rltSection, new());
            }

            _ptrsMap[rltSection].Add(offset);
        }

        public override Action WriteObjectPtr(ICeadObject obj) => WriteObjectPtr<long>(() => obj.Write(this), 0);
        public override Action WriteObjectPtr(Action writeObject) => WriteObjectPtr<long>(writeObject, 0);
        public override Action WriteObjectPtr<PtrType>(ICeadObject obj) where PtrType : struct => WriteObjectPtr<PtrType>(() => obj.Write(this), 0);
        public override Action WriteObjectPtr<PtrType>(Action writeObject) where PtrType : struct => WriteObjectPtr<PtrType>(writeObject, 0);

        public Action WriteObjectPtr(ICeadObject obj, int rltSection = 0, string? key = null) => WriteObjectPtr<long>(() => obj.Write(this), rltSection, key);
        public Action WriteObjectPtr(Action writeObject, int rltSection = 0, string? key = null) => WriteObjectPtr<long>(writeObject, rltSection, key);
        public Action WriteObjectPtr<PtrType>(ICeadObject obj, int rltSection = 0, string? key = null) where PtrType : struct => WriteObjectPtr<PtrType>(() => obj.Write(this), rltSection, key);
        public Action WriteObjectPtr<PtrType>(Action writeObject, int rltSection = 0, string? key = null) where PtrType : struct
        {
            long offset = _stream.Position;

            Action writePtr = typeof(PtrType).Name switch {
                "Byte" => () => WritePtr(pos => Write((byte)pos), byte.MaxValue),
                "SByte" => () => WritePtr(pos => Write((sbyte)pos), (ulong)sbyte.MaxValue),
                "Int16" => () => WritePtr(pos => Write((short)pos), (ulong)short.MaxValue),
                "UInt16" => () => WritePtr(pos => Write((ushort)pos), ushort.MaxValue),
                "Int32" => () => WritePtr(pos => Write((int)pos), int.MaxValue),
                "UInt32" => () => WritePtr(pos => Write((uint)pos), uint.MaxValue),
                "Int64" => () => WritePtr(Write, long.MaxValue),
                "UInt64" => () => WritePtr(pos => Write((ulong)pos), ulong.MaxValue),
                _ => throw new InvalidTypeException(
                    typeof(PtrType), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong))
            };

            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<PtrType>()];
            Write(buffer);

            if (key != null) {
                if (!_reserved.ContainsKey(key)) {
                    _reserved.Add(key, new());
                }

                _reserved[key].Add(writePtr);
            }

            return writePtr;

            void WritePtr(Action<long> writePtr, ulong ptrTypeMaxSize)
            {
                long pos = _stream.Position;
                if ((ulong)pos > ptrTypeMaxSize) {
                    throw new OverflowException($"Could not pack the value '{offset}' into a {typeof(PtrType)} struct ({ptrTypeMaxSize})");
                }

                if (ptrTypeMaxSize == long.MaxValue || ptrTypeMaxSize == ulong.MaxValue) {
                    RegisterPtr(offset);
                }

                Seek(offset, SeekOrigin.Begin);
                writePtr(pos);
                Seek(pos, SeekOrigin.Begin);
                writeObject();
            }
        }

        public Action WriteObjectPtrIf(bool condition, ICeadObject obj, int rltSection = 0, string? key = null, bool register = false, bool writeNullPtr = true)
            => WriteObjectPtrIf<long>(condition, () => obj.Write(this), rltSection, key, register, writeNullPtr);
        public Action WriteObjectPtrIf(bool condition, Action writeObject, int rltSection = 0, string? key = null, bool register = false, bool writeNullPtr = true)
            => WriteObjectPtrIf<long>(condition, writeObject, rltSection, key, register, writeNullPtr);
        public Action WriteObjectPtrIf<PtrType>(bool condition, ICeadObject obj, int rltSection = 0, string? key = null, bool register = false, bool writeNullPtr = true) where PtrType : struct
            => WriteObjectPtrIf<PtrType>(condition, () => obj.Write(this), rltSection, key, register, writeNullPtr);
        public Action WriteObjectPtrIf<PtrType>(bool condition, Action writeObject, int rltSection = 0, string? key = null, bool register = false, bool writeNullPtr = true) where PtrType : struct
        {
            if (condition) {
                return WriteObjectPtr<PtrType>(writeObject, rltSection, key);
            }

            if (writeNullPtr) {
                if (register) {
                    RegisterPtr(_stream.Position);
                }

                Span<byte> buffer = stackalloc byte[Marshal.SizeOf<PtrType>()];
                Write(buffer);
            }

            return () => { };
        }
    }
}
