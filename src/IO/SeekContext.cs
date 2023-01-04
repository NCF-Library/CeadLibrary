namespace CeadLibrary.IO
{
    public class SeekContext : IDisposable
    {
        private readonly long _origin;
        private readonly Stream _stream;
        private bool _disposed;

        public SeekContext(Stream stream, long offset, SeekOrigin origin)
        {
            _origin = stream.Position;
            _stream = stream;
            _stream.Seek(offset, origin);
        }

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    _stream.Seek(_origin, SeekOrigin.Begin);
                }

                _disposed = true;
            }
        }
    }
}
