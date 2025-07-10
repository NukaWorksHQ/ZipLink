using Microsoft.AspNetCore.Mvc;
using Server.Common;
using System.Security.Claims;

namespace Server.Services
{
    public class UserAccessValidator
    {

        public UserClaimResponse GetUserClaimStatus(ClaimsPrincipal user)
        {
            var authenticatedUserId = user.FindFirst("UserId")?.Value;
            if (authenticatedUserId is null)
                throw new ArgumentException("User is invalid");

            var roleValue = user.FindFirst(ClaimTypes.Role)?.Value;
            if (roleValue is null)
                throw new ArgumentException("User claim Role is missing");

            if (!Enum.TryParse<UserRole>(roleValue, out var userRole))
                throw new ArgumentException("Invalid role");

            return new UserClaimResponse
            {
                Role = userRole,
                UserId = authenticatedUserId
            };
        }

        public IActionResult ValidateUser(ClaimsPrincipal user, string id, bool needsAdminPrivileges = false)
        {
            try
            {
                var userStatus = GetUserClaimStatus(user);

                if (needsAdminPrivileges && userStatus.UserId != id && userStatus.Role is not UserRole.Admin)
                    return StatusError(403, "Access denied");
            }
            catch (ArgumentException ex)
            {
                return StatusError(400, ex.Message);
            }

            return new OkObjectResult("User Validation passed");
        }

        private IActionResult StatusError(int statusCode, string message)
        {
            return new ObjectResult(new { error = message }) { StatusCode = statusCode };
        }
    }
}
