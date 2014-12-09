using System;
using System.IO;

namespace BaggyBot.Commands
{
	class ProducerConsumerStream : Stream
	{
		private readonly MemoryStream innerStream;
		private long readPosition;
		private long writePosition;

		public ProducerConsumerStream()
		{
			innerStream = new MemoryStream();
		}

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return false; } }

		public override bool CanWrite { get { return true; } }

		public override void Flush()
		{
			lock (innerStream) {
				innerStream.Flush();
			}
		}

		public override long Length
		{
			get
			{
				lock (innerStream) {
					return innerStream.Length;
				}
			}
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			lock (innerStream) {
				innerStream.Position = readPosition;
				var red = innerStream.Read(buffer, offset, count);
				readPosition = innerStream.Position;

				return red;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException("SetLength");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			lock (innerStream) {
				innerStream.Position = writePosition;
				innerStream.Write(buffer, offset, count);
				writePosition = innerStream.Position;
			}
		}
	}
}
