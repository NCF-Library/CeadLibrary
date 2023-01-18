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
        protected readonly Dictionary<string, Action> _tasks = new();
        protected readonly Dictionary<int, List<long>> _ptrsMap = new();
        protected readonly Dictionary<string, List<Action>> _strings = new();
        protected readonly Dictionary<string, List<Action>> _reserved = new();

        public RltWriter(Stream stream) : base(stream) { }

        protected static void Register<TKey, TValue>(Dictionary<TKey, List<TValue>> src, TKey key, TValue value) where TKey : notnull
        {
            if (!src.ContainsKey(key)) {
                src.Add(key, new());
            }

            src[key].Add(value);
        }

        public void RegisterPtr(long offset, int rltSection = 0)
        {
            Register(_ptrsMap, rltSection, offset);
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
                Register(_reserved, key, writePtr);
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

        public Action WriteStringPtr(ReadOnlySpan<char> value, StringType? type = null, int rltSection = 0, bool addToStrPool = true) => WriteStringPtr<long>(value, type, rltSection, addToStrPool);
        public Action WriteStringPtr<PtrType>(ReadOnlySpan<char> value, StringType? type = null, int rltSection = 0, bool addToStrPool = true) where PtrType : struct
        {
            string strcpy = value.ToString();
            if (addToStrPool) {
                Register(_strings, strcpy, Write);
            }

            return Write;

            void Write() => WriteObjectPtr<PtrType>(addToStrPool ? () => { } : () => base.Write(strcpy, type), rltSection);
        }

        public Action WritePascalStringPtr(ReadOnlySpan<char> value, int rltSection = 0, bool addToStrPool = true) => WritePascalStringPtr<long>(value, rltSection, addToStrPool);
        public Action WritePascalStringPtr<PtrType>(ReadOnlySpan<char> value, int rltSection = 0, bool addToStrPool = true) where PtrType : struct
        {
            string strcpy = value.ToString();
            if (addToStrPool) {
                Register(_strings, strcpy, Write);
            }

            return Write;

            void Write() => WriteObjectPtr<PtrType>(addToStrPool ? () => { } : () => base.WritePascalString(strcpy), rltSection);
        }

        public virtual void WriteStringPool(int alignment = 2)
        {
            Write("STR "u8);
            Write(0U);
            Write(0L);
            Write(_strings.Count - 1);

            byte[] ReverseBinary(ReadOnlySpan<char> str)
            {
                Span<byte> data = new byte[str.Length * 3];
                int size = _encoding.GetBytes(str, data);
                data = data[..size];

                for (int i = 0; i < size; i++) {
                    data[i] = (byte)((data[i] * 0x0202020202ul & 0x010884422010ul) % 1023);
                }

                data.Reverse();
                return data.ToArray();
            }

            foreach (var str in _strings.OrderBy(x => ReverseBinary(x.Key))) {
                long offset = _stream.Position;
                foreach (var insertAction in str.Value) {
                    insertAction();
                }

                WritePascalString(str.Key);
                Align(alignment);
            }
        }

        public virtual void WriteRelocationTable(uint endOfData, ReadOnlySpan<byte> magic = default)
        {
            // Skip header and sections
            // to write after data collection
            long rltStart = _stream.Position;
            Seek(16 + 24 * _ptrsMap.Count, SeekOrigin.Current);

            int index = 0;
            List<(long, int)> entryCountList = new();
            foreach ((_, var ptrSource) in _ptrsMap) {
                List<long> ptrs = ptrSource.Distinct().ToList();
                IEnumerable<long> pointerList = ptrs.Order();

                int entryCount = 0;
                foreach (var ptr in pointerList) {
                    if (!ptrs.Contains(ptr)) {
                        continue;
                    }

                    int flag = 0;
                    for (int i = 0; i < 32; i++) {
                        long address = ptr + 8 * i;
                        if (ptrs.Contains(address)) {
                            flag |= 1 << i;
                            ptrs.Remove(address);
                        }
                    }

                    Write((uint)ptr);
                    Write(flag);

                    entryCount++;
                    index++;
                }

                entryCountList.Add((entryCount, index));
            }

            Seek(rltStart, SeekOrigin.Begin);
            if (magic == default) {
                magic = "RELT"u8;
            }

            Write(magic);
            Write((uint)BaseStream.Position - 4);
            Write(_ptrsMap.Count); // Assume each manged entry doesn't exceed 2^32 - 1
            Write(0U);

            foreach ((var entryCount, var _index) in entryCountList) {
                Write(0L); // base ptr
                Write(0U); // base ptr offset
                Write(0U); // section data size
                Write(_index);
                Write(entryCount);
            }
        }
    }
}
