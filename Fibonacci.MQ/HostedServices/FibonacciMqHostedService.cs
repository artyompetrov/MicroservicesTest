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
using Fibonacci.Common;
using RabbitMQ.Client.Exceptions;

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
            for (var attempt = 0; attempt < _options.BrokerConnectionAttempts; attempt++)
            {
                await Task.Delay(_options.AttemptsTimeout, stoppingToken)
                    .ConfigureAwait(false);

                try
                {
                    await _bus.PubSub.SubscribeAsync<FibonacciData>(nameof(FibonacciMqHostedService),
                            FibonacciDataReceivedAsync, FibonacciDataConfigure, stoppingToken)
                        .ConfigureAwait(false);

                    _logger.LogInformation($"Subscribed to {nameof(FibonacciData)} stream successfully");

                    break;
                }
                //TODO: remove high cohesion with RabbitMQ
                catch (BrokerUnreachableException e)
                {
                    _logger.LogWarning($"Unsuccessful Message bus subscribe attempt: {e.GetType()} {e.Message}");
                }
            }

            var runningTasks = new Task[_options.WorkersCount];

            for (var workerNumber = 0; workerNumber < _options.WorkersCount; workerNumber++)
            {
                var randomSessionId = Guid.NewGuid().ToString();

                var initialSessionState = new SessionState()
                {
                    NPreviousValue = 1
                };

                await _distributedCache
                    .SetAsJsonAsync(randomSessionId, initialSessionState, stoppingToken)
                    .ConfigureAwait(false);

                runningTasks[workerNumber] = SendFibonacciValueAsync(randomSessionId, 1, token: stoppingToken);
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
                               $"{nameof(FibonacciData.SessionId)} = {data.SessionId}; " +
                               $"{nameof(FibonacciData.NiValue)} = {data.NiValue.ToString()}");

            var sessionState = await _distributedCache
                .GetFromJsonAsync<SessionState>(data.SessionId, token)
                .ConfigureAwait(false);

            if (sessionState.Overflow)
            {
                _logger.LogInformation($"Session {data.SessionId} is already overflowed, " +
                                       $"cancelling message handling routine");
                return;
            }

            try
            {
                var currentValue = checked(sessionState.NPreviousValue + data.NiValue);

                await SendFibonacciValueAsync(data.SessionId, currentValue, token: token)
                    .ConfigureAwait(false);

                sessionState.NPreviousValue = currentValue;
            }
            catch (OverflowException)
            {
                _logger.LogInformation($"Session {data.SessionId} overflowed");

                sessionState.Overflow = true;
            }

            await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState, token)
                .ConfigureAwait(false);
        }

        private async Task SendFibonacciValueAsync(string sessionId, int value, int attempts = 5, CancellationToken token = default)
        {
            using var httpClient = new HttpClient();
            var fibonacciRestClient = new FibonacciRestClient(_options.FibonacciRestUri, httpClient);

            var answerData = new FibonacciData
            {
                SessionId = sessionId,
                NiValue = value
            };

            for (var attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    await fibonacciRestClient.FibonacciAsync(answerData, token)
                        .ConfigureAwait(false);

                    return;
                }
                catch (FibonacciRestApiException e)
                {
                    //TODO: add opposite side session overflow handling
                    _logger.LogWarning($"Not successful request attempt: {e.GetType()} {e.Message}");
                }

                await Task.Delay(_options.AttemptsTimeout, token)
                    .ConfigureAwait(false);
                
            }

            throw new FibonacciException("Unable to make a WebApi request, attempts run out");
        }
    }
}
