﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitMQ.Context
{
    public interface IRabbitMqContext
    {
        void DeclareQueue(string name);
        void PublishMessage(string queueName, object message);
        void PublishMessage(string queueName, string exchange, object message);
        Task ConsumeMessage(string queueName, Func<byte[], Task<bool>> action);
        Task BatchConsumeMessage(string queueName, ushort batchSize, Func<List<byte[]>, Task<bool>> action);
    }
}