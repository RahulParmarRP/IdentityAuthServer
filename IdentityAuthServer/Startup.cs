using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityAuthServer.Data;
using IdentityAuthServer.Interfaces;
using IdentityAuthServer.Models;
using IdentityAuthServer.Processors;
using IdentityAuthServer.Services;
using IdentityServer4;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAuthServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCors(options => options
                .AddPolicy("AllowAll", policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

            var connectionString = Configuration.GetConnectionString("MSSqlServerConnection");

            services.AddDbContext<AppDatabaseContext>(
                options => options.UseSqlServer(connectionString));

            /* Note that AddIdentity<ApplicationUser, IdentityRole> 
             * must be invoked before AddIdentityServer.
             */
            // this are dependency services for ASP Identity
            // related to create, login, sign out
            services
                .AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDatabaseContext>()
                /* they are exclusively used to generate opaque tokens for account operations (like password reset or email change) and two-factor authentication.
                 */
                .AddDefaultTokenProviders()
                // to add claims while returning user
                //.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>()
                ;

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            // services.AddAuthentication(options =>
            //{
            //    // This causes the default authentication scheme to be JWT.
            //    // Without this, the Authorization header is not checked and
            //    // you'll get no results. However, this also means that if
            //    // you're already using cookies in your app, they won't be 
            //    // checked by default.
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //});

            // auto added by ASP NET Web API
            services.AddControllers();

            services
                .AddAuthentication()
                //.AddJwtBearer()
                //.AddOpenIdConnect()
                .AddGoogle("Google", options =>
                {
                    //options.SignInScheme = IdentityServerConstants.JwtRequestClientKey;
                    //https://docs.identityserver.io/en/release/quickstarts/4_external_authentication.html
                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to https://localhost:5001/signin-google
                    //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    //options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = "563770001825-54sjoif5q2i2rqhnt69d7hhjc2khplec.apps.googleusercontent.com";
                    options.ClientSecret = "Hzim45MejNx8iOilM-RjZ2m_";
                    options.SaveTokens = true;
                })
            ;

            services.AddScoped<IProfileService, CustomProfileService>();
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomUserClaimsPrincipalFactory>();
            services.AddScoped<INonEmailUserProcessor, NonEmailUserProcessor>();
            services.AddScoped<IEmailUserProcessor, EmailUserProcessor>();

            // identity server service configs
            var builder = services
                .AddIdentityServer()
                // identity server service configs
                // Creates temporary key material at startup time.This is for dev scenarios. 
                // The generated key will be persisted in the local directory by default.
                .AddDeveloperSigningCredential()
                //.AddSigningCredential(cert)
                /*
                 * IdentityServer with an additional AddApiAuthorization helper method that sets up some default ASP.NET Core conventions on top of IdentityServer:
                 */
                // .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddTestUsers(Config.TestUsers)
                // needed for asp net identity to run with
                // identity server integration
                .AddAspNetIdentity<AppUser>()
                .AddExtensionGrantValidator<CustomExternalGrantValidator>()
                //.AddProfileService<IdentityWithAdditionalClaimsProfileService>()
                .AddProfileService<CustomProfileService>()
            ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /*
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        context.Response.AddApplicationError(error.Error.Message);
                        await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                    }
                });
            });
            */

            app.UseHttpsRedirection();

            // needed for MVC based routing in Web API
            app.UseRouting();

            app.UseCors("AllowAll");
            // UseIdentityServer includes a call to UseAuthentication, 
            // so it’s not necessary to have both.
            // needed to add identity server and find its 
            // discovery document end point 
            app.UseIdentityServer();
            //app.UseAuthentication();
            app.UseAuthorization();

            // needed for MVC based routing in Web API
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
