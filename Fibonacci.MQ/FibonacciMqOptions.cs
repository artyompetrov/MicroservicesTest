using Microsoft.Extensions.Configuration;
using System;

namespace Fibonacci.MQ
{
    /// <summary>
    /// This singleton class holds microservice options
    /// options are supplied form EnvironmentVariables and appsettings.json file
    /// </summary>
    public class FibonacciMqOptions
    {
        /// <summary>
        /// Count of concurrent workers
        /// </summary>
        public int WorkersCount { get; private set; }

        /// <summary>
        /// RabbitMQ connection string
        /// </summary>
        public string RmqConnectionString { get; private set; }


        /// <summary>
        /// Fibonacci.Rest WebApi connection string
        /// </summary>
        public string FibonacciRestUri { get; private set; }

        #region InitializationAndPrivateFields
        
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

            var workersCount = Environment.GetEnvironmentVariable("WORKERS_COUNT");
            _instance.WorkersCount = workersCount != null ? int.Parse(workersCount) : 1;

            return _instance;
        }

        private FibonacciMqOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion
    }
}
