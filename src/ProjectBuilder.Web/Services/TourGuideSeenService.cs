using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using IdentityUserManager = Volo.Abp.Identity.IdentityUserManager;

namespace ProjectBuilder.Web.Services
{
    public interface ITourGuideSeenService
    {
        Task<bool> IsSeenAsync(Guid userId, string tourKey);

        Task MarkSeenAsync(Guid userId, string tourKey);
    }
    
    public class TourGuideSeenService : ITourGuideSeenService, ITransientDependency
    {
        private const string ClaimTypePrefix = "pb:tour-seen:";

        private readonly IdentityUserManager _userManager;

        public TourGuideSeenService(IdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> IsSeenAsync(Guid userId, string tourKey)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.Any(c => c.Type == ClaimTypePrefix + tourKey);
        }

        public async Task MarkSeenAsync(Guid userId, string tourKey)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            if (!claims.Any(c => c.Type == ClaimTypePrefix + tourKey))
            {
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypePrefix + tourKey, "1"));
            }
        }
    }
}
