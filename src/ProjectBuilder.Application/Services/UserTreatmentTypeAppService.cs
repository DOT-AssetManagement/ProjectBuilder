using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.ScenarioBudgetFlat.Dtos;
using ProjectBuilder.Models.UserTreatment.Dtos;
using ProjectBuilder.Models.UserTreatmentType;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    public class UserTreatmentTypeAppService : ApplicationService, IUserTreatmentTypeAppService
    {
        private readonly IUserTreatmentTypeRepository _userTreatmentTypeRepository;
        private readonly IObjectMapper _objectMapper;

        public UserTreatmentTypeAppService(IUserTreatmentTypeRepository userTreatmentTypeRepository, IObjectMapper objectMapper)
        {
            _userTreatmentTypeRepository = userTreatmentTypeRepository;
            _objectMapper = objectMapper;
        }
        public async Task<PaginatedDto<UserTreatmentTypeDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var userTreatments = await _userTreatmentTypeRepository.GetQueryableAsync();

            var displayResult = userTreatments
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();

            var totalRecords = userTreatments.Count();
            var mappedResults = _objectMapper.Map<List<UserTreatmentType>, List<UserTreatmentTypeDto>>(displayResult);

            return new PaginatedDto<UserTreatmentTypeDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults 
            };
        }
        public async Task<UserTreatmentTypeDto> GetUserTreatment(int id)
        {
            var userTreatment = await _userTreatmentTypeRepository.FindAsync(id);
            if (userTreatment == null)
            {
                return null;
            }

            var userTreatmentViewModel = new UserTreatmentTypeDto
            {
                Id = userTreatment.Id,
                UserTreatmentName = userTreatment.UserTreatmentName,
            };
           return userTreatmentViewModel;
        }
        public async Task<UserTreatmentTypeDto> EditUserTreatments(UpdateUserTreatmentTypeDto model)
        {
            if (string.IsNullOrEmpty(model.UserTreatmentName))
            {
                throw new UserFriendlyException("the provided information are not valid to edit a User Treatments");
            }
                
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.UserTreatmentName), model.UserTreatmentName }
            };
            await _userTreatmentTypeRepository.UpdateUserTreatmentTypeAsync(pairs,model.Id);
            var updatedEntity = await _userTreatmentTypeRepository.FindAsync(model.Id);
            var result = _objectMapper.Map<Entities.UserTreatmentType, UserTreatmentTypeDto>(updatedEntity);
            return result;
        }
        public async Task<bool> DeleteUserTreatments(UserTreatmentTypeDto model)
        {
            return await _userTreatmentTypeRepository.DeleteAsync(model.Id);
        }

        public async Task<List<UserTreatmentTypeDto>> GetAllAsync()
        {
            var items = await _userTreatmentTypeRepository.GetQueryableAsync();
            return items.Select(x => new UserTreatmentTypeDto
            {
                Id = x.Id,
                UserTreatmentName = x.UserTreatmentName
            }).ToList();
        }
    }
}
