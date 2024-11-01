using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.Net;
using Newtonsoft.Json;
using telemetry_device.compactCollection;

namespace telemetry_device
{
    class KafkaConnection
    {
        private const string STATISTIC_TOPIC = "TelemetryStatistics";
        private IProducer<Null, string> _producer ;
        private IAdminClient _adminClient;
        private TelemetryLogger _logger;
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
        public void SendFrameToKafka(string topicName, Dictionary<string,(int,bool)> paramDict)
        {
            string jsonString = JsonConvert.SerializeObject(paramDict);
            Message<Null,string> message = new Message<Null, string>
            {
                Value = jsonString
            };
            _producer.Produce(topicName, message);
        }
        public void SendStatisticsToKafka(Dictionary<string,float> metricDict)
        {
            string jsonString = JsonConvert.SerializeObject(metricDict);
            Message<Null, string> message = new Message<Null, string>
            {
                Value = jsonString
            };
            _producer.Produce(STATISTIC_TOPIC, message);

        }
        public void WaitForKafkaConnection()
        {

            while (true)
            {
                try
                {
                    _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
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

    }
}
