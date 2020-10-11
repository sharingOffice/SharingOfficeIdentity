using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharingOffice.Domain.Contracts.Repositories;
using SharingOffice.Service.Contracts.Services;

namespace SharingOffice.Api.Infra.Authorizations
{
    public class TokenValidatorService : ITokenValidatorService
    {
        private IConfiguration _configuration;

        private IAccountService _accountService;
        private IUserRepository _userRepository;
        //private readonly IUsersService _usersService;
        //private readonly ITokenStoreService _tokenStoreService;

        public TokenValidatorService(
            IConfiguration configuration,
            IAccountService accountService,
            IUserRepository userRepository

            // IUsersService usersService, ITokenStoreService tokenStoreService
        )
        {
            _configuration = configuration;
            _accountService = accountService;
            _userRepository = userRepository;
            //  _usersService = usersService;
            //  _usersService.CheckArgumentIsNull(nameof(usersService));

            //_tokenStoreService = tokenStoreService;
            //_tokenStoreService.CheckArgumentIsNull(nameof(_tokenStoreService));
        }

        public async Task ValidateAsync(TokenValidatedContext context)
        {
            var userPrincipal = context.Principal;

            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            
            if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
            {
                context.Fail("This is not our issued token. It has no claims.");
                return;
            }

            // var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);
            // if (serialNumberClaim == null)
            // {
            //     context.Fail("This is not our issued token. It has no serial.");
            //     return;
            // }

            var userId = claimsIdentity.FindFirst("id").Value;
            if (!Guid.TryParse(userId, out Guid accountId))
            {
                context.Fail("This is not our issued token. It has no user-id.");
                return;
            }
            
            // var user = await _userRepository.GetById(accountId);
            // if (user == null || user.SerialNumber != serialNumberClaim.Value || !user.IsActive)
            // {
            //     // user has changed his/her password/roles/stat/IsActive
            //     context.Fail("This token is expired. Please login again.");
            // }

            var accessToken = context.SecurityToken as JwtSecurityToken;
            if (accessToken == null || string.IsNullOrWhiteSpace(accessToken.RawData)
                                     //|| !await _tokenStoreService.IsValidTokenAsync(accessToken.RawData, userId)
            )
            {
                context.Fail("This token is not in our database.");
                return;
            }

            await Task.Delay(0);
            await _accountService.UpdateUserLastActivityDateAsync(accountId);

        }

        public Guid ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["BearerTokens:Key"]);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            return accountId;
        }
    }
}