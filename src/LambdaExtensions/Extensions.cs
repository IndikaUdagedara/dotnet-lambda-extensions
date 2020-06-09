using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace LambdaExtensions
{
    public static class Extensions
    {
        public static IHostBuilder UseLambdaLocal<TRequest, TResponse, TFunctionHandler>(this IHostBuilder hostBuilder, Action<IWebHostBuilder> configureWebHost = null)
            where TFunctionHandler : class, ILambdaFunctionHandler<TRequest, TResponse>
        {
            hostBuilder
                .UseLambda<TRequest, TResponse, TFunctionHandler>()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    configureWebHost?.Invoke(webHostBuilder);
                    webHostBuilder
                        .Configure(a => a.UseLambdaProxy<TRequest, TResponse>())
                        .UseKestrel();
                });
            return hostBuilder;
        }

        public static IHostBuilder UseLambda<TRequest, TResponse, TFunctionHandler>(this IHostBuilder hostBuilder)
            where TFunctionHandler : class, ILambdaFunctionHandler<TRequest, TResponse>
        {
            hostBuilder.ConfigureServices(c =>
                c.AddScoped<ILambdaFunctionHandler<TRequest, TResponse>, TFunctionHandler>());
            return hostBuilder;
        }

        public static void UseLambdaProxy<TRequest, TResponse>(this IApplicationBuilder app)
        {
            var handler = app.ApplicationServices.GetService<ILambdaFunctionHandler<TRequest, TResponse>>();
            app.Run(async context =>
            {
                string request;

                using (var sr = new StreamReader(context.Request.Body))
                {
                    request = await sr.ReadToEndAsync();
                }

                context.Response.ContentType = "application/json";
                if (string.IsNullOrWhiteSpace(request))
                {
                    await context.Response.WriteAsync(string.Empty);
                    return;
                }

                var response = await handler.HandleAsync(JsonConvert.DeserializeObject<TRequest>(request));
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
            });
        }
    }
}