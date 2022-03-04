﻿using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHost();

            await host.RunAsync();
        }

        public static IHost CreateHost()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            return new HostBuilder()
                .ConfigureServices((hostContext, services) => { ConfigureServices(services); })
                .UseConsoleLifetime()
                .UseSerilog()
                .Build();
        }

        static void ConfigureServices(IServiceCollection collection)
        {
            collection.AddMassTransit(x =>
            {
                x.AddConsumer<TestConsumer>(c => c.UseConcurrentMessageLimit(1));

                x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", 5672, "/", h =>
                        {
                            h.Username("admin");
                            h.Password("admin");
                        });

                        cfg.Message<TestMessage>(c => c.SetEntityName("test"));
                        EndpointConvention.Map<TestMessage>(new Uri("rabbitmq://localhost/%2F/test"));

                        cfg.ReceiveEndpoint("test", configurator =>
                        {
                            configurator.PrefetchCount = 1;
                            configurator.ConfigureConsumer<TestConsumer>(context);
                        });
                    }
                );
            });

            // before start create topology like test(exchange)->test(queue)
            // at rabbitmq
            collection.AddHostedService<Sender>();
        }
    }
}