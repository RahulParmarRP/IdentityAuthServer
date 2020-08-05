using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityAuthServer.Data;
using IdentityAuthServer.Models;
using IdentityAuthServer.Services;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Services;
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

            // auto added by ASP NET Web API
            services.AddControllers();

            services.AddScoped<IProfileService, CustomProfileService>();
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomUserClaimsPrincipalFactory>();

            // identity server service configs
            var builder = services
                .AddIdentityServer()
                // identity server service configs
                // Creates temporary key material at startup time.This is for dev scenarios. 
                // The generated key will be persisted in the local directory by default.
                .AddDeveloperSigningCredential()
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

            app.UseHttpsRedirection();

            // needed for MVC based routing in Web API
            app.UseRouting();

            // UseIdentityServer includes a call to UseAuthentication, 
            // so it’s not necessary to have both.
            // needed to add identity server and find its 
            // discovery document end point 
            app.UseIdentityServer();
            app.UseAuthorization();

            // needed for MVC based routing in Web API
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
