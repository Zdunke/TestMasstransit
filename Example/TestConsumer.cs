using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example
{
    public class TestConsumer : IConsumer<TestMessage>
    {
        private readonly ILogger _logger;

        public TestConsumer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Consumed");
        }

        public Task Consume(ConsumeContext<TestMessage> context)
        {
            var consumeCounter = Counter.IncrementConsume();
            Counter._counterList.Add(context.Message.Counter);
            try
            {
                if (context.Message.Counter != consumeCounter)
                {
                    //_logger.LogWarning("Counters do not match!!");

                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"{DateTime.Now} [{consumeCounter}] Consume : {context.Message}");
                    Console.ResetColor();

                    for (int i = 1; i <= context.Message.Counter; i++)
                    {
                        if (!Counter._counterList.Contains(i))
                        {
                            Console.WriteLine($"{DateTime.Now} Missing Message #{i}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} [{consumeCounter}] Consume : {context.Message}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Timestamp} Consume Exception ", DateTime.Now);
            }

            return Task.CompletedTask;
        }
    }
}