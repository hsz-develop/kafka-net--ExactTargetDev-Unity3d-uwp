﻿namespace Kafka.Tests.Zk
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using Kafka.Client.Utils;
    using Kafka.Client.ZKClient;
    using Kafka.Tests.Custom.Server;
    using Kafka.Tests.Utils;

    public abstract class ZooKeeperTestHarness : IDisposable
    {
        public string ZkConnect { get; private set; }

        public Process Zookeeper { get; private set; }

        public ZkClient ZkClient { get; private set; }

        private TempZookeeperConfig zkConfig;

        public const int ZkConnectionTimeout = 6000;

        public const int ZkSessionTimeout = 6000;

        protected ZooKeeperTestHarness()
        {
            this.zkConfig = new TempZookeeperConfig(TestZkUtils.ZookeeperPort);
            this.ZkConnect = TestZkUtils.ZookeeperConnect;
            this.Zookeeper = KafkaRunClassHelper.Run(KafkaRunClassHelper.ZookeeperMainClass, this.zkConfig.ConfigLocation); 
            this.ZkClient = new ZkClient(this.ZkConnect, ZkSessionTimeout, ZkConnectionTimeout, new ZkStringSerializer());
        }

        public virtual void Dispose()
        {
            try
            {
                ZkClient.Dispose();
            }
            catch
            {
            }
            try
            {
                using (this.Zookeeper)
                {
                    this.Zookeeper.Kill();
                    SpinWait.SpinUntil(() => this.Zookeeper.HasExited, 5000);
                }
            }
            catch
            {
            }

            try
            {
                if (this.zkConfig != null)
                {
                    this.zkConfig.Dispose();
                }
            }
            catch
            {
            }
            TestZkUtils.ResetPorts();
        }
    }
}