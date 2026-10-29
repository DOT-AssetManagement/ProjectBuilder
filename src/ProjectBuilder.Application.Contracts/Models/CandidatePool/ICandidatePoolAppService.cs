using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.CreateBridgeToPavements.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace ProjectBuilder.Models.CandidatePool
{
    public interface ICandidatePoolAppService : ICrudAppService<CandidatePoolDto, Guid, GetCandidatePoolInput, CreateCandidatePoolDto, UpdateCandidatePoolDto>
    {
        Task<CandidatePoolDto> GetCandidatePoolDataAsync(int? scenid, bool? importing, string searchString = "");
        Task<CandidatePoolDto> ListAsync(int? scenid, bool? importing, Guid userId = default);
        Task<Guid> CreateNewLibrary(CreateCandidatePoolDto libraryViewModel, Guid userId, string? libraraysource = null);
        Task<bool> EditLibraryAsync(UpdateCandidatePoolDto libraryViewModel);
        Task<bool> CheckLibraryNameAsync(string libraryName, string? libraryId);
        Task<string?> ImportTreatmentsAsync(ImportTreatmentViewModel data);
        Task<string?> ImportMASAndB2PAsync(ImportMASAndB2PViewModel data);
        Task<List<CandidatePoolDto>> GetPoolsListAsync();


    }
}
