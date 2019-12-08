using System.IO;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.AspNetCoreServer.Internal;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http.Features;

namespace LambdaExtensions
{
    public abstract class GenericProxyFunction : AbstractAspNetCoreFunction<Stream, Stream>
    {
        protected override void MarshallRequest(InvokeFeatures features, Stream lambdaRequest,
            ILambdaContext lambdaContext)
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