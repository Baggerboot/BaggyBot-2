using System;
using System.IO;

namespace BaggyBot.Commands.Interpreters.Python
{
	internal class ProducerConsumerStream : Stream
	{
		private readonly MemoryStream innerStream;
		private readonly object streamLock = new object();
		private long readPosition;
		private long writePosition;

		public ProducerConsumerStream()
		{
			innerStream = new MemoryStream();
		}

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override void Flush()
		{
			lock (streamLock)
			{
				innerStream.Flush();
			}
		}

		public override long Length
		{
			get
			{
				lock (streamLock)
				{
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
			lock (streamLock)
			{
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
			lock (streamLock)
			{
				innerStream.Position = writePosition;
				innerStream.Write(buffer, offset, count);
				writePosition = innerStream.Position;
			}
		}
	}
}
