﻿using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Newtonsoft.Json;
using telemetry_device.compactCollection;
using telemetry_device.Settings;
using telemetry_device.Statistics.CompactCollection;
using telemetry_device_main;
using telemetry_device_main.Enums;

namespace telemetry_device
{
    class KafkaConnection
    {
        private readonly IProducer<Null, string> _producer ;
        private readonly IAdminClient _adminClient;
        private readonly TelemetryLogger _logger;

        public KafkaConnection()
        {
            ConfigProvider configProvider = ConfigProvider.Instance;
            TelemetryDeviceSettings telemetryDeviceSettings = configProvider.ProvideTelemetrySettings();
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
            while (true)
            {
                try
                {
                    _adminClient.GetMetadata(TimeSpan.FromSeconds(Consts.TIMEOUT));
                    return;
                }
                catch(KafkaException e)
                {
                    _logger.LogFatal("Tried connecting to kafka -"+e.Message,LogId.FatalKafkaConnection);
                }
                catch(Exception e) 
                {
                    _logger.LogFatal("Tried connecting to kafka -" + e.Message, LogId.FatalKafkaConnection);
                }
            }
        }

        public void SendFrameToKafka(string topicName, Dictionary<string,(int,bool)> paramDict)
        {
            string jsonString = JsonConvert.SerializeObject(paramDict);
            SendToKafka(jsonString,topicName);
        }

        public void SendStatisticsToKafka(Dictionary<StatisticDictionaryKey,StatisticsDictionaryValue> metricDict)
        {
            string jsonString = JsonConvert.SerializeObject(metricDict);
            SendToKafka(jsonString, Consts.STATISTIC_TOPIC);
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
