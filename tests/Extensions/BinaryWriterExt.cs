using CeadLibrary.IO;
using System.Runtime.InteropServices;
using System.Text;
using Tests.Data;

namespace Tests.Extensions
{
    public static class BinaryWriterExt
    {
        public static Action WriteObjectPtr<PtrType>(this BinaryWriter writer, Action writeObject) where PtrType : struct
        {
            long offset = writer.BaseStream.Position;

            Action writePtr = typeof(PtrType).Name switch {
                "Byte" => () => WritePtr((pos) => writer.Write((byte)pos), byte.MaxValue),
                "SByte" => () => WritePtr((pos) => writer.Write((sbyte)pos), (ulong)sbyte.MaxValue),
                "Int16" => () => WritePtr((pos) => writer.Write((short)pos), (ulong)short.MaxValue),
                "UInt16" => () => WritePtr((pos) => writer.Write((ushort)pos), ushort.MaxValue),
                "Int32" => () => WritePtr((pos) => writer.Write((int)pos), int.MaxValue),
                "UInt32" => () => WritePtr((pos) => writer.Write((uint)pos), uint.MaxValue),
                "Int64" => () => WritePtr(writer.Write, long.MaxValue),
                "UInt64" => () => WritePtr((pos) => writer.Write((ulong)pos), ulong.MaxValue),
                _ => throw new InvalidTypeException(
                    typeof(PtrType), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong))
            };

            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<PtrType>()];
            writer.Write(buffer);

            return writePtr;

            void WritePtr(Action<long> writePtr, ulong ptrTypeMaxSize)
            {
                long pos = writer.BaseStream.Position;
                if ((ulong)pos > ptrTypeMaxSize) {
                    throw new OverflowException($"Could not pack the value '{offset}' into a {typeof(PtrType)} struct ({ptrTypeMaxSize})");
                }

                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writePtr(pos);
                writer.BaseStream.Seek(pos, SeekOrigin.Begin);
                writeObject();
            }
        }

        public static void WriteObjects<T>(this BinaryWriter writer, IEnumerable<T> objects) where T : GenericObject => writer.WriteObjects(objects, (obj) => obj.Write(writer));
        public static void WriteObjects<T>(this BinaryWriter _, IEnumerable<T> objects, Action<T> writeObject)
        {
            foreach (var obj in objects) {
                writeObject(obj);
            }
        }

        public static void Align(this BinaryWriter writer, int alignment)
        {
            writer.BaseStream.Seek((alignment - writer.BaseStream.Position % alignment) % alignment, SeekOrigin.Current);
        }

        public static void WritePascalString(this BinaryWriter writer, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            writer.Write((ushort)data.Length);
            writer.Write(data);
            writer.Write((byte)0);
        }
    }
}
