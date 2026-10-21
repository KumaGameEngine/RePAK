using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKLib
{
    public sealed class SubStream : Stream
    {
        private readonly Stream _parent;
        private readonly long _start;
        private readonly long _length;
        private long _position;

        public SubStream(Stream parent, long offset, long length)
        {
            ArgumentNullException.ThrowIfNull(parent);
            if (!parent.CanSeek) throw new ArgumentException("Parent must be seekable.", nameof(parent));
            if (offset < 0 || length < 0 || offset + length > parent.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            _parent = parent;
            _start = offset;
            _length = length;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            long remaining = _length - _position;
            if (remaining <= 0) return 0;
            if (buffer.Length > remaining) buffer = buffer[..(int)remaining];

            _parent.Position = _start + _position;
            int read = _parent.Read(buffer);
            _position += read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            long remaining = _length - _position;
            if (remaining <= 0) return 0;
            if (buffer.Length > remaining) buffer = buffer[..(int)remaining];

            _parent.Position = _start + _position;
            int read = await _parent.ReadAsync(buffer, ct);
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            if (target < 0) throw new IOException("Seek before start of stream.");
            _position = target;
            return _position;
        }

        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing) => base.Dispose(disposing);
    }
}
