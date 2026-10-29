using ProjectBuilder.Models.UserTreatmentType.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.UserTreatmentType
{
    public interface IUserTreatmentTypeAppService : IApplicationService
    {
        Task<List<UserTreatmentTypeDto>> GetAllAsync();
        Task<UserTreatmentTypeDto> EditUserTreatments(UpdateUserTreatmentTypeDto model);
        Task<bool> DeleteUserTreatments(UserTreatmentTypeDto model);
    }
}
