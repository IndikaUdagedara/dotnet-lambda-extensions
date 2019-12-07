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
    public abstract class GenericProxyFunction<TRequest, TResponse> : AbstractAspNetCoreFunction<TRequest, TResponse>
    {
        protected override void MarshallRequest(InvokeFeatures features, TRequest lambdaRequest, ILambdaContext lambdaContext)
        {
            IHttpRequestFeature aspNetCoreRequestFeature = features;
            aspNetCoreRequestFeature.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lambdaRequest)));
        }

        protected override TResponse MarshallResponse(IHttpResponseFeature responseFeatures, ILambdaContext lambdaContext,
            int statusCodeIfNotSet = 200)
        {
            var s = Encoding.UTF8.GetString(((MemoryStream)responseFeatures.Body).ToArray());
            return JsonConvert.DeserializeObject<TResponse>(s);
        }
    }
}