using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Models.CandidatePool;
using ProjectBuilder.Models.CandidatePool.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using IdentityUserManager = Volo.Abp.Identity.IdentityUserManager;

namespace ProjectBuilder.Web.Jobs
{
    public class CopyLibraryJob : ITransientDependency
    {
        private readonly ICandidatePoolAppService _candidatePoolAppService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IdentityUserManager _userManager;
        private readonly IUserClaimsPrincipalFactory<IdentityUser> _claimsPrincipalFactory;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;
        private readonly ILogger<CopyLibraryJob> _logger;

        public CopyLibraryJob(
            ICandidatePoolAppService candidatePoolAppService,
            IUnitOfWorkManager unitOfWorkManager,
            IdentityUserManager userManager,
            IUserClaimsPrincipalFactory<IdentityUser> claimsPrincipalFactory,
            ICurrentPrincipalAccessor currentPrincipalAccessor,
            ILogger<CopyLibraryJob> logger)
        {
            _candidatePoolAppService = candidatePoolAppService;
            _unitOfWorkManager = unitOfWorkManager;
            _userManager = userManager;
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _currentPrincipalAccessor = currentPrincipalAccessor;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunAsync(CreateCandidatePoolDto dto, Guid userId, Guid sourceLibraryId)
        {
            _logger.LogInformation("Library copy started: source={SourceId} name={Name}", sourceLibraryId, dto.Name);

            // Hangfire runs this on a background worker with no ambient ClaimsPrincipal, so the
            // [Authorize(...)] on CandidatePoolAppService's methods has no user/role claims to
            // check against and always throws AbpAuthorizationException — even though the acting
            // user's role was already confirmed to have the permission when the job was enqueued
            // (see OnPostCreateSharedLibraryAsync). Re-establish that user's principal for the
            // duration of the call so the authorization interceptor can see their role grants.
            var user = await _userManager.GetByIdAsync(userId);
            var principal = await _claimsPrincipalFactory.CreateAsync(user);

            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            using (_currentPrincipalAccessor.Change(principal))
            {
                await _candidatePoolAppService.CreateNewLibrary(dto, userId, sourceLibraryId.ToString());
            }
            await uow.CompleteAsync();
            _logger.LogInformation("Library copy completed: source={SourceId} name={Name}", sourceLibraryId, dto.Name);
        }
    }
}
