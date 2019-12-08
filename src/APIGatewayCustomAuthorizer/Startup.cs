using Amazon.Lambda.APIGatewayEvents;
using LambdaExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APIGatewayCustomAuthorizer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore()
                .AddApiExplorer()
                .AddJsonFormatters();

            services
                .AddScoped<IProxyHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>,
                    CustomAuthorizerHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IProxyHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse> handler)
        {
            app.UseLambdaProxy(handler);
        }
    }
}