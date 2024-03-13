using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MarketRadio.SelectionsLoader.Extensions
{
    public class ProgressableStream<T> : Stream
    {
        public delegate void Progress(long uploaded, long length, T @object); 
            
        private readonly Stream _stream;
        
        private readonly Progress _progress;
        private readonly T _object;
        private readonly long _length;
        private long _uploaded;

        public ProgressableStream(Stream stream, T @object, Progress progress)
        {
            _stream = stream;
            _progress = progress;
            _object = @object;

            _length = stream.Length;
        }
        
        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = _stream.Read(buffer, offset, count);
            _uploaded += count;
            _progress?.Invoke(_uploaded, _length, _object);
            return result;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var result = await base.ReadAsync(buffer, offset, count, cancellationToken);
            return result;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var result = await base.ReadAsync(buffer, cancellationToken);
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var result = _stream.Seek(offset, origin);
            _uploaded += offset;
            _progress?.Invoke(_uploaded, _length, _object);
            return result;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }
    }
}