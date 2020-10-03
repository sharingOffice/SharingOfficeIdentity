using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharingOffice.Api.IoC;
using SharingOffice.Api.Middlewares;
using SharingOffice.Common.models;
using SharingOffice.Domain.Contracts.Repositories;
using SharingOffice.Infra.DbContexts;
using SharingOffice.Infra.Repositories;
using SharingOffice.Service.Contracts.Services;
using SharingOffice.Service.Services;

namespace SharingOffice.Api
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
            //services.AddControllers();
            
            services.AddDbContext<SharringOfficeDbContext>();
            
            services.AddAuthentication()
                .AddGoogle(opts =>
                {
                    opts.ClientId = "983471041526-0orv8d7imhopfncss6du98u5vr1be79q.apps.googleusercontent.com";
                    opts.ClientSecret = "OMTNetH_FLAhNAUSoNIvzpK4";
                    opts.SignInScheme = IdentityConstants.ExternalScheme;
                });
            
            
            services.AddCors();
            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddSwaggerGen();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            
            services.PerformDbContextRegistration(Configuration);
            
            // configure DI for application services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserRepository, UserRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "ASP.NET Core Sign-up and Verification API"));

            
            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            
            // global error handler
            app.UseMiddleware<ErrorHandlerMiddleware>();

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();
            
            //
            // app.UseHttpsRedirection();
            //
             app.UseRouting();
            //
            // app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}