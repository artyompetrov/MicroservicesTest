using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Fibonacci.Common;
using Fibonacci.Common.Model;
using Fibonacci.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;


namespace FibonacciRest.Controllers
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
            _distributedCache = distributedCache.ToKeyPrefixed("fibonacci_");
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(FibonacciData data)
        {
            //TODO: прочитать про CancellationToken cancellationToken
            //TODO: Прочитать статью https://habr.com/ru/post/482354/

            var sessionState = await _distributedCache
                .GetFromJsonOrCreateAsync<SessionState>(data.SessionId)
                .ConfigureAwait(false);


            var currentValue = sessionState.NPreviousValue + data.NiValue;

            await _bus.PubSub.PublishAsync("Test")
                .ConfigureAwait(false);
            

            sessionState.NPreviousValue = currentValue;
            await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState)
                .ConfigureAwait(false);
            
            return Ok();
        }
    }
}
