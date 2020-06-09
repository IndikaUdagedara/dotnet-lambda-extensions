using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.AspNetCoreServer.Internal;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#if NETCOREAPP_3_1
using Microsoft.Extensions.Hosting;
#endif


namespace LambdaExtensions
{
    /// <summary>
    ///     Base class for ASP.NET Core Lambda functions.
    /// </summary>
    public abstract class AbstractDotNetCoreFunction
    {
        /// <summary>
        ///     Key to access the ILambdaContext object from the HttpContext.Items collection.
        /// </summary>
        public const string LAMBDA_CONTEXT = "LambdaContext";

        /// <summary>
        ///     Key to access the Lambda request object from the HttpContext.Items collection. The object
        ///     can be either APIGatewayProxyRequest or ApplicationLoadBalancerRequest depending on the source of the event.
        /// </summary>
        public const string LAMBDA_REQUEST_OBJECT = "LambdaRequestObject";
    }

    /// <summary>
    ///     Base class for ASP.NET Core Lambda functions.
    /// </summary>
    /// <typeparam name="TREQUEST"></typeparam>
    /// <typeparam name="TRESPONSE"></typeparam>
    public abstract class AbstractDotNetCoreFunction<TREQUEST, TRESPONSE> : AbstractDotNetCoreFunction
    {
        private protected IServiceProvider _hostServices;
        private protected ILogger _logger;
        private protected StartupMode _startupMode;
        private ILambdaFunctionHandler<TREQUEST, TRESPONSE> _functionHandler;

        protected AbstractDotNetCoreFunction()
            : this(StartupMode.Constructor)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="startupMode">Configure when the ASP.NET Core framework will be initialized</param>
        protected AbstractDotNetCoreFunction(StartupMode startupMode)
        {
            _startupMode = startupMode;

            if (_startupMode == StartupMode.Constructor)
            {
                Start();
            }
        }


        private protected bool IsStarted => _hostServices != null;


        /// <summary>
        ///     Method to initialize the host builder before starting the host. In a typical Web API this is similar to the main
        ///     function.
        ///     Setting the Startup class is required in this method.
        ///     <para>
        ///         It is recommended to not configure the IWebHostBuilder from this method. Instead configure the IWebHostBuilder
        ///         in the Init(IWebHostBuilder builder) method. If you configure the IWebHostBuilder in this method the
        ///         IWebHostBuilder will be
        ///         configured twice, here and and as part of CreateHostBuilder.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        /// protected override void Init(IHostBuilder builder)
        /// {
        ///     builder
        ///         .UseStartup&lt;Startup&gt;();
        /// }
        /// </code>
        /// </example>
        /// <param name="builder"></param>
        protected virtual void Init(IHostBuilder builder)
        {
        }

        /// <summary>
        ///     Creates the IHostBuilder similar to Host.CreateDefaultBuilder but replacing the registration of the Kestrel web
        ///     server with a
        ///     registration for Lambda.
        ///     <para>
        ///         When overriding this method it is recommended that ConfigureWebHostLambdaDefaults should be called instead of
        ///         ConfigureWebHostDefaults to ensure the IWebHostBuilder
        ///         has the proper services configured for running in Lambda. That includes registering Lambda instead of Kestrel
        ///         as the IServer implementation
        ///         for processing requests.
        ///     </para>
        /// </summary>
        /// <returns></returns>
        protected virtual IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder();
            Init(builder);
            return builder;
        }

        /// <summary>
        ///     Should be called in the derived constructor
        /// </summary>
        protected void Start()
        {
            var builder = CreateHostBuilder();
            var host = builder.Build();
            PostCreateHost(host);

            host.Start();
            _hostServices = host.Services;
            _logger =
                ActivatorUtilities.CreateInstance<Logger<AbstractAspNetCoreFunction<TREQUEST, TRESPONSE>>>(
                    _hostServices);
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

        /// <summary>
        ///     This method is what the Lambda function handler points to.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lambdaContext"></param>
        /// <returns></returns>
        public virtual async Task<TRESPONSE> FunctionHandlerAsync(TREQUEST request, ILambdaContext lambdaContext)
        {
            if (!IsStarted)
            {
                Start();
            }


            var scope = _hostServices.CreateScope();
            try
            {

                return await ProcessRequest(lambdaContext, request);
            }
            finally
            {
                scope.Dispose();
            }
        }

        /// <summary>
        ///     Processes the current request.
        /// </summary>
        /// <param name="lambdaContext"><see cref="ILambdaContext" /> implementation.</param>
        /// <param name="context">The hosting application request context object.</param>
        /// <param name="features">An <see cref="InvokeFeatures" /> instance.</param>
        /// <param name="rethrowUnhandledError">
        ///     If specified, an unhandled exception will be rethrown for custom error handling.
        ///     Ensure that the error handling code calls 'this.MarshallResponse(features, 500);' after handling the error to
        ///     return a the typed Lambda object to the user.
        /// </param>
        protected async Task<TRESPONSE> ProcessRequest(ILambdaContext lambdaContext, TREQUEST request, bool rethrowUnhandledError = false)
        {
            Exception ex = null;
            try
            {
                var handler = _hostServices.GetService<ILambdaFunctionHandler<TREQUEST, TRESPONSE>>();
                return await handler.HandleAsync(request);
            }
            catch (AggregateException agex)
            {
                ex = agex;
                _logger.LogError($"Caught AggregateException: '{agex}'");
                var sb = new StringBuilder();
                foreach (var newEx in agex.InnerExceptions)
                {
                    sb.AppendLine(ErrorReport(newEx));
                }

                _logger.LogError(sb.ToString());
            }
            catch (ReflectionTypeLoadException rex)
            {
                ex = rex;
                _logger.LogError($"Caught ReflectionTypeLoadException: '{rex}'");
                var sb = new StringBuilder();
                foreach (var loaderException in rex.LoaderExceptions)
                {
                    var fileNotFoundException = loaderException as FileNotFoundException;
                    if (fileNotFoundException != null && !string.IsNullOrEmpty(fileNotFoundException.FileName))
                    {
                        sb.AppendLine($"Missing file: {fileNotFoundException.FileName}");
                    }
                    else
                    {
                        sb.AppendLine(ErrorReport(loaderException));
                    }
                }

                _logger.LogError(sb.ToString());
            }
            catch (Exception e)
            {
                ex = e;
                if (rethrowUnhandledError) throw;
                _logger.LogError($"Unknown error responding to request: {ErrorReport(e)}");
            }

            InternalCustomResponseExceptionHandling(lambdaContext, ex);

            return default;
        }


        private protected virtual void InternalCustomResponseExceptionHandling(
            ILambdaContext lambdaContext, Exception ex)
        {
        }

        protected virtual void PostCreateHost(IHost webHost)
        {
        }

    }
}