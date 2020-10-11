using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharingOffice.Common.models;
using SharingOffice.Domain.Contracts.Repositories;
using SharingOffice.Domain.models;
using SharingOffice.Service.Contracts.Services;
using BC = BCrypt.Net.BCrypt;

namespace SharingOffice.Service.Services
{
    public class AccountService: IAccountService
    {
        private IUserRepository _userRepository;
        private IConfiguration _configuration;

        public AccountService(
            IUserRepository userRepository, 
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }
        
        public async Task<AuthenticateResponse> SignIn(AuthenticateRequest model, string ipAddress)
        {
            var account = await _userRepository.GetByEmail(model.Email);
            
            if (account == null || !account.EmailConfirmed || !BC.Verify(model.Password, account.PasswordHash))
                throw new Exception("Email or password is incorrect");
            
            // authentication successful so generate jwt and refresh tokens
            var jwtToken = GenerateJwtToken(account);
            var refreshToken = GenerateRefreshToken(ipAddress);

            // save refresh token
            account.RefreshTokens.Add(refreshToken);
            _userRepository.Update(account);

            var response = new AuthenticateResponse()
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                IsVerified = account.EmailConfirmed,
                Created = refreshToken.Created,
                Id = refreshToken.Id
            };
            
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
            return response;
        }
        
        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var (refreshToken, account) = GetRefreshToken(token);

            // replace old refresh token with a new one and save
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            account.RefreshTokens.Add(newRefreshToken);
            _userRepository.Update(account);

            // generate new jwt
            var jwtToken = GenerateJwtToken(account);

            var response = new AuthenticateResponse()
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,

            };
            
            response.JwtToken = jwtToken;
            response.RefreshToken = newRefreshToken.Token;
            return response;
        }

        public void RevokeToken(string token, string ipAddress)
        {
            var (refreshToken, account) = GetRefreshToken(token);

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _userRepository.Update(account);
        }

        public async Task<AuthenticateResponse> SignInOAuthUser(string name, string email, string subject, string issuer, string ipAddress)
        {
            var user = await _userRepository.GetByEmail(email);

            if (user == null)
            {
                user = new User()
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    FirstName = name,
                    LastName = name,
                    EmailConfirmed = true,
                    NormalizedEmail = email,
                    PhoneNumber = "123456789",
                    PhoneNumberConfirmed = true,
                    NormalizedUserName = email,
                    TwoFactorEnabled = false,
                    OAuthSubject = subject,
                    OAuthIssuer = issuer,
                    IsActive = true,
                };
                _userRepository.Update(user);
            }
            
            // authentication successful so generate jwt and refresh tokens
            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            // save refresh token
            user.RefreshTokens.Add(refreshToken);
            _userRepository.Update(user);
            
            var response = new AuthenticateResponse()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsVerified = user.EmailConfirmed,
                Created = refreshToken.Created,
                Id = refreshToken.Id
            };
            
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
            return response;
        }

        public async Task UpdateUserLastActivityDateAsync(Guid userId)
        {
            var user = await _userRepository.GetById(userId);
            user.LastActivityAt = DateTime.UtcNow;
            _userRepository.Update(user);
        }

        private string GenerateJwtToken(User account)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["BearerTokens:Key"]));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new HashSet<Claim>
            {
                new Claim("id", account.Id.ToString())
            };
            
            var jwt = new JwtSecurityToken(
                _configuration["BearerTokens:Issuer"], // site that makes the token
                _configuration["BearerTokens:Audience"], // site that consumes the token
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: DateTime.Now.AddMinutes(-120),
                expires: DateTime.Now.AddMinutes(2));

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }
        
        private (RefreshToken, User) GetRefreshToken(string token)
        {
            var account = _userRepository.Get(token).GetAwaiter().GetResult();
            if (account == null) throw new Exception("Invalid token");
            var refreshToken = account.RefreshTokens.Single(x => x.Token == token);
            if (!refreshToken.IsActive) throw new Exception("Invalid token");
            return (refreshToken, account);
        }
        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }
        private string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
    }
}