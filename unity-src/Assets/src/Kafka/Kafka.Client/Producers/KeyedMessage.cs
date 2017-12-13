﻿namespace Kafka.Client.Producers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A topic, key, and value.
    /// If a partition key is provided it will override the key for the purpose of partitioning but will not be stored.
    /// </summary>
    public class KeyedMessage<TKey, TValue>
    {
        public readonly string Topic;

        public readonly TKey Key;

        public readonly object PartKey;

        public readonly TValue Message;

        public KeyedMessage(string topic, TKey key, object partKey, TValue message)
        {
            this.Topic = topic;
            this.Key = key;
            this.PartKey = partKey;
            this.Message = message;
            if (topic == null)
            {
                throw new ArgumentException("Topic cannot be null", "topic");
            }
        }

        public KeyedMessage(string topic, TValue message)
            : this(topic, default(TKey), null, message)
        {
        }

        public KeyedMessage(string topic, TKey key, TValue message)
            : this(topic, key, key, message)
        {
        }

        public object PartitionKey
        {
            get
            {
                if (this.PartKey != null)
                {
                    return this.PartKey;
                }

                if (this.HasKey)
                {
                    return this.Key;
                }

                return null;
            }
        }

        public bool HasKey 
        { 
            get
            {
                return this.Key != null;
            }
        }

        protected bool Equals(KeyedMessage<TKey, TValue> other)
        {
            return string.Equals(this.Topic, other.Topic) && EqualityComparer<TKey>.Default.Equals(this.Key, other.Key) && Equals(this.PartKey, other.PartKey) && EqualityComparer<TValue>.Default.Equals(this.Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((KeyedMessage<TKey, TValue>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Topic != null ? this.Topic.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ EqualityComparer<TKey>.Default.GetHashCode(this.Key);
                hashCode = (hashCode * 397) ^ (this.PartKey != null ? this.PartKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ EqualityComparer<TValue>.Default.GetHashCode(this.Message);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Topic: {0}, Key: {1}, PartKey: {2}, Message: {3}", this.Topic, this.Key, this.PartKey, this.Message);
        }
    }
}