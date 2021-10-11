using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Fibonacci.MQ
{
    /// <summary>
    /// This singleton class holds microservice options
    /// options are supplied form EnvironmentVariables and appsettings.json file
    /// </summary>
    public class FibonacciMqOptions
    {
        /// <summary>
        /// RabbitMQ connection string
        /// </summary>
        public string RmqConnectionString { get; private set; }


        /// <summary>
        /// Fibonacci Web Api connection string
        /// </summary>
        public string FibonacciRestUri { get; private set; }


        private readonly IConfiguration _configuration;
        private static FibonacciMqOptions _instance;

        /// <summary>
        /// Get <see cref="FibonacciMqOptions"/> instance
        /// </summary>
        /// <returns></returns>
        public static FibonacciMqOptions Get(IConfiguration configuration)
        {
            // TODO: should make Get method thread-safe
            if (_instance != null) return _instance;

            _instance = new FibonacciMqOptions(configuration);
            
            _instance.RmqConnectionString = Environment.GetEnvironmentVariable("RMQ_CONNECTION_STRING");

            _instance.FibonacciRestUri = Environment.GetEnvironmentVariable("FIBONACCI_REST_URI");
            
            return _instance;
        }

        private FibonacciMqOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
