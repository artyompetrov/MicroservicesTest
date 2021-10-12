using EasyNetQ;
using Fibonacci.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;


namespace Fibonacci.Rest
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;

            KeyPrefixedCacheWrapper.CommonPrefix = "FibonacciRest_";
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var options = FibonacciRestOptions.Get(_configuration);
            services.AddSingleton(options);

            var rmqBus = RabbitHutch.CreateBus(options.RmqConnectionString);
            services.AddSingleton<IBus>(rmqBus);
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FibonacciRest", Version = "v1" });
            });

            // AddDistributedMemoryCache is used because we may need to use Redis (or smth similar) instead of MemoryCache in the future
            services.AddDistributedMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FibonacciRest v1"));
            }

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
