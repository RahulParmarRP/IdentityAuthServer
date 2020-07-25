using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace TestWebAPI
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
            services.AddControllers();

            /*
             * Authentication with an additional AddIdentityServerJwt helper method that configures the app to validate JWT tokens produced by IdentityServer:
                services.AddAuthentication()
                    .AddIdentityServerJwt();
             */

            // accepts any access token issued by identity server
            services.AddAuthentication("JWTBearerToken") // custom scheme name
                 .AddJwtBearer("JWTBearerToken", options =>
                 {
                     options.Authority = "https://localhost:5001";
                     options.TokenValidationParameters =
                     new TokenValidationParameters
                     {
                         ValidateAudience = false
                     };
                 });

            // adds an authorization policy to make sure the token is for scope 'api1'
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AuthorizeByApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "api1");
                });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            /*
                In some cases, the call to AddAuthentication is automatically made by other extension methods. For example, when using ASP.NET Core Identity, AddAuthentication is called internally.

                Identity is enabled by calling UseAuthentication. UseAuthentication adds authentication middleware to the request pipeline.
            */
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                .MapControllers()
                // auth policy name
                .RequireAuthorization("AuthorizeByApiScope");
            });
        }
    }
}
