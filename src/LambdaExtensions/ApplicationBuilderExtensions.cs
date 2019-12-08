using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LambdaExtensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseLambdaProxy<TRequest, TRresponse>(this IApplicationBuilder app,
            IProxyHandler<TRequest, TRresponse> handler)
        {
            app.Run(async context =>
            {
                string request;

                if (context.Request.Body is MemoryStream stream)
                    request = Encoding.UTF8.GetString(stream.ToArray());
                else
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