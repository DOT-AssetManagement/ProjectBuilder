using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.Counties;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.CreateBridgeToPavements.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
using ProjectBuilder.Permissions;
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
    [Authorize(SetUpLookupsPermissions.Counties)]
    public class CountyAppService : ApplicationService, ICountyAppService
    {
        private readonly ICountyRepository _countyRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public CountyAppService(ICountyRepository countyRepository, ITreatmentRepository treatmentRepository, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _countyRepository = countyRepository;
            _treatmentRepository = treatmentRepository;
            _objectMapper = objectMapper;
            _mapper = mapper;
        }

        public async Task<PaginatedDto<CountyDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var counties = await _countyRepository.GetQueryableAsync();
            var treatments = await _treatmentRepository.GetQueryableAsync();
            var ListNewCounty = new List<CountyDto>();
            foreach (var item in counties)
            {
                var NewCounty = new CountyDto();
                NewCounty.Id = item.Id;
                NewCounty.CountyName = item.CountyName;
                NewCounty.District = item.District;
                NewCounty.CountyFullName = item.CountyFullName;
                {
                    // Deleteable only when no treatment still references this county —
                    // this was previously inverted (true meant "has dependents").
                    var Contain = treatments.Any(a => a.CountyId == item.Id);
                    NewCounty.IsDeleteable = !Contain;
                }
                ListNewCounty.Add(NewCounty);
            }
            if (param.sSearch != null)
            {
                ListNewCounty = ListNewCounty.Where(
                   t =>
                   (t.CountyName.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.District.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.CountyFullName.ToLower().Contains(param.sSearch.ToLower()))
                    ).ToList();
            }

            var displayResult = ListNewCounty.Skip(param.iDisplayStart)
                     .Take(param.iDisplayLength).ToList();
            var totalRecords = ListNewCounty.Count();

            return new PaginatedDto<CountyDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = displayResult
            };

        }
        public async Task<List<CountyDto>> NewCounty()
        {
            var counties = await _countyRepository.GetListAsync();
            var treatments = await _treatmentRepository.GetListAsync();
            var ListNewCounty = new List<CountyDto>();
            foreach (var item in counties)
            {
                var NewCounty = new CountyDto();
                NewCounty.Id = item.Id;
                NewCounty.CountyName = item.CountyName;
                NewCounty.District = item.District;
                NewCounty.CountyFullName = item.CountyFullName;
                {
                    var Contain = treatments.Any(a => a.CountyId == item.Id);
                    NewCounty.IsDeleteable = !Contain;
                }
                ListNewCounty.Add(NewCounty);
            }
            return ListNewCounty;
        }
        public async Task<CountyDto> GetCountyAsync(int district, int id)
        {
            var county = await _countyRepository.FindAsync(district, id);
            if (county == null)
            {
                throw new UserFriendlyException("County not found");
            }

            return new CountyDto
            {
                Id = county.Id,
                CountyName = county.CountyName,
                District = county.District
            };
        }
        public async Task<CountyDto> EditCountyAsync(UpdateCountyDto model)
        {
            if (string.IsNullOrWhiteSpace(model.CountyName))
            {
                throw new UserFriendlyException("The provided information is not valid to edit a county.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(model.CountyName.Trim(), @"^[A-Za-z ]+$"))
            {
                throw new UserFriendlyException("County name can only contain letters and spaces.");
            }

            var updateValues = new Dictionary<string, object>
            {
                { nameof(model.CountyName), model.CountyName }
            };

            await _countyRepository.UpdateCountyAsync(updateValues, model.District, model.Id);

            var updatedCounty = await _countyRepository.FindAsync(model.District, model.Id);
            var result = _mapper.Map(updatedCounty);
            return result;

        }
        public async Task<bool> CheckCounty(int counId)
        {
            var treatments = await _treatmentRepository.GetListAsync();
            var NewTreat = treatments.Any(a => a.CountyId == counId);
            return NewTreat;
        }
        public async Task<bool> DeleteCounty(int countyId, int district)
        {
            // Mirrors the client-side guard (which disables the button for counties still
            // referenced by a treatment) so a direct/bypassed post can't hit an unhandled
            // FK violation in the database instead.
            var treatments = await _treatmentRepository.GetListAsync();
            if (treatments.Any(a => a.CountyId == countyId))
            {
                throw new UserFriendlyException("This county cannot be deleted because it is still referenced by a treatment.");
            }

            return await _countyRepository.DeleteCountyAsync(district, countyId);
        }

        // Shared lookup used by non-admin flows (e.g. WorkCandidates Create).
        // Bypass the class-level Counties setup permission; the calling pages are [Authorize].
        [AllowAnonymous]
        public async Task<List<CountyDto>> GetAllDistricts()
        {
            var districts = await _countyRepository.GetAllDistricts();
            return _mapper.Map(districts);
        }

        // Shared lookup used by non-admin flows (e.g. WorkCandidates Create).
        [AllowAnonymous]
        public async Task<List<CountyDto>> GetCountiesByDistrict(int id)
        {
            var counties = await _countyRepository.GetCountiesByDistrict(id);
            return _mapper.Map(counties);
        }

        public async Task<List<CountyDto>> GetCounties()
        {
            var counties = await _countyRepository.GetCounties();
            var referencedCountyIds = new HashSet<int?>((await _treatmentRepository.GetTreatmentCountyIds()).Select(id => (int?)id));
            var countyDtos = _mapper.Map(counties);
            foreach (var countyDto in countyDtos)
            {
                countyDto.IsDeleteable = !referencedCountyIds.Contains(countyDto.Id);
            }
            return countyDtos;
        }
    }
}
