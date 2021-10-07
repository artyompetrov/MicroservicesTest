﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FibonacciRest
{
    /// <summary>
    /// This singleton class holds microservice options
    /// options are supplied form EnvironmentVariables and appsettings.json file
    /// </summary>
    public class Options
    {
        private readonly IConfiguration _configuration;
        private static Options _instance;

        /// <summary>
        /// Count of concurrent workers
        /// </summary>
        public int WorkersCount { get; private set; }

        /// <summary>
        /// RabbitMQ connection string
        /// </summary>
        public string RmqConnectionString { get; private set; }
        
        /// <summary>
        /// Get Options instance
        /// </summary>
        /// <returns></returns>
        public static Options Get(IConfiguration configuration)
        {
            // TODO: should make Get method thread-safe
            if (_instance != null) return _instance;
            
            _instance = new Options(configuration);
#if DEBUG
            _instance.WorkersCount = 1;

            _instance.RmqConnectionString = "host=172.17.0.3";
#else
            var workersCount = Environment.GetEnvironmentVariable("WORKERS_COUNT");
            _instance.WorkersCount = workersCount != null ? int.Parse(workersCount) : 0;

            _instance.RmqConnectionString = Environment.GetEnvironmentVariable("RMQ_CONNECTION_STRING");
#endif

            return _instance;
        }

        private Options(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}