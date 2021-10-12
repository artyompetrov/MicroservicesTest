using EasyNetQ;
using Fibonacci.Common.Extensions;
using Fibonacci.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


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
        public async Task<ActionResult> PostAsync(FibonacciData data /*token*/)
        {
            
            //TODO: read about https://habr.com/ru/post/482354/
            
            _logger.LogInformation($"Received {nameof(FibonacciData)} via WebApi with " +
                               $"{nameof(FibonacciData.SessionId)} = {data.SessionId};" +
                               $"{nameof(FibonacciData.NiValue)} = {data.NiValue.ToString()}");

            var sessionState = await _distributedCache
                .GetFromJsonOrCreateAsync<SessionState>(data.SessionId)
                .ConfigureAwait(false);
            
            var currentValue = sessionState.NPreviousValue + data.NiValue;

            var answerData = new FibonacciData()
            {
                SessionId = data.SessionId,
                NiValue = currentValue
            };
            
            await _bus.PubSub.PublishAsync(answerData)
                .ConfigureAwait(false);
            
            sessionState.NPreviousValue = currentValue;
            await _distributedCache.SetAsJsonAsync(data.SessionId, sessionState)
                .ConfigureAwait(false);
            
            return Ok();
        }
    }
}
