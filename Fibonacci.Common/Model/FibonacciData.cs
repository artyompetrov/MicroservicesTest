namespace Fibonacci.Common.Model
{
    public class FibonacciData
    {
        [System.Text.Json.Serialization.JsonPropertyName("sessionId")]
        public string SessionId { get; set; }


        //TODO: Int32 will overflow very fast, replace Int32 with BigNumber 
        [System.Text.Json.Serialization.JsonPropertyName("niValue")]
        public int NiValue { get; set; }
    }
}
