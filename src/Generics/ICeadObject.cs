using CeadLibrary.IO;

namespace CeadLibrary.Generics
{
    public interface ICeadObject
    {
        public ICeadObject Read(CeadReader reader);
        public void Write(CeadWriter writer);
    }
}
