﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fibonacci.Common;
using Fibonacci.Common.Model;
using Fibonacci.Common.Extensions;
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
            _distributedCache = distributedCache.ToKeyPrefixed("fibonacci_");
        }

        
        [HttpPost]
        public async Task<ActionResult> PostAsync(FibonacciData data, CancellationToken cancellationToken = new CancellationToken())
        {
            //TODO: прочитать про CancellationToken cancellationToken
            //TODO: Прочитать статью https://habr.com/ru/post/482354/
            
            var x = await _distributedCache.GetAsync(data.SessionId, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation($"Received Post {data.SessionId.ToString()} {data.ToString()}");


            return Ok();
        }
    }
}
