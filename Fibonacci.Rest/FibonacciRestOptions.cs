using Microsoft.Extensions.Configuration;
using System;

namespace Fibonacci.Rest
{
    /// <summary>
    /// This singleton class holds microservice options
    /// options are supplied form EnvironmentVariables and appsettings.json file
    /// </summary>
    public class FibonacciRestOptions
    {
        /// <summary>
        /// RabbitMQ connection string
        /// </summary>
        public string RmqConnectionString { get; private set; }

        #region InitializationAndPrivateFields

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


            _instance.RmqConnectionString = Environment.GetEnvironmentVariable("RMQ_CONNECTION_STRING");

            return _instance;
        }

        private FibonacciRestOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

    }
}
