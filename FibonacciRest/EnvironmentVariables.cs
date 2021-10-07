using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FibonacciRest
{
    public class EnvironmentVariables
    {
        /// <summary>
        /// Count of concurrent workers
        /// </summary>
        public int WorkersCount { get; private set; }

        /// <summary>
        /// RabbitMQ connection string
        /// </summary>
        public string RmqConnectionString { get; private set; }


        private static EnvironmentVariables _instance;

        /// <summary>
        /// Get EnvironmentVariables instance
        /// </summary>
        /// <returns></returns>
        public static EnvironmentVariables Get()
        {
            // TODO: should make Get method thread-safe
            if (_instance != null) return _instance;
            
            _instance = new EnvironmentVariables();

            var workersCount = Environment.GetEnvironmentVariable("WORKERS_COUNT");
            _instance.WorkersCount = workersCount != null ? int.Parse(workersCount) : 0;

            _instance.RmqConnectionString = Environment.GetEnvironmentVariable("RMQ_CONNECTION_STRING");

            return _instance;
        }

        private EnvironmentVariables()
        {

        }
    }
}
