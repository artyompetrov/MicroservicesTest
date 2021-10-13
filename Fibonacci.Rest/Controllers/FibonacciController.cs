using System;
using System.Threading;
using Fibonacci.Common.Extensions;
using Fibonacci.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Fibonacci.Common;
//TODO: remove high cohesion with RabbitMQ and EasyNetQ
using EasyNetQ;

namespace Fibonacci.Rest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibonacciController : ControllerBase
    {
        private readonly ILogger<FibonacciController> _logger;
        private readonly IBus _bus;
        private readonly IDistributedCache _distributedCache;
        

        public FibonacciController(ILogger<FibonacciController> logger, IDistributedCache distributedCache, IBus bus)
        {
            _logger = logger;
            _bus = bus;
            _distributedCache = distributedCache.ToKeyPrefixed("FibonacciController_");
        }

        [HttpPost]
        //TODO: read about CancellationToken
        public async Task<ActionResult> PostAsync(FibonacciData data, CancellationToken token)
        {
            //TODO: read about https://habr.com/ru/post/482354/
            
            _logger.LogInformation($"Received {nameof(FibonacciData)} via WebApi with " +
                               $"{nameof(FibonacciData.SessionId)} = {data.SessionId}; " +
                               $"{nameof(FibonacciData.NiValue)} = {data.NiValue.ToString()}");

            var sessionState = await _distributedCache
                .GetFromJsonAsync<SessionState>(data.SessionId, token)
                .ConfigureAwait(false);

            if (sessionState == null)
            {
                if (data.NiValue != 1)
                {
                    throw new FibonacciException($"Not found state in cache and {nameof(data.NiValue)} != 1");
                }

                sessionState = new SessionState
                {
                    NPreviousValue = 0
                };

                _logger.LogInformation($"Session {data.SessionId} initialized");

                await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState, token)
                    .ConfigureAwait(false);
            }

            if (sessionState.Overflow)
            {
                _logger.LogError($"Session {data.SessionId} is already overflowed, " +
                                       $"cancelling request handling routine");
                return UnprocessableEntity($"Session {data.SessionId} is already overflowed");
            }

            try
            {
                var currentValue = checked(sessionState.NPreviousValue + data.NiValue);

                var messageBusAnswerData = new FibonacciData()
                {
                    SessionId = data.SessionId,
                    NiValue = currentValue
                };

                await _bus.PubSub.PublishAsync(messageBusAnswerData, token)
                    .ConfigureAwait(false);

                sessionState.NPreviousValue = currentValue;
                await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState, token)
                    .ConfigureAwait(false);

                return Ok();
            }
            catch (OverflowException)
            {
                sessionState.Overflow = true;

                await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState, token)
                    .ConfigureAwait(false);

                var message = $"Session {data.SessionId} overflowed";

                _logger.LogInformation(message);

                return UnprocessableEntity(message);
            }
        }
    }
}
