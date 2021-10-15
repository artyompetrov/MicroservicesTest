using System;
using System.Threading;
using Fibonacci.Common.Extensions;
using Fibonacci.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Fibonacci.Common;
using EasyNetQ;
using Microsoft.AspNetCore.Http;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(string))]
        public async Task<ActionResult> PostAsync([FromBody] FibonacciData data)
        {
            
            _logger.LogInformation($"Received {nameof(FibonacciData)} via WebApi with " +
                               $"{nameof(FibonacciData.SessionId)} = {data.SessionId}; " +
                               $"{nameof(FibonacciData.NiValue)} = {data.NiValue}");

            var sessionState = await _distributedCache
                .GetFromJsonAsync<SessionState>(data.SessionId);

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


                await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState);

                _logger.LogInformation($"Session {data.SessionId} initialized");

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

                var messageBusAnswerData = new FibonacciData
                {
                    SessionId = data.SessionId,
                    NiValue = currentValue
                };

                sessionState.NPreviousValue = currentValue;
                await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState);

                await _bus.PubSub.PublishAsync(messageBusAnswerData);
                
                return Ok();
            }
            catch (OverflowException)
            {
                sessionState.Overflow = true;

                await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState);

                var message = $"Session {data.SessionId} overflowed";

                _logger.LogInformation(message);
                

                return UnprocessableEntity(message);
            }
        }
    }
}
