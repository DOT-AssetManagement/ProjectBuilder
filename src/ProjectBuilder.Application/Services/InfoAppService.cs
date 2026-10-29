using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Helpers;
using ProjectBuilder.Models;
using ProjectBuilder.Models.AraEnumerations;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.Info;
using ProjectBuilder.Models.UserTreatment.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    public  class InfoAppService : ApplicationService, IInfoAppService
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private IProjectRepository _projectRepository;
        private readonly IProjectTreatmentRepository _projectTreatmentsRepo;
        private readonly IObjectMapper _objectMapper;

        public InfoAppService(ITreatmentRepository treatmentRepository, IProjectRepository projectRepository, IProjectTreatmentRepository projectTreatmentsRepo, IObjectMapper objectMapper)
        {
            _treatmentRepository = treatmentRepository;
            _projectRepository = projectRepository;
            _projectTreatmentsRepo = projectTreatmentsRepo;
            _objectMapper = objectMapper;
        }
        public async Task<List<byte?>> GetDistricts(string caller, int? scenarioId)
        {
            switch (caller)
            {
                default:
                    var treatments = await _treatmentRepository.GetListAsync();
                    return treatments.Select(a => a.District).OrderBy(d => d).Distinct().ToList();

                case "/Projects":
                    return await _projectRepository.GetDistricts(scenarioId);

                case "/ProjectTreatments":
                    return await _projectTreatmentsRepo.GetDistricts(scenarioId);
            }
        }
        public async Task<List<CountyEntity>> GetCounties(string caller, int? scenarioId, int? district)
        {
            switch (caller)
            {
                default:
                    return await _treatmentRepository.GetCounties(district);
                case "/Projects":
                    return await _projectRepository.GetCounties(scenarioId, district);
                case "/ProjectTreatments":
                    return await _projectTreatmentsRepo.GetCounties(scenarioId, district);
            }
        }
        public async Task<List<int?>> GetRoutes(string caller, int? scenarioId, int? district, int? county)
        {
            switch (caller)
            {
                default:
                    return await _treatmentRepository.GetRoutes(district, county);
                case "/Projects":
                    return await _projectRepository.GetRoutes(scenarioId, district, county);
                case "/ProjectTreatments":
                    return await _projectTreatmentsRepo.GetRoutes(scenarioId, district, county);
            }
        }
        public async Task<List<string>> GetSections(string caller, int? scenarioId, int? district, int? county, int? route)
        {
            switch (caller)
            {
                default:
                    return await _treatmentRepository.GetSections(district, county, route);
                case "/Projects":
                    return await _projectRepository.GetSections(scenarioId, district, county, route);
                case "/ProjectTreatments":
                    return await _projectTreatmentsRepo.GetSections(scenarioId, district, county, route);
            }
        }
        public async Task<object> GetTreatmentTypes(string caller, int? scenarioId, int? district, int? county, int? route)
        {
            var treatmentFilter = new TreatmentSearchModel
            {
                District = (byte?)district,
                Cnty = (byte?)county,
                Route = route
            };
            switch (caller)
            {
                default:
                    return await _treatmentRepository.FilterTreatments(treatmentFilter);
                case "/Outputs/Maps":
                    return await _treatmentRepository.GetTreatmentTypes(district, county, route);
            }
        }
        public async Task<SegmentationModel> GetDirectionInterstate(string section, int? route, int? county, int? district)
        {
            if (district.HasValue && county.HasValue && route.HasValue)
            {
                var combinedsections = section.DecomposeString<int>('-');
                if (combinedsections.Length == 2)
                {
                    var interstate = await _treatmentRepository.GetDirectionInterstate(district, county, route, combinedsections[0], combinedsections[1]);
                    return interstate;
                }
                else if (combinedsections.Length == 1)
                {
                    var interstate = await _treatmentRepository.GetDirectionInterstate(district, county, route, combinedsections[0], combinedsections[0]);
                    return interstate;
                }
            }
            return default;
        }
        public async Task<DefaultSlackDto> GetDefaultSlackValues(string assettype)
        {
            if (string.IsNullOrEmpty(assettype))
            {
                throw new UserFriendlyException("could not locate default slacks for the selected asset type.");
            }
            var defaultSlack = await _treatmentRepository.GetAssetTypeDefaultSlack(assettype);
            var result = _objectMapper.Map<DefaultSlack, DefaultSlackDto>(defaultSlack);
            return result;
        }
    }
}

