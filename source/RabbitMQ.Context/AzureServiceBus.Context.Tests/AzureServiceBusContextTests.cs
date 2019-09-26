using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace AzureServiceBus.Context.Tests
{
    [TestFixture]
    public class AzureServiceBusContextTests
    {
        [TestFixture]
        class DeclareQueue
        {
            [Test]
            public void WhenPassedValidQueueName_ShouldCreateQueue()
            {
                // arrange
                var queueName = "test-queue";

                var client = Substitute.For<IClient>();
                var connection = Substitute.For<IConnection>();
                connection.CreateClient().Returns(client);
                var connectionFactory = Substitute.For<IConnectionFactory>();
                connectionFactory.CreateConnection().Returns(connection);

                var mqContext = new AzureServiceBusContext(connectionFactory);
                // act
                mqContext.DeclareQueue(queueName);
                // assert
                //channel.Received(1).QueueDeclare(Arg.Is<string>(s => s == queueName),
                    //Arg.Is<bool>(b => b),
                    //Arg.Is<bool>(b => !b),
                    //Arg.Is<bool>(b => !b),
                    //Arg.Is<IDictionary<string, object>>(d => d == null));
            }
        }
        
    }

    public class AzureServiceBusContext : IAzureServiceBusContext
    {
        public AzureServiceBusContext(IConnectionFactory connectionFactory)
        {
            throw new NotImplementedException();
        }

        public void PublishMessage(string queueName, object message)
        {
            throw new NotImplementedException();
        }

        public Task ConsumeMessage(string queueName, Func<byte[], Task<bool>> action)
        {
            throw new NotImplementedException();
        }
    }

    public interface IAzureServiceBusContext
    {
        void PublishMessage(string queueName, object message);
        Task ConsumeMessage(string queueName, Func<byte[], Task<bool>> action);
    }

    public interface IConnectionFactory
    {
        IConnection CreateConnection();
    }

    public interface IConnection
    {
        IClient CreateClient();
    }

    public interface IClient
    {
        Task Send();
    }

    /*
    [TestFixture]
    public class DeclareQueue
    {
        [Test]
        public void WhenPassedValidQueueName_ShouldCreateQueue()
        {
            // arrange
            var queueName = "test-queue";

            var channel = Substitute.For<IModel>();
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(channel);
            var connectionFactory = Substitute.For<IConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);

            var rabbitMqContext = new RabbitMqContext(connectionFactory);
            // act
            rabbitMqContext.DeclareQueue(queueName);
            // assert
            channel.Received(1).QueueDeclare(Arg.Is<string>(s => s == queueName),
                Arg.Is<bool>(b => b),
                Arg.Is<bool>(b => !b),
                Arg.Is<bool>(b => !b),
                Arg.Is<IDictionary<string, object>>(d => d == null));
        }

        [TestCase(" ")]
        [TestCase("")]
        [TestCase(null)]
        public void WhenPassedWhitespaceQueueName_ShouldNotCreateQueue(string queueName)
        {
            // arrange
            var channel = Substitute.For<IModel>();
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(channel);
            var connectionFactory = Substitute.For<IConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);

            var rabbitMqContext = new RabbitMqContext(connectionFactory);
            // act
            rabbitMqContext.DeclareQueue(queueName);
            // assert
            channel.DidNotReceive().QueueDeclare(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>());
        }
    }

    [TestFixture]
    public class PublishMessage
    {
        [Test]
        public void WhenQueueNameNotWhitespace_ShouldPublishMessage()
        {
            // arrange
            var queueName = "test-queue";
            var message = new EmailMessage
            {
                To = "bob@smith.com",
                From = "jane@doe.com",
                Subject = "Test",
                Message = "Hello!"
            };

            var channel = Substitute.For<IModel>();
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(channel);
            var connectionFactory = Substitute.For<IConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);

            var rabbitMqContext = new RabbitMqContext(connectionFactory);
            // act
            rabbitMqContext.PublishMessage(queueName, message);
            // assert
            channel.Received(1).BasicPublish(Arg.Is<string>(s => s == string.Empty),
                Arg.Is<string>(s => s == queueName),
                Arg.Is<bool>(b => !b),
                Arg.Any<IBasicProperties>(),
                Arg.Any<byte[]>());
        }

        [Test]
        public void WhenQueueNameNotWhitespace_ShouldDeclareQueue()
        {
            // arrange
            var queueName = "test-queue";
            var message = new EmailMessage
            {
                To = "bob@smith.com",
                From = "jane@doe.com",
                Subject = "Test",
                Message = "Hello!"
            };

            var channel = Substitute.For<IModel>();
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(channel);
            var connectionFactory = Substitute.For<IConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);

            var rabbitMqContext = new RabbitMqContext(connectionFactory);
            // act
            rabbitMqContext.PublishMessage(queueName, message);
            // assert
            channel.Received(1).QueueDeclare(Arg.Is<string>(s => s == queueName),
                Arg.Is<bool>(s => s),
                Arg.Is<bool>(s => !s),
                Arg.Is<bool>(s => !s),
                null);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void WhenQueueNameWhitespace_ShouldNotPublishMessage(string queueName)
        {
            // arrange
            var message = new EmailMessage
            {
                To = "bob@smith.com",
                From = "jane@doe.com",
                Subject = "Test",
                Message = "Hello!"
            };

            var channel = Substitute.For<IModel>();
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(channel);
            var connectionFactory = Substitute.For<IConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);

            var rabbitMqContext = new RabbitMqContext(connectionFactory);
            // act
            rabbitMqContext.PublishMessage(queueName, message);
            // assert
            channel.DidNotReceive().BasicPublish(Arg.Is<string>(s => s == string.Empty),
                Arg.Is<string>(s => s == "general"),
                Arg.Is<bool>(b => !b),
                Arg.Is<BasicProperties>(p => p == null),
                Arg.Any<byte[]>());
        }
    }

    [TestFixture]
    public class ConsumeMessage
    {
        [Test]
        public async Task WhenProcessingOfMessageSuccessful_ShouldConsumeMessage()
        {
            // arrange

            var queueName = "test-queue";
            var recievedBytes = new byte[0];
            Func<byte[], Task<bool>> action = (byte[] bytes) =>
            {
                recievedBytes = bytes;
                return Task.FromResult(true);
            };

            var rabbitMqContext = new RabbitMqTestContextBuilder()
                .With_Queue(queueName)
                .Build();

            rabbitMqContext.PublishMessage(queueName, "hello_world");
            // act
            await rabbitMqContext.ConsumeMessage(queueName, action);
            // assert
            var expectedLength = 13; // len of 'hello_world'
            recievedBytes.Length.Should().Be(expectedLength);
        }

        [Test]
        public async Task WhenProcessingOfMessageSuccessful_ShouldAckMessage()
        {
            // arrange

            var queueName = "test-queue";
            Func<byte[], Task<bool>> action = (byte[] bytes) => Task.FromResult(true);

            var rabbitMqContext = new RabbitMqTestContextBuilder()
                .With_Queue(queueName)
                .Build();

            rabbitMqContext.PublishMessage(queueName, "hello_world");

            // act
            await rabbitMqContext.ConsumeMessage(queueName, action);
            // assert
            rabbitMqContext.Assert_Queue_Message_Count_Is(0);
        }

        [Test]
        public async Task WhenProcessingOfMessageErrors_ShouldNotAckMessage()
        {
            // arrange
            var queueName = "test-queue";
            Func<byte[], Task<bool>> action = (byte[] bytes) => Task.FromResult(false);

            var rabbitMqContext = new RabbitMqTestContextBuilder()
                .With_Queue(queueName)
                .Build();

            rabbitMqContext.PublishMessage(queueName, "hello_world");
            // act
            await rabbitMqContext.ConsumeMessage(queueName, action);
            // assert
            rabbitMqContext.Assert_Queue_Message_Count_Is(1);
        }

    }
    }
    */
}
