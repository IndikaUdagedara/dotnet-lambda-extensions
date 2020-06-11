using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AWS.Lambda.Extensions
{
    /// <summary>
    ///     Base class for .NET Core Lambda functions.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class AbstractDotNetCoreFunction
    {
        protected IServiceProvider HostServices;
        protected ILogger Logger;
        protected StartupMode StartupMode;


        protected AbstractDotNetCoreFunction()
            : this(StartupMode.Constructor)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="startupMode">Configure when the host is initialized</param>
        protected AbstractDotNetCoreFunction(StartupMode startupMode)
        {
            StartupMode = startupMode;

            if (StartupMode == StartupMode.Constructor)
            {
                Start();
            }
        }


        protected bool IsStarted => HostServices != null;


        /// <summary>
        ///     Method to configure the Host e.g. register services
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Init(IHostBuilder builder)
        {
        }

        /// <summary>
        ///     Creates a default IHostBuilder (Host.CreateDefaultBuilder) and calls implementations configuration method (Init())
        /// </summary>
        /// <returns></returns>
        protected virtual IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder();
            Init(builder);
            return builder;
        }

        /// <summary>
        ///     Start the host
        /// </summary>
        protected void Start()
        {
            var builder = CreateHostBuilder();
            var host = builder.Build();
            PostCreateHost(host);

            host.Start();
            HostServices = host.Services;
            Logger = ActivatorUtilities.CreateInstance<Logger<AbstractDotNetCoreFunction>>(HostServices);
        }


        /// <summary>
        ///     Formats an Exception into a string, including all inner exceptions.
        /// </summary>
        /// <param name="e"><see cref="Exception" /> instance.</param>
        protected string ErrorReport(Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{e.GetType().Name}:\n{e}");

            var inner = e;
            while (inner != null)
            {
                // Append the messages to the StringBuilder.
                sb.AppendLine($"{inner.GetType().Name}:\n{inner}");
                inner = inner.InnerException;
            }

            return sb.ToString();
        }


        private protected virtual void InternalCustomResponseExceptionHandling(
            ILambdaContext lambdaContext, Exception ex)
        {
        }

        /// <summary>
        ///     Called after host is created
        /// </summary>
        /// <param name="webHost"></param>
        protected virtual void PostCreateHost(IHost webHost)
        {
        }

        /// <summary>
        ///     Handle known exceptions
        /// </summary>
        /// <param name="lambdaContext"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected virtual bool HandleException(ILambdaContext lambdaContext, Exception ex)
        {
            InternalCustomResponseExceptionHandling(lambdaContext, ex);

            if (ex is AggregateException agex)
            {
                Logger.LogError($"Caught AggregateException: '{agex}'");
                var sb = new StringBuilder();
                foreach (var newEx in agex.InnerExceptions)
                {
                    sb.AppendLine(ErrorReport(newEx));
                }

                Logger.LogError(sb.ToString());

                return true;
            }

            if (ex is ReflectionTypeLoadException rex)
            {
                Logger.LogError($"Caught ReflectionTypeLoadException: '{rex}'");
                var sb = new StringBuilder();

                foreach (var loaderException in rex.LoaderExceptions)
                {
                    if (loaderException is FileNotFoundException fileNotFoundException &&
                        !string.IsNullOrEmpty(fileNotFoundException.FileName))
                    {
                        sb.AppendLine($"Missing file: {fileNotFoundException.FileName}");
                    }
                    else
                    {
                        sb.AppendLine(ErrorReport(loaderException));
                    }
                }


                Logger.LogError(sb.ToString());

                return true;
            }

            return false;
        }
    }


    public abstract class AbstractDotNetCoreFunction<TRequest, TResponse> : AbstractDotNetCoreFunction
    {
        /// <summary>
        ///     This method is what the Lambda function handler points to for synchronous (request/response) invocations e.g. API GW
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lambdaContext"></param>
        /// <returns></returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
        public virtual async Task<TResponse> FunctionHandlerAsync(TRequest request, ILambdaContext lambdaContext)
        {
            if (!IsStarted)
            {
                Start();
            }

            try
            {
                using var scope = HostServices.CreateScope();
                var handler = scope.ServiceProvider.GetService<ILambdaFunctionHandler<TRequest, TResponse>>();
                return await handler.HandleAsync(request);
            }
            catch (Exception ex)
            {
                if (!HandleException(lambdaContext, ex))
                {
                    Logger.LogError($"Unknown error responding to request: {ErrorReport(ex)}");
                    throw;
                }
            }

            return default;
        }
    }

    public abstract class AbstractDotNetCoreFunction<TRequest> : AbstractDotNetCoreFunction
    {
        /// <summary>
        ///     This method is what the Lambda function handler points to for asynchronous (event triggered) invocations e.g. S3 event
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lambdaContext"></param>
        /// <returns></returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
        public virtual async Task FunctionHandlerAsync(TRequest request, ILambdaContext lambdaContext)
        {
            if (!IsStarted)
            {
                Start();
            }

            try
            {
                using var scope = HostServices.CreateScope();
                var handler = scope.ServiceProvider.GetService<ILambdaFunctionHandler<TRequest>>();
                await handler.HandleAsync(request);
            }
            catch (Exception ex)
            {
                if (!HandleException(lambdaContext, ex))
                {
                    Logger.LogError($"Unknown error responding to request: {ErrorReport(ex)}");
                    throw;
                }
            }
        }
    }
}