using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
                    Limit_Prefetch_To(250, channel);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (sender, args) =>
                    {
                        var body = args.Body;

                        var result = await action.Invoke(body);

                        if (result)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                        }
                    };
                    channel.BasicConsume(queueName, false, consumer);
                }
            }
        }

        private static void Limit_Prefetch_To(ushort prefetchCount, IModel channel)
        {
            channel.BasicQos(0, prefetchCount, false);
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
