﻿namespace Kafka.Client.Consumers
{
    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using log4net;

    public abstract class TopicFilter
    {
        public string RawRegexp { get; private set; }

        public string Regex
        {
            get
            {
                return this.RawRegexp.Replace(",", "|").Replace(" ", string.Empty);
            }
        }

        protected TopicFilter(string rawRegexp)
        {
            this.RawRegexp = rawRegexp;
            try
            {
// ReSharper disable ObjectCreationAsStatement
                new Regex(this.RawRegexp);
// ReSharper restore ObjectCreationAsStatement
            }
            catch (Exception)
            {
                throw new Exception(rawRegexp + "is an invalid regex.");
            }
        }

        public override string ToString()
        {
            return string.Format("Regex: {0}", this.Regex);
        }

        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public abstract bool IsTopicAllowed(string topic);
    }

    public class Whitelist : TopicFilter
    {
        public Whitelist(string rawRegexp)
            : base(rawRegexp)
        {
        }

        public override bool IsTopicAllowed(string topic)
        {
            var allowed = new Regex(Regex).IsMatch(topic);

            Logger.DebugFormat("{0} {1}", topic, allowed ? "allowed" : "filtered");

            return allowed;
        }
    }

    public class Blacklist : TopicFilter
    {
        public Blacklist(string rawRegexp)
            : base(rawRegexp)
        {
        }

        public override bool IsTopicAllowed(string topic)
        {
            var allowed = !new Regex(RawRegexp).IsMatch(topic);

            Logger.DebugFormat("{0} {1}", topic, allowed ? "allowed" : "filtered");

            return allowed;
        }
    }
}