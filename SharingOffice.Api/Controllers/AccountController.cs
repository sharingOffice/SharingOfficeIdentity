using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharingOffice.Common.models;
using SharingOffice.Domain;
using SharingOffice.Domain.models;
using SharingOffice.Service.Contracts.Services;
using Google.Apis.Auth;

namespace SharingOffice.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        private IAccountService _accountService; 
        
        public User CurrentUser => (User)HttpContext.Items["Account"];

        public AccountController(ILogger<ValuesController> logger, IAccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("true");
        }
        
        [HttpPost("SignIn")]
        public ActionResult<AuthenticateResponse> SignIn([FromBody]AuthenticateRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest("");
            
            var response =  _accountService.SignIn(model, IpAddress()).GetAwaiter().GetResult();
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }
        
        [HttpPost("RefreshToken")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken, IpAddress());
            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("RevokeToken")]
        public IActionResult RevokeToken(RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            // users can revoke their own tokens and admins can revoke any tokens
            if (!CurrentUser.OwnsToken(token) && CurrentUser.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            _accountService.RevokeToken(token, IpAddress());
            return Ok(new { message = "Token revoked" });
        }
        
        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(3)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
        
        [AllowAnonymous]
        [HttpPost("Google")]
        public async Task<IActionResult> Google([FromBody]string tokenId)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(tokenId, new GoogleJsonWebSignature.ValidationSettings()).Result;
                
                // add user if not exists, then signin 
                var response = await _accountService.SignInOAuthUser(payload.Name, payload.Email, payload.Subject, payload.Issuer, IpAddress());
                SetTokenCookie(response.RefreshToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                //Helpers.SimpleLogger.Log(ex);
                BadRequest(ex.Message);
            }
            return BadRequest();
        }
        
        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
    
    public class UserView
    {
        public string tokenId {get; set;}
    }
    
}