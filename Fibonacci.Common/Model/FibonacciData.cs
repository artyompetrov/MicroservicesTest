using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fibonacci.Common.Model
{
    public class FibonacciData
    {
        [System.Text.Json.Serialization.JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("niValue")]
        public int NiValue { get; set; }
    }
}
