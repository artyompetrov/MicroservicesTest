using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Fibonacci.Common.Model;
using Microsoft.Extensions.Hosting;

namespace Fibonacci.MQ.HostedServices
{
    public class FibonacciHostedService : BackgroundService
    {
        private readonly IBus _bus;

        public FibonacciHostedService(IBus bus)
        {
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //TODO: resume here
            await _bus.PubSub.SubscribeAsync<FibonacciData>("", FibonacciDataReceived, FibonacciDataConfigure, stoppingToken);
        }

        private void FibonacciDataConfigure(ISubscriptionConfiguration obj)
        {
            throw new NotImplementedException();
        }

        private Task FibonacciDataReceived(FibonacciData arg1, CancellationToken arg2)
        {
            throw new NotImplementedException();
        }
    }
}
