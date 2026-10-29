using ProjectBuilder.Entities;
using ProjectBuilder.Models.ProjectTreatment.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Services
{
    internal static class UserTreatmentValidationHelper
    {
        public static async Task<int> ResolveUserTreatmentTypeNoByCategoryAsync(IRepository<UserTreatmentType, int> userTreatmentTypeRepository, string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return 0;
            }

            var types = await userTreatmentTypeRepository.GetListAsync();
            var match = types.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.UserTreatmentName)
                && t.UserTreatmentName.Trim().Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));
            return match?.Id ?? 0;
        }

        public static void ValidateRequiredForCreate(UserTreatment t)
        {
            ApplyCommittedDefaults(t);

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(t.AssetType)) missing.Add("Asset Type");
            if (string.IsNullOrWhiteSpace(t.Treatment)) missing.Add("Treatment");
            if (!t.UserTreatmentTypeNo.HasValue) missing.Add("Treatment Type");
            if (!t.District.HasValue) missing.Add("District");
            if (!t.CountyId.HasValue) missing.Add("County");
            if (!t.Route.HasValue) missing.Add("Route");
            if (!t.FromSection.HasValue) missing.Add("Section");
            if (!t.Interstate.HasValue) missing.Add("Interstate");
            if (!t.PreferredYear.HasValue) missing.Add("Preferred Year");
            if (!t.PriorityOrder.HasValue) missing.Add("Priority Order");
            if (!t.Cost.HasValue) missing.Add("Construction Cost");

            if (missing.Count > 0)
            {
                throw new UserFriendlyException("The following fields are required: " + string.Join(", ", missing));
            }
            if (t.MinYear.HasValue && t.MaxYear.HasValue && t.MinYear > t.MaxYear)
            {
                throw new UserFriendlyException("Maximum year must be greater than or equal to minimum year.");
            }
        }

        // Committed treatments pin priority to 0 and min/max to preferred year; UIs disable
        // those fields so they may arrive null from FormData even though the user set them.
        public static void ApplyCommittedDefaults(UserTreatment t)
        {
            if (t.IsCommitted != true) return;

            t.PriorityOrder ??= 0;
            if (t.PreferredYear.HasValue)
            {
                t.MinYear ??= t.PreferredYear;
                t.MaxYear ??= t.PreferredYear;
            }
        }

        public static void ApplyCommittedDefaults(CreateProjectTreatmentDto input)
        {
            if (input.IsCommitted != true) return;

            input.PriorityOrder ??= 0;
            if (input.PreferredYear.HasValue)
            {
                input.MinYear ??= input.PreferredYear;
                input.MaxYear ??= input.PreferredYear;
            }
        }
    }
}
