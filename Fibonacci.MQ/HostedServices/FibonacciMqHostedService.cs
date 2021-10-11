using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Fibonacci.Common.Extensions;
using Fibonacci.Common.Model;
using Fibonacci.MQ.ServiceReferences;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fibonacci.MQ.HostedServices
{
    public class FibonacciMqHostedService : BackgroundService
    {
        private readonly IBus _bus;
        private readonly ILogger<FibonacciMqHostedService> _logger;
        private readonly IDistributedCache _distributedCache;
        private readonly FibonacciMqOptions _options;

        public FibonacciMqHostedService(IBus bus, ILogger<FibonacciMqHostedService> logger, 
            IDistributedCache distributedCache, FibonacciMqOptions options)
        {
            _bus = bus;
            _logger = logger;
            _distributedCache = distributedCache.ToKeyPrefixed("FibonacciMqHostedService_");
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bus.PubSub.SubscribeAsync<FibonacciData>(nameof(FibonacciMqHostedService),
                FibonacciDataReceived, FibonacciDataConfigure, stoppingToken);
        }

        private void FibonacciDataConfigure(ISubscriptionConfiguration obj)
        {
        }

        //TODO: CancellationToken
        private async Task FibonacciDataReceived(FibonacciData data, CancellationToken token)
        {
            _logger.LogInformation($"Received {nameof(FibonacciData)} via MQ API with " +
                               $"{nameof(FibonacciData.SessionId)} = {data.SessionId};" +
                               $"{nameof(FibonacciData.NiValue)} = {data.NiValue}");

            var sessionState = await _distributedCache
                .GetFromJsonOrCreateAsync<SessionState>(data.SessionId)
                .ConfigureAwait(false);

            var currentValue = sessionState.NPreviousValue + data.NiValue;

            var answerData = new FibonacciData()
            {
                SessionId = data.SessionId,
                NiValue = currentValue
            };

            using var httpClient = new HttpClient();
            var fibonacciRestClient = new FibonacciRestClient(_options.FibonacciRestUri, httpClient);

            await fibonacciRestClient.FibonacciAsync(answerData)
                .ConfigureAwait(false);

            sessionState.NPreviousValue = currentValue;
            await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState)
                .ConfigureAwait(false);

        }
    }
}
