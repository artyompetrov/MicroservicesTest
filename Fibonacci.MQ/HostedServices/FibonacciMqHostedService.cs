using EasyNetQ;
using Fibonacci.Common.Extensions;
using Fibonacci.Common.Model;
using Fibonacci.MQ.ServiceReferences;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            _options = options;
            _distributedCache = distributedCache.ToKeyPrefixed("FibonacciMqHostedService_");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bus.PubSub.SubscribeAsync<FibonacciData>(nameof(FibonacciMqHostedService),
                FibonacciDataReceivedAsync, FibonacciDataConfigure, stoppingToken);


            var runningTasks = new Task[_options.WorkersCount];

            for (var workerNumber = 0; workerNumber < _options.WorkersCount; workerNumber++)
            {
                var randomSessionId = Guid.NewGuid().ToString();

                await _distributedCache
                    .GetFromJsonOrCreateAsync<SessionState>(randomSessionId)
                    .ConfigureAwait(false);

                runningTasks[workerNumber] = SendFibonacciValueAsync(randomSessionId, 1);
            }

            await Task.WhenAll(runningTasks)
                .ConfigureAwait(false);
        }

        private void FibonacciDataConfigure(ISubscriptionConfiguration obj)
        {
        }

        //TODO: read about CancellationToken
        private async Task FibonacciDataReceivedAsync(FibonacciData data, CancellationToken token)
        {
            _logger.LogInformation($"Received {nameof(FibonacciData)} via MQ API with " +
                               $"{nameof(FibonacciData.SessionId)} = {data.SessionId};" +
                               $"{nameof(FibonacciData.NiValue)} = {data.NiValue.ToString()}");

            var sessionState = await _distributedCache
                .GetFromJsonAsync<SessionState>(data.SessionId)
                .ConfigureAwait(false);

            var currentValue = sessionState.NPreviousValue + data.NiValue;
            
            await SendFibonacciValueAsync(data.SessionId, currentValue)
                .ConfigureAwait(false);

            sessionState.NPreviousValue = currentValue;
            await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState)
                .ConfigureAwait(false);

        }

        private Task SendFibonacciValueAsync(string sessionId, int value)
        {
            using var httpClient = new HttpClient();
            var fibonacciRestClient = new FibonacciRestClient(_options.FibonacciRestUri, httpClient);

            var answerData = new FibonacciData()
            {
                SessionId = sessionId,
                NiValue = value
            };

            //TODO: add http response and exceptions handling
            return fibonacciRestClient.FibonacciAsync(answerData);

        }
    }
}
