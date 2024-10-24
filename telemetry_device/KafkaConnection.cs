using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.Net;
using Newtonsoft.Json;

namespace telemetry_device
{
    class KafkaConnection
    {
        private const string KAFKA_URL = "localhost:9092";
        private IProducer<Null, string> _producer ;
        private IAdminClient _adminClient;

        public KafkaConnection()
        {
            ProducerConfig producerConfig = new ProducerConfig
            {
                BootstrapServers = KAFKA_URL
            };
            AdminClientConfig adminConfig = new AdminClientConfig
            {
                BootstrapServers = KAFKA_URL
            };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _adminClient = new AdminClientBuilder(adminConfig).Build();
        }
        public void SendToTopic(string topicName, Dictionary<string,(int,bool)> paramDict)
        {
            string jsonString = JsonConvert.SerializeObject(paramDict);
            Message<Null,string> message = new Message<Null, string>
            {
                Value = jsonString
            };
            _producer.Produce(topicName, message);

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
                { }
                catch(Exception e) 
                { }
            }
        }

    }
}
