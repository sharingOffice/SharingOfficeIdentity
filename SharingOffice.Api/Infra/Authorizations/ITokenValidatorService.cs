using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SharingOffice.Api.Infra.Authorizations
{
    public interface ITokenValidatorService
    {
        Task ValidateAsync(TokenValidatedContext context);

        Guid ValidateToken(string token);
    }
}