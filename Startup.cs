using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using System.Net;

namespace DotnetDocker
{
    public static class ServicesExtensions
    {
        private static bool IsIpAddress(string host)
        {
            string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            return Regex.IsMatch(host, ipPattern);
        }
        public static IServiceCollection AddRedisMultiplexer(
            this IServiceCollection services)
        {
            ConfigurationOptions config = ConfigurationOptions.Parse("redis");

            DnsEndPoint addressEndpoint = config.EndPoints.First() as DnsEndPoint;
            int port = addressEndpoint.Port;

            bool isIp = IsIpAddress(addressEndpoint.Host);
            if (!isIp)
            {
                //Please Don't use this line in blocking context. Please remove ".Result"
                //Just for test purposes
                IPHostEntry ip = Dns.GetHostEntryAsync(addressEndpoint.Host).Result;
                config.EndPoints.Remove(addressEndpoint);
                config.EndPoints.Add(ip.AddressList.First(), port);
            }

            // The Redis is a singleton, shared as much as possible.
            return services.AddSingleton<IConnectionMultiplexer>(
                provider => ConnectionMultiplexer.Connect(config)
            );
        }
    }

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddRedisMultiplexer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
