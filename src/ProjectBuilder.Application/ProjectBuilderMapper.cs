using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.AssetTypeParameter.Dtos;
using ProjectBuilder.Models.BridgeReferenceData.Dtos;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.CreateBridgeToPavements.Dtos;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.Goal.Dtos;
using ProjectBuilder.Models.ImportSessions.Dtos;
using ProjectBuilder.Models.Measure.Dtos;
using ProjectBuilder.Models.OtherParameter.Dtos;
using ProjectBuilder.Models.PAMSSectionSegment.Dtos;
using ProjectBuilder.Models.ParameterEntity.Dtos;
using ProjectBuilder.Models.Project.Dtos;
using ProjectBuilder.Models.ProjectCandidate.Dtos;
using ProjectBuilder.Models.ProjectTreatment.Dtos;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Models.ScenarioBudget.Dtos;
using ProjectBuilder.Models.ScenarioBudgetFlat.Dtos;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
using ProjectBuilder.Models.TreatmentType.Dtos;
using ProjectBuilder.Models.UserClaimEntity.Dtos;
using ProjectBuilder.Models.UserTreatment.Dtos;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
using ProjectBuilder.Models.WorkCandidates.Dtos;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using Volo.Abp.Mapperly;

namespace ProjectBuilder;

[Mapper]
public partial class ProjectBuilderMapper
{
    // Core mappings used in your CandidatePoolAppService
    public partial CandidatePoolDto Map(CandidatePool source);
    public partial List<CandidatePoolDto> Map(List<CandidatePool> source);   // explicit for List

    // DSS pool mappings (manual — property name differences: LibNo, IsSharedLibrary, Guid UserId)
    public CandidatePoolDto Map(UserPoolEntity source) => new CandidatePoolDto
    {
        Id = source.Id,
        CandidatePoolNumber = source.LibNo,
        UserId = source.UserId,
        Name = source.Name,
        Description = source.Description,
        CreatedBy = source.CreatedBy,
        CreatedAt = source.CreatedAt,
        IsActive = source.IsActive,
        IsShared = source.IsSharedLibrary ?? false,
        SourcePoolId = source.SourcePoolId,
        UpdatedBy = source.UpdatedBy,
        UpdatedAt = source.UpdatedAt
    };
    public List<CandidatePoolDto> Map(List<UserPoolEntity> source) => source.ConvertAll(Map);

    public partial ScenarioDto Map(Scenario source);
    public partial List<ScenarioDto> Map(List<Scenario> source);
    public partial List<ScenarioParameterDto> Map(List<ScenarioParamater> source);
    public partial List<ScenarioBudgetFlatDto> Map(List<ScenarioBudgetFlat> source);

    public partial ScenarioBudgetFlat Map(ScenarioBudgetFlatDto source);
    public partial ScenarioBudgetFlat Map(UpdateScenarioBudgetFlatDto source);

    // ====================== Standard Entity → Dto mappings ======================
    public partial AraEnumerationDto Map(AraEnumerations source);
    public partial CountyDto Map(CountyEntity source);
    public partial BridgeToPavementDto Map(BridgeToPavements source);
    public partial BridgeReferenceDataDto Map(BridgeReferenceData source);
    public partial List<BridgeReferenceDataDto> Map(List<BridgeReferenceData> source);
    public partial DefaultSlackDto Map(DefaultSlack source);
    public partial ImportSessionDto Map(ImportSessions source);
    public partial PAMSSectionSegmentDto Map(PAMSSectionSegment source);
    public partial List<PAMSSectionSegmentDto> Map(List<PAMSSectionSegment> source);
    public partial ParameterDto Map(ParameterEntity source);
    // SequenceNo is computed at read time (see ProjectAppService), not a real column.
    [MapperIgnoreTarget(nameof(ProjectDto.SequenceNo))]
    public partial ProjectDto Map(Project source);
    public partial List<ProjectDto> Map(List<Project> source);
    public partial ProjectTreatmentDto Map(ProjectTreatment source);
    public partial ScenarioBudgetDto Map(ScenarioBudget source);
    public partial ScenarioBudgetFlatDto Map(ScenarioBudgetFlat source);
    public partial ScenarioParameterDto Map(ScenarioParamater source);
    public partial TreatmentDto Map(Treatment source);
    public partial TreatmentCancellationMatrixDto Map(TreatmentCancellationMatrixEntity source);
    public partial TreatmentParameterDto Map(TreatmentParameterEntity source);
    public partial TreatmentTypeDto Map(TreatmentType source);
    public partial UserClaimDto Map(UserClaimEntity source);
    // County is a [NotMapped] CountyEntity navigation (null at runtime) on the entity,
    // but a string on the DTO — let the caller populate it from a county lookup.
    [MapperIgnoreTarget(nameof(UserTreatmentDto.County))]
    public partial UserTreatmentDto Map(UserTreatment source);
    public partial UserTreatmentTypeDto Map(UserTreatmentType source);
    public partial UserModel Map(UserEntity source);

    // ====================== Create / Update DTO mappings ======================
    public partial CreateCandidatePoolDto MapToCreate(CandidatePool source);
    public partial UpdateCandidatePoolDto MapToUpdate(CandidatePool source);

    public partial CreateAraEnumerationDto MapToCreate(AraEnumerations source);
    public partial UpdateAraEnumerationDto MapToUpdate(AraEnumerations source);

    public partial CreateCountyDto MapToCreate(CountyEntity source);
    public partial UpdateCountyDto MapToUpdate(CountyEntity source);

    public partial CreateBridgeToPavementDto MapToCreate(BridgeToPavements source);
    public partial UpdateBridgeToPavementDto MapToUpdate(BridgeToPavements source);

    public partial CreateDefaultSlackDto MapToCreate(DefaultSlack source);
    public partial UpdateDefaultSlackDto MapToUpdate(DefaultSlack source);

    public partial CreateImportSessionDto MapToCreate(ImportSessions source);
    public partial UpdateImportSessionDto MapToUpdate(ImportSessions source);

    public partial CreatePAMSSectionSegmentDto MapToCreate(PAMSSectionSegment source);
    public partial UpdatePAMSSectionSegmentDto MapToUpdate(PAMSSectionSegment source);

    public partial CreateParameterDto MapToCreate(ParameterEntity source);
    public partial UpdateParameterDto MapToUpdate(ParameterEntity source);

    public partial CreateProjectDto MapToCreate(Project source);
    public partial UpdateProjectDto MapToUpdate(Project source);

    public partial CreateProjectTreatmentDto MapToCreate(ProjectTreatment source);
    public partial UpdateProjectTreatmentDto MapToUpdate(ProjectTreatment source);

    public partial CreateScenarioDto MapToCreate(Scenario source);
    public partial UpdateScenarioDto MapToUpdate(Scenario source);

    public partial CreateScenarioBudgetDto MapToCreate(ScenarioBudget source);
    public partial UpdateScenarioBudgetDto MapToUpdate(ScenarioBudget source);

    public partial CreateScenarioBudgetFlatDto MapToCreate(ScenarioBudgetFlat source);
    public partial UpdateScenarioBudgetFlatDto MapToUpdate(ScenarioBudgetFlat source);

    public partial CreateScenarioParameterDto MapToCreate(ScenarioParamater source);
    public partial UpdateScenarioParameterDto MapToUpdate(ScenarioParamater source);

    public partial CreateTreatmentDto MapToCreate(Treatment source);
    public partial UpdateTreatmentDto MapToUpdate(Treatment source);

    public partial CreateTreatmentCancellationMatrixDto MapToCreate(TreatmentCancellationMatrixEntity source);
    public partial UpdateTreatmentCancellationMatrixDto MapToUpdate(TreatmentCancellationMatrixEntity source);

    public partial CreateTreatmentParameterDto MapToCreate(TreatmentParameterEntity source);
    public partial UpdateTreatmentParameterDto MapToUpdate(TreatmentParameterEntity source);

    public partial CreateTreatmentTypeDto MapToCreate(TreatmentType source);
    public partial UpdateTreatmentTypeDto MapToUpdate(TreatmentType source);

    public partial CreateUserClaimDto MapToCreate(UserClaimEntity source);
    public partial UpdateUserClaimDto MapToUpdate(UserClaimEntity source);

    public partial CreateUserTreatmentDto MapToCreate(UserTreatment source);
    public partial UpdateUserTreatmentDto MapToUpdate(UserTreatment source);

    public partial CreateUserTreatmentTypeDto MapToCreate(UserTreatmentType source);
    public partial UpdateUserTreatmentTypeDto MapToUpdate(UserTreatmentType source);

    public partial WorkCandidatesDto Map(WorkCandidateEntity source);
    public partial List<WorkCandidatesDto> Map(List<WorkCandidateEntity> source);
    public partial WorkCandidateEntity Map(CreateWorkCandidatesDto source);

    public partial List<CountyDto> Map(List<CountyEntity> source);

    public partial List<DefaultSlackDto> Map(List<DefaultSlack> source);

    public partial List<BridgeToPavementDto> Map(List<BridgeToPavements> source);

    // Goal
    public partial GoalDto Map(GoalEntity source);
    public partial List<GoalDto> Map(List<GoalEntity> source);
    public partial GoalEntity Map(CreateGoalDto source);

    // Measure
    public partial MeasureDto Map(MeasureEntity source);
    public partial List<MeasureDto> Map(List<MeasureEntity> source);
    public partial MeasureEntity Map(CreateMeasureDto source);

    // ProjectCandidate
    public partial ProjectCandidateDto Map(ProjectCandidateEntity source);
    public partial List<ProjectCandidateDto> Map(List<ProjectCandidateEntity> source);
    public partial ProjectCandidateEntity Map(CreateProjectCandidateDto source);
    public partial ProjectCandidateEntity Map(UpdateProjectCandidateDto source);

    // OtherParameter - the entity key is the Code column, exposed as Code on the DTOs
    [MapProperty(nameof(OtherParameterEntity.Id), nameof(OtherParameterDto.Code))]
    public partial OtherParameterDto Map(OtherParameterEntity source);
    public partial List<OtherParameterDto> Map(List<OtherParameterEntity> source);
    // Manual - the entity key (Code) is only settable through the constructor
    public OtherParameterEntity Map(CreateOtherParameterDto source) => new OtherParameterEntity(source.Code)
    {
        Description = source.Description,
        Val = source.Val,
        CreatedBy = source.CreatedBy,
        CreatedAt = source.CreatedAt
    };

    // AssetTypeParameter
    public partial AssetTypeParameterDto Map(AssetTypeParameterDataEntity source);
    public partial List<AssetTypeParameterDto> Map(List<AssetTypeParameterDataEntity> source);
    public partial AssetTypeParameterDataEntity Map(CreateAssetTypeParameterDto source);

}