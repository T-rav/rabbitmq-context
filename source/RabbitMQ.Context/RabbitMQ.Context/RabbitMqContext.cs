﻿using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using StoneAge.System.Utils.Json;

namespace RabbitMQ.Context
{
    public class RabbitMqContext : IRabbitMqContext
    {
        private readonly IConnectionFactory _connectionFactory;

        public RabbitMqContext(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void DeclareQueue(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    DeclareQueue(name, channel);
                }
            }
        }

        public void PublishMessage(string queueName, object message)
        {
            PublishMessage(queueName, string.Empty, message);
        }

        public void PublishMessage(string queueName, string exchange, object message)
        {
            if (string.IsNullOrWhiteSpace(queueName)) return;

            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    DeclareQueue(queueName, channel);

                    var body = Encoding.UTF8.GetBytes(message.Serialize());
                    var properties = Create_DurableMessage_Properties(channel);

                    channel.BasicPublish(exchange,
                        queueName,
                        properties,
                        body);
                }
            }
        }

        public async Task ConsumeMessage(string queueName, Func<byte[], Task<bool>> action)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    DeclareQueue(queueName, channel);
                    Limit_Prefetch_To(1, channel); // todo : config prefetch
                    var basicConsumer = RegisterBasicConsumer(queueName, channel);
                    await ProcessMessage(action, basicConsumer, channel);
                }
            }
        }

        private static void Limit_Prefetch_To(ushort prefetchCount, IModel channel)
        {
            channel.BasicQos(0, prefetchCount, false);
        }

#pragma warning disable 618
        private QueueingBasicConsumer RegisterBasicConsumer(string queueName, IModel channel)

        {
            var basicConsumer = MakeConsumer(channel);
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: basicConsumer);

            return basicConsumer;
        }

        private async Task ProcessMessage(Func<byte[], Task<bool>> action, QueueingBasicConsumer basicConsumer,
            IModel channel)
        {
            var ea = basicConsumer.Queue.Dequeue();
            var body = ea.Body;

            var result = await action.Invoke(body);

            if (result)
            {
                channel.BasicAck(ea.DeliveryTag, false);
            }
        }

        private void DeclareQueue(string name, IModel channel)
        {
            channel.QueueDeclare(name,
                true,
                false,
                false,
                null);
        }

        private QueueingBasicConsumer MakeConsumer(IModel channel)
        {
            var basicConsumer = new QueueingBasicConsumer(channel);
            return basicConsumer;
        }
#pragma warning restore 618

        private static IBasicProperties Create_DurableMessage_Properties(IModel channel)
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // todo : put into config
            return properties;
        }
    }
}