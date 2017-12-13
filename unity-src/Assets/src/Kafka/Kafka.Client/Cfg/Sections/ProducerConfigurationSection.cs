﻿/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Kafka.Client.Cfg.Sections
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Xml.Linq;

    using Kafka.Client.Cfg.Elements;
    using Kafka.Client.Messages;
    using Kafka.Client.Producers;
    using Kafka.Client.Producers.Async;

    public class ProducerConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("brokers", IsRequired = false, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(BrokerConfigurationElementCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public BrokerConfigurationElementCollection Brokers
        {
            get
            {
                return (BrokerConfigurationElementCollection)this["brokers"];
            }
        }

        [ConfigurationProperty(
           "partitioner",
           DefaultValue = ProducerConfig.DefaultPartitioner,
           IsRequired = false)]
        public string Partitioner
        {
            get
            {
                return (string)this["partitioner"];
            }
        }

        [ConfigurationProperty(
            "type",
            DefaultValue = ProducerConfig.DefaultProducerType,
            IsRequired = false)]
        public ProducerTypes ProducerType
        {
            get
            {
                return (ProducerTypes)this["type"];
            }
        }

        [ConfigurationProperty(
                  "compressionCodec",
                  DefaultValue = CompressionCodecs.DefaultCompressionCodec,
                  IsRequired = false)]
        public CompressionCodecs CompressionCodec
        {
            get
            {
                return Messages.CompressionCodec.GetCompressionCodec((int)this["compressionCodec"]);
            }
        }

        [ConfigurationProperty(
           "compressedTopics",
           DefaultValue = null,
           IsRequired = false)]
        public List<string> CompressedTopics
        {
            get
            {
                if (string.IsNullOrEmpty((string)this["compressedTopics"]))
                {
                    return new List<string>();
                }
                else
                {
                    return
                        new List<string>(
                            ((string)this["compressedTopics"]).Split(',').Where(x => !string.IsNullOrEmpty(x)));
                }
            }
        }

        [ConfigurationProperty(
            "messageSendMaxRetries",
            DefaultValue = ProducerConfig.DefaultMessageSendRetries,
            IsRequired = false)]
        public int MessageSendMaxRetries
        {
            get
            {
                return (int)this["messageSendMaxRetries"];
            }
        }

        [ConfigurationProperty(
            "retryBackoffMs",
            DefaultValue = ProducerConfig.DefaultRetryBackoffMs,
            IsRequired = false)]
        public int RetryBackoffMs
        {
            get
            {
                return (int)this["retryBackoffMs"];
            }
        }

        [ConfigurationProperty(
            "topicMetadataRefreshIntervalMs",
            DefaultValue = ProducerConfig.DefaultTopicMetadataRefreshIntervalMs,
            IsRequired = false)]
        public int TopicMetadataRefreshIntervalMs
        {
            get
            {
                return (int)this["topicMetadataRefreshIntervalMs"];
            }
        }

        [ConfigurationProperty(
            "queueBufferingMaxMs",
            DefaultValue = AsyncProducerConfig.DefaultQueueBufferingMaxMs,
            IsRequired = false)]
        public int QueueBufferingMaxMs
        {
            get
            {
                return (int)this["queueBufferingMaxMs"];
            }
        }

        [ConfigurationProperty(
            "queueBufferingMaxMessages",
            DefaultValue = AsyncProducerConfig.DefaultQueueBufferingMaxMessages,
            IsRequired = false)]
        public int QueueBufferingMaxMessages
        {
            get
            {
                return (int)this["queueBufferingMaxMessages"];
            }
        }

        [ConfigurationProperty(
            "queueEnqueueTimeoutMs",
            DefaultValue = AsyncProducerConfig.DefaultQueueEnqueueTimeoutMs,
            IsRequired = false)]
        public int QueueEnqueueTimeoutMs
        {
            get
            {
                return (int)this["queueEnqueueTimeoutMs"];
            }
        }

        [ConfigurationProperty(
            "batchNumMessages",
            DefaultValue = AsyncProducerConfig.DefaultBatchNumMessages,
            IsRequired = false)]
        public int BatchNumMessages
        {
            get
            {
                return (int)this["batchNumMessages"];
            }
        }

        [ConfigurationProperty(
           "serializer",
           DefaultValue = AsyncProducerConfig.DefaultSerializerClass,
           IsRequired = false)]
        public string Serializer
        {
            get
            {
                return (string)this["serializer"];
            }
        }

        [ConfigurationProperty(
           "keySerializer",
           DefaultValue = AsyncProducerConfig.DefaultKeySerializerClass,
           IsRequired = false)]
        public string KeySerializer
        {
            get
            {
                return (string)(this["keySerializer"] ?? this["serializer"]);
            }
        }

        [ConfigurationProperty(
           "sendBufferBytes",
           DefaultValue = SyncProducerConfig.DefaultSendBufferBytes,
           IsRequired = false)]
        public int SendBufferBytes
        {
            get
            {
                return (int)this["sendBufferBytes"];
            }
        }

        [ConfigurationProperty(
           "clientId",
           DefaultValue = SyncProducerConfig.DefaultClientId,
           IsRequired = false)]
        public string ClientId
        {
            get
            {
                return (string)this["clientId"];
            }
        }

        [ConfigurationProperty(
           "requestRequiredAcks",
           DefaultValue = SyncProducerConfig.DefaultRequiredAcks,
           IsRequired = false)]
        public short RequestRequiredAcks
        {
            get
            {
                return (short)this["requestRequiredAcks"];
            }
        }

        [ConfigurationProperty(
           "requestTimeoutMs",
           DefaultValue = SyncProducerConfig.DefaultAckTimeout,
           IsRequired = false)]
        public int RequestTimeoutMs
        {
            get
            {
                return (int)this["requestTimeoutMs"];
            }
        }

        public static ProducerConfigurationSection FromXml(XElement xml)
        {
            var config = new ProducerConfigurationSection();
            config.DeserializeSection(xml.CreateReader());
            return config;
        }
    }
}
