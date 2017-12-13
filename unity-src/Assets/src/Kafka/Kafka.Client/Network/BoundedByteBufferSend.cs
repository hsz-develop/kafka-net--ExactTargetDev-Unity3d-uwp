﻿namespace Kafka.Client.Network
{
    using System;
    using System.IO;

    using Kafka.Client.Api;
    using Kafka.Client.Common.Imported;

    internal class BoundedByteBufferSend : Send
    {
        public ByteBuffer Buffer { get; private set; }

        private ByteBuffer sizeBuffer = ByteBuffer.Allocate(4);

        public BoundedByteBufferSend(ByteBuffer buffer)
        {
            this.Buffer = buffer;

            if (buffer.Remaining() > int.MaxValue - this.sizeBuffer.Limit())
            {
                throw new ArgumentException("Attempt to create a bounded buffer of " + buffer.Length + "bytes, but the maximum allowable size for a bounded buffer is " + (int.MaxValue - this.sizeBuffer.Length));
            }

            this.sizeBuffer.PutInt(buffer.Limit());
            this.sizeBuffer.Rewind();
        }

        public BoundedByteBufferSend(int size)
            : this(ByteBuffer.Allocate(size))
        {
        }

        public BoundedByteBufferSend(RequestOrResponse request) : this(request.SizeInBytes + (request.RequestId.HasValue ? 2 : 0))
        {
            if (request.RequestId.HasValue)
            {
                this.Buffer.PutShort(request.RequestId.Value);
            } 

            request.WriteTo(this.Buffer);
            this.Buffer.Rewind();
        }

        public override int WriteTo(Stream channel)
        {
            this.ExpectIncomplete();
            var written = 0;
            channel.Write(this.sizeBuffer.Array, this.sizeBuffer.ArrayOffset(), this.sizeBuffer.Limit());
            written += (int)this.sizeBuffer.Length;
            channel.Write(this.Buffer.Array, this.Buffer.ArrayOffset(), this.Buffer.Limit());
            written += (int)this.Buffer.Length;

            // custom: since .net Write doesn't return written bytes we assume that all was written.
            
            // if we are done, mark it off
            this.complete = true;
            return written;
        }
    }
}