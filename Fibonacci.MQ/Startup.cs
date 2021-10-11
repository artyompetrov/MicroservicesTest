using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Fibonacci.Common;
using Fibonacci.MQ.HostedServices;

namespace Fibonacci.MQ
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            
            KeyPrefixedCacheWrapper.CommonPrefix = "FibonacciMq_";
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var options = FibonacciMqOptions.Get(_configuration);
            services.AddSingleton(options);

            var rmqBus = RabbitHutch.CreateBus(options.RmqConnectionString);
            services.AddSingleton<IBus>(rmqBus);

            services.AddSingleton(rmqBus);

            // AddDistributedMemoryCache is used because we may need to use Redis (or smth similar) instead of MemoryCache in the future
            services.AddDistributedMemoryCache();

            services.AddHostedService<FibonacciMqHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
