﻿namespace Kafka.Tests.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Kafka.Client.Common.Imported;
    using Kafka.Client.Extensions;
    using Kafka.Client.Messages;
    using Kafka.Tests.Utils;

    using Xunit;

    public class ByteBufferMessageSetTest
    {
        protected List<Message> Messages { get; private set; }

        public ByteBufferMessageSetTest()
        {
            this.Messages = new List<Message>
                                {
                                    new Message(Encoding.UTF8.GetBytes("abcd")),
                                    new Message(Encoding.UTF8.GetBytes("efgh")),
                                    new Message(Encoding.UTF8.GetBytes("ijkl")),
                                };
        }

        [Fact]
        public void TestWrittenEqualsRead()
        {
            var messageSet = this.CreateMessageSet(this.Messages);
            var expected = TestUtil.EnumeratorToArray(this.Messages.GetEnumerator());
            var actual = messageSet.Select(m => m.Message).ToList();
            Assert.Equal(expected, actual);
        }
       
        [Fact]
        public void TestIteratorIsConsistent()
        {
            var m = this.CreateMessageSet(this.Messages);
            var expected = TestUtil.IteratorToArray(m.Iterator());
            var actual = TestUtil.IteratorToArray(m.Iterator());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestSizeInBytes()
        {
            Assert.Equal(0, this.CreateMessageSet(new List<Message>()).SizeInBytes);
            Assert.Equal(MessageSet.MessageSetSize(this.Messages), this.CreateMessageSet(this.Messages).SizeInBytes);
        }
        
        [Fact]
        public void TestWriteTo()
        {
            // test empty message set
            this.TestWriteToWithMessageSet(this.CreateMessageSet(new List<Message>()));
            this.TestWriteToWithMessageSet(this.CreateMessageSet(this.Messages));
        }

        internal void TestWriteToWithMessageSet(MessageSet set)
        {
            // do the write twice to ensure the message set is restored to its orginal state
            for (var i = 0; i < 1; i++)
            {
                var stream = ByteBuffer.Allocate(1024);
                var written = set.WriteTo(stream, 0, 1024);
                stream.SetLength(written);
                Assert.Equal(set.SizeInBytes, written);
                stream.Position = 0;
                var newSet = new ByteBufferMessageSet(stream);
                Assert.Equal(TestUtil.IteratorToArray(set.Iterator()), TestUtil.IteratorToArray(newSet.Iterator()));
            }
        }

        [Fact]
        public void TestValidBytes()
        {
            var messages = new ByteBufferMessageSet(
                CompressionCodecs.NoCompressionCodec,
                new List<Message>
                {
                  new Message(Encoding.UTF8.GetBytes("hello")),
                  new Message(Encoding.UTF8.GetBytes("there"))
                });
            var buffer = ByteBuffer.Allocate(messages.SizeInBytes + 2);
            buffer.Put(messages.Buffer);
            buffer.PutShort(4);

            var messagesPlus = new ByteBufferMessageSet(buffer);
            Assert.Equal(messages.ValidBytes, messagesPlus.ValidBytes);

            // test valid bytes on empty ByteBufferMessageSet
            {
                Assert.Equal(0, MessageSet.Empty.ValidBytes);
            }
        }
        
        [Fact]
        public void TestValidBytesWithCompression()
        {
            var messages = new ByteBufferMessageSet(
                CompressionCodecs.DefaultCompressionCodec,
                new List<Message>
                {
                  new Message(Encoding.UTF8.GetBytes("hello")),
                  new Message(Encoding.UTF8.GetBytes("there"))
                });
            var buffer = ByteBuffer.Allocate(messages.SizeInBytes + 2);
            buffer.Put(messages.Buffer);
            buffer.PutShort(4);
            var messagesPlus = new ByteBufferMessageSet(buffer);
            Assert.Equal(messages.ValidBytes, messagesPlus.ValidBytes);
        }
        
        [Fact]
        public void TestEquals()
        {
            var messages = new ByteBufferMessageSet(
                CompressionCodecs.DefaultCompressionCodec,
                new List<Message> 
                {
                  new Message(Encoding.UTF8.GetBytes("hello")),
                  new Message(Encoding.UTF8.GetBytes("there"))
                });
            var moreMessages = new ByteBufferMessageSet(
                CompressionCodecs.DefaultCompressionCodec,
                new List<Message>
                {
                  new Message(Encoding.UTF8.GetBytes("hello")),
                  new Message(Encoding.UTF8.GetBytes("there"))
                });

            Assert.True(messages.Equals(moreMessages));

            messages = new ByteBufferMessageSet(
               CompressionCodecs.NoCompressionCodec,
               new List<Message>
                {
                  new Message(Encoding.UTF8.GetBytes("hello")),
                  new Message(Encoding.UTF8.GetBytes("there"))
                });
            moreMessages = new ByteBufferMessageSet(
                CompressionCodecs.NoCompressionCodec,
                new List<Message>
                {
                  new Message(Encoding.UTF8.GetBytes("hello")),
                  new Message(Encoding.UTF8.GetBytes("there"))
                });

            Assert.True(messages.Equals(moreMessages));
        }
        
        [Fact]
        public void TestIterator()
        {
            var messageList = new List<Message>
                                  {
                                      new Message(Encoding.UTF8.GetBytes("msg1")),
                                      new Message(Encoding.UTF8.GetBytes("msg2")),
                                      new Message(Encoding.UTF8.GetBytes("msg3")),
                                  };

            // test for uncompressed regular Messages
            {
                var messageSet = new ByteBufferMessageSet(CompressionCodecs.NoCompressionCodec, messageList);
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(messageSet.Iterator()).ToEnumerable());

                // make sure ByteBufferMessageSet is re-iterable.
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(messageSet.Iterator()).ToEnumerable());

                // make sure shallow iterator is the same as deep iterator
                TestUtils.CheckEquals(
                    TestUtils.GetMessageIterator(messageSet.ShallowIterator()).ToEnumerable(),
                    TestUtils.GetMessageIterator(messageSet.Iterator()).ToEnumerable());
            }

            // test for compressed regular Messages
            {
                 var messageSet = new ByteBufferMessageSet(CompressionCodecs.DefaultCompressionCodec, messageList);
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(messageSet.Iterator()).ToEnumerable());

                // make sure ByteBufferMessageSet is re-iterable.
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(messageSet.Iterator()).ToEnumerable());
            }

            // test for mixed empty and non-empty messagesets uncompressed
            {
                List<Message> emptyMessageList = null;
                var emptyMessageSet = new ByteBufferMessageSet(CompressionCodecs.NoCompressionCodec, emptyMessageList);
                var regularMessgeSet = new ByteBufferMessageSet(CompressionCodecs.NoCompressionCodec, messageList);

                var buffer = ByteBuffer.Allocate(emptyMessageSet.Buffer.Limit() + regularMessgeSet.Buffer.Limit());
                buffer.Put(emptyMessageSet.Buffer);
                buffer.Put(regularMessgeSet.Buffer);
                buffer.Rewind();

                var mixedMessageSet = new ByteBufferMessageSet(buffer);
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(mixedMessageSet.Iterator()).ToEnumerable());

                // make sure ByteBufferMessageSet is re-iterable.
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(mixedMessageSet.Iterator()).ToEnumerable());

                // make sure shallow iterator is the same as deep iterator
                TestUtils.CheckEquals(
                    TestUtils.GetMessageIterator(mixedMessageSet.ShallowIterator()).ToEnumerable(),
                    TestUtils.GetMessageIterator(mixedMessageSet.Iterator()).ToEnumerable());
            }

             // test for mixed empty and non-empty messagesets compressed
            {
                List<Message> emptyMessageList = null;
                var emptyMessageSet = new ByteBufferMessageSet(CompressionCodecs.DefaultCompressionCodec, emptyMessageList);
                var regularMessgeSet = new ByteBufferMessageSet(CompressionCodecs.DefaultCompressionCodec, messageList);
                var buffer = ByteBuffer.Allocate(emptyMessageSet.Buffer.Limit() + regularMessgeSet.Buffer.Limit());
                buffer.Put(emptyMessageSet.Buffer);
                buffer.Put(regularMessgeSet.Buffer);
                buffer.Rewind();
                var mixedMessageSet = new ByteBufferMessageSet(buffer);
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(mixedMessageSet.Iterator()).ToEnumerable());

                // make sure ByteBufferMessageSet is re-iterable.
                TestUtils.CheckEquals(
                    messageList, TestUtils.GetMessageIterator(mixedMessageSet.Iterator()).ToEnumerable());

                this.VerifyShallowIterator(mixedMessageSet);
            }
        }
        
        [Fact]
        public void TestOffsetAssigment()
        {
            var messages = new ByteBufferMessageSet(
                CompressionCodecs.NoCompressionCodec,
                new List<Message>
                    {
                        new Message(Encoding.UTF8.GetBytes("hello")),
                        new Message(Encoding.UTF8.GetBytes("there")),
                        new Message(Encoding.UTF8.GetBytes("beautiful")),
                    });

            var compressedMessages = new ByteBufferMessageSet(
                CompressionCodecs.DefaultCompressionCodec, messages.Select(m => m.Message).ToList());

            // check uncompressed offsets 
            this.CheckOffsets(messages, 0);
            var offset = 1234567;
            this.CheckOffsets(messages.AssignOffsets(new AtomicLong(offset), CompressionCodecs.NoCompressionCodec), offset);

            // check compressed offset
            this.CheckOffsets(compressedMessages, 0);
            this.CheckOffsets(
                compressedMessages.AssignOffsets(new AtomicLong(offset), CompressionCodecs.DefaultCompressionCodec),
                offset);
        }

        internal void CheckOffsets(ByteBufferMessageSet messages, long baseOffset)
        {
            var offset = baseOffset;

            foreach (var entry in messages)
            {
                Assert.Equal(offset, entry.Offset);
                offset++;
            }
        }
        
        internal void VerifyShallowIterator(ByteBufferMessageSet messageSet)
        {
            // make sure the offsets returned by a shallow iterator is a subset of that of a deep iterator
            var shallowOffsets =
                new HashSet<long>(
                    TestUtil.IteratorToArray(messageSet.ShallowIterator()).Select(msgAndOff => msgAndOff.Offset));
            var deepOffsets = new HashSet<long>(
                TestUtil.IteratorToArray(messageSet.Iterator()).Select(msgAndOff => msgAndOff.Offset));
            Assert.True(shallowOffsets.IsSubsetOf(deepOffsets));
        }
        
        private ByteBufferMessageSet CreateMessageSet(List<Message> list)
        {
            return new ByteBufferMessageSet(CompressionCodecs.NoCompressionCodec, list);
        }
    }
}