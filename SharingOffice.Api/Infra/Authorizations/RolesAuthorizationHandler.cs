using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SharingOffice.Domain.models;

namespace SharingOffice.Api.Infra.Authorizations
{
    public class RolesAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private IHttpContextAccessor _httpContextAccessor;
        private UserManager<User> _userManager;

        public RolesAuthorizationHandler(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            RolesAuthorizationRequirement requirement)
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext.User;
            if (!claimsPrincipal.Identity.IsAuthenticated)
            {
                context.Fail();
            }
            
            var user = (User)_httpContextAccessor.HttpContext.Items["Account"];
            if (await _userManager.IsInRoleAsync(user, user.Role.ToString()))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            
            // var routeData = _httpContextAccessor.HttpContext.GetRouteData();
            //
            // var areaName = routeData?.Values["area"]?.ToString();
            // var area = string.IsNullOrWhiteSpace(areaName) ? string.Empty : areaName;
            //
            // var controllerName = routeData?.Values["controller"]?.ToString();
            // var controller = string.IsNullOrWhiteSpace(controllerName) ? string.Empty : controllerName;
            //
            // var actionName = routeData?.Values["action"]?.ToString();
            // var action = string.IsNullOrWhiteSpace(actionName) ? string.Empty : actionName;
            //
            // var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
            // var userId = claimsIdentity?.FindFirst("id").Value;
            //
            //
            // var user = await _userManager.FindByIdAsync(userId);
            // if (context.Resource is Endpoint endpoint)
            // {
            //     var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            //     if (controllerActionDescriptor != null)
            //     {
            //         var authorzieAttributes =
            //             controllerActionDescriptor.EndpointMetadata.FirstOrDefault(q => q is AuthorizeAttribute);
            //
            //         var authorzieAttribute = (AuthorizeAttribute) authorzieAttributes;
            //         var roles = authorzieAttribute.Roles.Split(",");
            //
            //         foreach (var role in roles)
            //         {
            //             if (await _userManager.IsInRoleAsync(user, role))
            //             {
            //                 context.Succeed(requirement);
            //                 return;
            //             }
            //         }
            //     }
            // }

           
        }
    }
}