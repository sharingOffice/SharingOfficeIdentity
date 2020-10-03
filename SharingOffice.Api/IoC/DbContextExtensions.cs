using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharingOffice.Infra.DbContexts;

namespace SharingOffice.Api.IoC
{
    public static  class DbContextExtensions
    {
        public static void PerformDbContextRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services
                .AddDbContext<SharringOfficeDbContext>(options =>
                    {
                        options.UseSqlServer(configuration["ConnectionStrings:DatabaseConnectionString"],
                            sqlServerOptionsAction: sqlOptions =>
                            {
                                sqlOptions.MigrationsAssembly(typeof(SharringOfficeDbContext).GetTypeInfo().Assembly.GetName()
                                    .Name);
                                sqlOptions.EnableRetryOnFailure(maxRetryCount: 10,
                                    maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                            });
                    },
                    ServiceLifetime.Scoped
                    //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
                );
        }
    }
}