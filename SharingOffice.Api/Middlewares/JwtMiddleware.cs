using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SharingOffice.Infra.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using SharingOffice.Api.Infra.Authorizations;

namespace SharingOffice.Api.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, SharingOfficeDbContext dataContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachAccountToContext(context, dataContext, token);
            
            await _next(context);
        }
        
        private async Task AttachAccountToContext(HttpContext context, SharingOfficeDbContext dataContext, string token)
        {
            try
            {
                var tokenValidator = context.RequestServices.GetService<ITokenValidatorService>();

                var accountId = tokenValidator.ValidateToken(token);

                // attach account to context on successful jwt validation
                var user = await dataContext.Users.FindAsync(accountId);
                context.Items["Account"] = user;
            }
            catch (Exception exception)
            {
                // do nothing if jwt validation fails
                // account is not attached to context so request won't have access to secure routes
            }
        }
    }
}