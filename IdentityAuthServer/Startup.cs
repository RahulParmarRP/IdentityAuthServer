using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityAuthServer.Data;
using IdentityAuthServer.Models;
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
                .AddEntityFrameworkStores<AppDatabaseContext>();

            // auto added by ASP NET Web API
            services.AddControllers();

            // identity server service configs
            var builder = services
                .AddIdentityServer()
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
            // needed for asp net identity to run with
            // identity server integration
                .AddAspNetIdentity<AppUser>();

            // identity server service configs
            builder.AddDeveloperSigningCredential();
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
