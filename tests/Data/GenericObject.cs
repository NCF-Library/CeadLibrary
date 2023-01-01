using CeadLibrary.Generics;
using CeadLibrary.IO;
using Tests.Extensions;

namespace Tests.Data
{
    public class GenericObject : ICeadObject
    {
        public string Name { get; set; } = "John";
        public string? LastName { get; set; }
        public int Age { get; set; }

        public GenericObject() { }
        public GenericObject(string name, string lastName, int age)
        {
            Name = name;
            LastName = lastName;
            Age = age;
        }

        public ICeadObject Read(CeadReader reader)
        {
            reader.CheckMagic("GOBJ"u8);

            Name = reader.ReadString(StringType.Int16CharCount);
            reader.Seek(1, SeekOrigin.Current);
            reader.Align(4);

            LastName = reader.ReadString(StringType.Int16CharCount);
            reader.Seek(1, SeekOrigin.Current);
            reader.Align(4);

            Age = reader.ReadInt16();

            return this;
        }

        public void Write(CeadWriter writer)
        {
            writer.Write("GOBJ"u8);

            writer.WritePascalString(Name);
            writer.Align(4);
            writer.WritePascalString(LastName);
            writer.Align(4);

            writer.Write((short)Age);
            writer.Align(4);
        }

        public ICeadObject Read(BinaryReader reader)
        {
            reader.CheckMagic("GOBJ"u8);

            Name = reader.ReadString(StringType.Int16CharCount);
            reader.BaseStream.Seek(1, SeekOrigin.Current);
            reader.Align(4);

            LastName = reader.ReadString(StringType.Int16CharCount);
            reader.BaseStream.Seek(1, SeekOrigin.Current);
            reader.Align(4);

            Age = reader.ReadInt16();

            return this;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write("GOBJ"u8);

            writer.WritePascalString(Name);
            writer.Align(4);
            writer.WritePascalString(LastName ?? "");
            writer.Align(4);

            writer.Write((short)Age);
            writer.Align(4);
        }
    }
}
