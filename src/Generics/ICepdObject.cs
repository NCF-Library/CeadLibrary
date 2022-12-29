using CepdLibrary.IO;

namespace CepdLibrary.Generics
{
    public interface ICepdObject
    {
        public ICepdObject Read(CepdReader reader);
        public void Write(CepdWriter writer);
    }
}
