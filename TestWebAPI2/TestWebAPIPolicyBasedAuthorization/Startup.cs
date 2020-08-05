using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace TestWebAPIPolicyBasedAuthorization
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            /*
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
            */
            /*
            services.AddAuthentication("Bearer")
               .AddJwtBearer("Bearer", config =>
               {
                   config.Authority = "https://localhost:5001/";

                   config.Audience = "api1";

                   config.RequireHttpsMetadata = false;
               });
            */

            // adds an authorization policy to make sure the token is for scope 'api1'
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AuthorizeByApiScope", policy =>
            //    {
            //        policy.RequireAuthenticatedUser();
            //        policy.RequireClaim("scope", "api1");
            //    });
            //});

            // we need authentication scheme for authorization
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Microsoft.AspNetCore.Authentication.JwtBearer
            //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            //Configuration.Bind("JwtSettings", options));
            .AddJwtBearer(options =>
             {
                 options.Authority = "https://localhost:5001";
                 //options.Audience = "api1";
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
             });


            // AddCookie
            //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => Configuration.Bind("CookieSettings", options))

            // policy based authorization using user's Claims
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PublicSecure",
                    policy => policy.RequireClaim("client_id", "clientId_for_claims_based_api"));
                options.AddPolicy("UserSecure",
                    policy => policy.RequireClaim("userRole", "endUser"));
                options.AddPolicy("AdminSecure",
                    policy => policy.RequireClaim("userRole", "clientAdmin"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});
        }
    }
}
