using System.IO;
using System.Text;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.AspNetCoreServer.Internal;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace LambdaExtensions
{
    public abstract class GenericProxyFunction : AbstractAspNetCoreFunction<Stream, Stream>
    {
        protected override void MarshallRequest(InvokeFeatures features, Stream lambdaRequest, ILambdaContext lambdaContext)
        {
            IHttpRequestFeature aspNetCoreRequestFeature = features;
            aspNetCoreRequestFeature.Body = lambdaRequest;
        }

        protected override Stream MarshallResponse(IHttpResponseFeature responseFeatures, ILambdaContext lambdaContext,
            int statusCodeIfNotSet = 200)
        {
            return new MemoryStream(((MemoryStream) responseFeatures.Body).ToArray());
        }
    }
}