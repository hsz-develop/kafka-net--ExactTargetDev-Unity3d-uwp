﻿namespace Kafka.Client.Messages
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Kafka.Client.Common.Imported;

    /// <summary>
    /// A set of messages with offsets. A message set has a fixed serialized form, though the container
    /// for the bytes could be either in-memory or on disk. The format of each message is
    /// as follows:
    /// 
    /// 8 byte message offset number
    /// 4 byte size containing an integer N
    /// N message bytes as described in the Message class
    /// </summary>
    public abstract class MessageSet : IIterable<MessageAndOffset>, IEnumerable<MessageAndOffset>
    {
        public const int MessageSizeLength = 4;

        public const int OffsetLength = 8;

        public const int LogOverhead = MessageSizeLength + OffsetLength;

        public static ByteBufferMessageSet Empty = new ByteBufferMessageSet(ByteBuffer.Allocate(0));

        /// <summary>
        /// The size of a message set containing the given messages
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static int MessageSetSize(IEnumerable<Message> messages)
        {
            return messages.Aggregate(
                0, (i, message) => i + EntrySize(message));
        }

        /// <summary>
        /// The size of a size-delimited entry in a message set
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static int EntrySize(Message message)
        {
            return LogOverhead + message.Size;
        }

        public abstract IIterator<MessageAndOffset> Iterator();

        /// <summary>
        /// Write the messages in this set to the given channel starting at the given offset byte. 
        /// Less than the complete amount may be written, but no more than maxSize can be. The number
        /// of bytes written is returned 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="offset"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public abstract int WriteTo(Stream channel, long offset, int maxSize);

        /// <summary>
        /// Gives the total size of this message set in bytes
        /// </summary>
        public abstract int SizeInBytes { get; }

        /// <summary>
        /// Validate the checksum of all the messages in the set. Throws an InvalidMessageException if the checksum doesn't
        /// match the payload for any message.
        /// </summary>
        public void Validate()
        {
            var iter = this.Iterator();
            while (iter.HasNext())
            {
                if (!iter.Next().Message.IsValid)
                {
                    throw new InvalidMessageException();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<MessageAndOffset> GetEnumerator()
        {
            var iter = this.Iterator();
            while (iter.HasNext())
            {
                yield return iter.Next();
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(this.GetType().Name + "(");
            var iter = this.Iterator();
            var i = 0;
            while (iter.HasNext() && i < 100)
            {
                var message = iter.Next();
                builder.Append(message);
                if (iter.HasNext())
                {
                    builder.Append(", ");
                }

                i++;
            }

            if (iter.HasNext())
            {
                builder.Append("...");
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}