using System.Threading.Tasks;
using SharingOffice.Common.models;
using SharingOffice.Domain.models;

namespace SharingOffice.Service.Contracts.Services
{
    public interface IAccountService
    {
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        void RevokeToken(string token, string ipAddress);
        Task<AuthenticateResponse> SignIn(AuthenticateRequest model, string ipAddress);
        Task<AuthenticateResponse> SignInOAuthUser(string name, string email, string subject, string issuer, string ipAddress);
    }
}