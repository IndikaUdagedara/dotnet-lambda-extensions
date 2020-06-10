using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace AWS.Lambda.Extensions
{
    public static class Extensions
    {
        /// <summary>
        ///     Call this after creating the host builder and to expose a local HTTP endpoint to invoke lambda
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TFunctionHandler"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureWebHost"></param>
        /// <returns></returns>
        public static IHostBuilder UseLambdaLocal<TRequest, TResponse, TFunctionHandler>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configureWebHost = null)
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

        /// <summary>
        ///     Register lambda function handler
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TFunctionHandler"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseLambda<TRequest, TResponse, TFunctionHandler>(this IHostBuilder hostBuilder)
            where TFunctionHandler : class, ILambdaFunctionHandler<TRequest, TResponse>
        {
            hostBuilder.ConfigureServices(c =>
                c.AddScoped<ILambdaFunctionHandler<TRequest, TResponse>, TFunctionHandler>());
            return hostBuilder;
        }


        internal static void UseLambdaProxy<TRequest, TResponse>(this IApplicationBuilder app)
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
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
            });
        }

        /// <summary>
        ///     Call this after creating the host builder and to expose a local HTTP endpoint to invoke lambda
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TFunctionHandler"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureWebHost"></param>
        /// <returns></returns>
        public static IHostBuilder UseLambdaLocal<TRequest, TFunctionHandler>(this IHostBuilder hostBuilder,
            Action<IWebHostBuilder> configureWebHost = null)
            where TFunctionHandler : class, ILambdaFunctionHandler<TRequest>
        {
            hostBuilder
                .UseLambda<TRequest, TFunctionHandler>()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    configureWebHost?.Invoke(webHostBuilder);
                    webHostBuilder
                        .Configure(a => a.UseLambdaProxy<TRequest>())
                        .UseKestrel();
                });
            return hostBuilder;
        }

        /// <summary>
        ///     Register lambda function handler
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TFunctionHandler"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseLambda<TRequest, TFunctionHandler>(this IHostBuilder hostBuilder)
            where TFunctionHandler : class, ILambdaFunctionHandler<TRequest>
        {
            hostBuilder.ConfigureServices(c =>
                c.AddScoped<ILambdaFunctionHandler<TRequest>, TFunctionHandler>());
            return hostBuilder;
        }

        internal static void UseLambdaProxy<TRequest>(this IApplicationBuilder app)
        {
            var handler = app.ApplicationServices.GetService<ILambdaFunctionHandler<TRequest>>();
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

                await handler.HandleAsync(JsonConvert.DeserializeObject<TRequest>(request));
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.CompleteAsync();
            });
        }
    }
}