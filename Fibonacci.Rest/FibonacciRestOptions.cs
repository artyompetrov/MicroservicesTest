using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Fibonacci.Rest
{
    /// <summary>
    /// This singleton class holds microservice options
    /// options are supplied form EnvironmentVariables and appsettings.json file
    /// </summary>
    public class FibonacciRestOptions
    {
        /// <summary>
        /// Count of concurrent workers
        /// </summary>
        public int WorkersCount { get; private set; }

        /// <summary>
        /// RabbitMQ connection string
        /// </summary>
        public string RmqConnectionString { get; private set; }



        private readonly IConfiguration _configuration;
        private static FibonacciRestOptions _instance;

        /// <summary>
        /// Get <see cref="FibonacciRestOptions"/> instance
        /// </summary>
        /// <returns></returns>
        public static FibonacciRestOptions Get(IConfiguration configuration)
        {
            // TODO: should make Get method thread-safe
            if (_instance != null) return _instance;
            
            _instance = new FibonacciRestOptions(configuration);

            var workersCount = Environment.GetEnvironmentVariable("WORKERS_COUNT");
            _instance.WorkersCount = workersCount != null ? int.Parse(workersCount) : 1;

            _instance.RmqConnectionString = Environment.GetEnvironmentVariable("RMQ_CONNECTION_STRING");

            return _instance;
        }

        private FibonacciRestOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
