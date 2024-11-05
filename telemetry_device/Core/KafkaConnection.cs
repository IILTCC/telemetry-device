using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Newtonsoft.Json;
using telemetry_device.compactCollection;

namespace telemetry_device
{
    class KafkaConnection
    {
        private const string STATISTIC_TOPIC = "TelemetryStatistics";
        private readonly IProducer<Null, string> _producer ;
        private readonly IAdminClient _adminClient;
        private readonly TelemetryLogger _logger;

        public KafkaConnection(TelemetryDeviceSettings telemetryDeviceSettings)
        {
            _logger = TelemetryLogger.Instance;
            ProducerConfig producerConfig = new ProducerConfig
            {
                BootstrapServers = telemetryDeviceSettings.KafkaUrl
            };
            AdminClientConfig adminConfig = new AdminClientConfig
            {
                BootstrapServers = telemetryDeviceSettings.KafkaUrl
            };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _adminClient = new AdminClientBuilder(adminConfig).Build();
        }

        public void WaitForKafkaConnection()
        {
            const int TIMEOUT = 5;
            while (true)
            {
                try
                {
                    _adminClient.GetMetadata(TimeSpan.FromSeconds(TIMEOUT));
                    return;
                }
                catch(KafkaException e)
                {
                    _logger.LogFatal("Tried connecting to kafka -"+e.Message);
                }
                catch(Exception e) 
                {
                    _logger.LogFatal("Tried connecting to kafka -" + e.Message);
                }
            }
        }

        public void SendFrameToKafka(string topicName, Dictionary<string,(int,bool)> paramDict)
        {
            string jsonString = JsonConvert.SerializeObject(paramDict);
            SendToKafka(jsonString,topicName);
        }

        public void SendStatisticsToKafka(Dictionary<StatisticDictionaryKey,float> metricDict)
        {
            string jsonString = JsonConvert.SerializeObject(metricDict);
            SendToKafka(jsonString, STATISTIC_TOPIC);
        }

        private void SendToKafka(string jsonString,string topicName)
        {
            Message<Null, string> message = new Message<Null, string>
            {
                Value = jsonString
            };
            _producer.Produce(topicName, message);
        }


    }
}
