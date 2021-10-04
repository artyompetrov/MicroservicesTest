using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fibonacci.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace FibonacciRest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibonacciController : ControllerBase
    {
        private readonly ILogger<FibonacciController> _logger;
        private readonly IDistributedCache _distributedCache;



        public FibonacciController(ILogger<FibonacciController> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;
        }


        //TODO: прочитать про CancellationToken cancellationToken
        [HttpPost]
        public async Task<ActionResult> PostAsync(FibonacciData data, CancellationToken cancellationToken)
        {
#if DEBUG
            //TODO: Прочитать статью https://habr.com/ru/post/482354/
            var x = await _distributedCache.GetAsync("", cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation($"Received Post {data.SessionId.ToString()} {data.ToString()}");


#endif
            return Ok();
        }
    }
}
