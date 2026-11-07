using Coravel.Pro.Features.Auth.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WingsEmu.ClusterScheduler.Utility
{
    public class AdminHasPermission : IHasPermission
    {
        public bool HasPermission(HttpRequest request, ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.IsInRole("Admin");
        }
    }
}
