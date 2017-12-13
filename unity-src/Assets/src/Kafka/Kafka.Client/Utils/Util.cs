﻿namespace Kafka.Client.Utils
{
    using System;
    using System.Reflection;
    using System.Text;

    using Kafka.Client.Common.Imported;

    using log4net;

    /// <summary>
    /// Original name: Utils
    /// 
    /// General helper functions!
    /// </summary>
    public static class Util
    {
         /// <summary>
         ///  Read the given byte buffer into a byte array
         /// </summary>
         /// <param name="buffer"></param>
         /// <returns></returns>
        public static byte[] ReadBytes(ByteBuffer buffer)
        {
             return ReadBytes(buffer, 0, buffer.Limit());
        }

        public static byte[] ReadBytes(ByteBuffer buffer, int offset, int size)
        {
            var result = new byte[size];
            Buffer.BlockCopy(buffer.Array, buffer.ArrayOffset(), result, 0, size);
            return result;
        }

        /// <summary>
        /// Read an unsigned integer from the current position in the buffer, 
        /// incrementing the position by 4 bytes
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static long ReadUnsignedInt(ByteBuffer buffer)
        {
            return buffer.GetInt() & 0xffffffffL;
        }

        /// <summary>
        /// Read an unsigned integer from the given position without modifying the buffers
        /// position
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static long ReadUnsingedInt(ByteBuffer buffer, int index)
        {
            return buffer.GetInt(index) & 0xffffffffL;
        }

        /// <summary>
        /// Write the given long value as a 4 byte unsigned integer. Overflow is ignored.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="value">The value to write</param>
        public static void WriteUnsignedInt(ByteBuffer buffer, long value)
        {
            buffer.PutInt((int)(value & 0xffffffffL));
        }

        /// <summary>
        /// Write the given long value as a 4 byte unsigned integer. Overflow is ignored
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public static void WriteUnsignedInt(ByteBuffer buffer, int index, long value)
        {
            buffer.PutInt(index, (int)(value & 0xffffffffL));
        }


        /// <summary>
        /// Compute the CRC32 of the byte array
        /// </summary>
        /// <param name="bytes">The array to compute the checksum for</param>
        /// <returns>The CRC32</returns>
        public static long Crc32(byte[] bytes)
        {
            return Crc32(bytes, 0, bytes.Length);
        }

        public static long Crc32(byte[] bytes, int offset, int size)
        {
            return Utils.Crc32.Compute(bytes, offset, size) & 0xffffffffL;
        }

        public static T CreateObject<T>(string type, params object[] args) where T : class
        {
            return (T)Activator.CreateInstance(Type.GetType(type), args);
        }

        public static string ReadString(ByteBuffer buffer)
        {
            var bytes = new byte[buffer.Remaining()];
            buffer.Get(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static void SwallowError(ILog logger, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        public static void Swallow(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
            }
        }
    }
}