﻿namespace Kafka.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Kafka.Client.Common;
    using Kafka.Client.Utils;
    using Kafka.Tests.Custom.Server;
    using Kafka.Tests.Utils;
    using Kafka.Tests.Zk;

    using Xunit;

    public abstract class KafkaServerTestHarness : ZooKeeperTestHarness
    {
        protected List<TempKafkaConfig> Configs { get; private set; }

        protected abstract List<TempKafkaConfig> CreateConfigs();

        protected List<Process> Servers { get; private set; } 

        protected KafkaServerTestHarness()
        {
            this.Configs = this.CreateConfigs();
            if (this.Configs.Count <= 0)
            {
                throw new KafkaException("Must suply at least one server config.");
            }

            this.Servers = this.Configs.Select(this.StartServer).ToList();
            this.WaitForServersToSettle();
        }

        private Process StartServer(TempKafkaConfig config)
        {
            return KafkaRunClassHelper.Run(KafkaRunClassHelper.KafkaServerMainClass, config.ConfigLocation);
        }

        public void WaitForServersToSettle()
        {
            foreach (TempKafkaConfig config in this.Configs)
            {
                if (!ZkClient.WaitUntilExists(ZkUtils.BrokerIdsPath + "/" + config.BrokerId, TimeSpan.FromSeconds(5)))
                {
                    throw new Exception("Timeout on waiting for broker " + config.BrokerId + " to settle");
                }
            }
        }

        public override void Dispose()
        {
            foreach (var process in this.Servers)
            {
                using (process)
                {
                    process.Kill();
                    process.WaitForExit(5000);
                    Assert.True(process.HasExited);
                }
            }

            foreach (var serverConfig in this.Configs)
            {
                serverConfig.Dispose();
            }

            base.Dispose();
        }
    }
}