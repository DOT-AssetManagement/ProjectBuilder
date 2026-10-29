//using AutoMapper;
//using ProjectBuilder.Entities;
//using ProjectBuilder.Models;
//using ProjectBuilder.Models.AraEnumerations.Dtos;
//using ProjectBuilder.Models.CandidatePool.Dtos;
//using ProjectBuilder.Models.Counties.Dtos;
//using ProjectBuilder.Models.CreateBridgeToPavements.Dtos;
//using ProjectBuilder.Models.DefaultSlack.Dtos;
//using ProjectBuilder.Models.ImportSessions.Dtos;
//using ProjectBuilder.Models.PAMSSectionSegment.Dtos;
//using ProjectBuilder.Models.ParameterEntity.Dtos;
//using ProjectBuilder.Models.Project.Dtos;
//using ProjectBuilder.Models.ProjectTreatment.Dtos;
//using ProjectBuilder.Models.Scenario.Dtos;
//using ProjectBuilder.Models.ScenarioBudget.Dtos;
//using ProjectBuilder.Models.ScenarioBudgetFlat.Dtos;
//using ProjectBuilder.Models.ScenarioParamater.Dtos;
//using ProjectBuilder.Models.Treatment.Dtos;
//using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
//using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
//using ProjectBuilder.Models.TreatmentType.Dtos;
//using ProjectBuilder.Models.UserClaimEntity.Dtos;
//using ProjectBuilder.Models.UserTreatment.Dtos;
//using ProjectBuilder.Models.UserTreatmentType.Dtos;

//namespace ProjectBuilder;

//public class ProjectBuilderApplicationAutoMapperProfile : Profile
//{
//    public ProjectBuilderApplicationAutoMapperProfile()
//    {
       
//        /* You can configure your AutoMapper mapping configuration here.
//         * Alternatively, you can split your mapping configurations
//         * into multiple profile classes for a better organization. */

//        #region AraEnumerations

//        CreateMap<AraEnumerations, AraEnumerationDto>().ReverseMap();
//        CreateMap<AraEnumerations, CreateAraEnumerationDto>().ReverseMap();
//        CreateMap<AraEnumerations, UpdateAraEnumerationDto>().ReverseMap();

//        #endregion

//        #region CandidatePool

//        CreateMap<CandidatePool, CandidatePoolDto>().ReverseMap();
//        CreateMap<CandidatePool, CreateCandidatePoolDto>().ReverseMap();
//        CreateMap<CandidatePool, UpdateCandidatePoolDto>().ReverseMap();

//        #endregion

//        #region CountyEntity

//        CreateMap<CountyEntity, CountyDto>().ReverseMap();
//        CreateMap<CountyEntity, CreateCountyDto>().ReverseMap();
//        CreateMap<CountyEntity, UpdateCountyDto>().ReverseMap();

//        #endregion

//        #region BridgeToPavements

//        CreateMap<BridgeToPavements, BridgeToPavementDto>().ReverseMap();
//        CreateMap<BridgeToPavements, CreateBridgeToPavementDto>().ReverseMap();
//        CreateMap<BridgeToPavements, UpdateBridgeToPavementDto>().ReverseMap();

//        #endregion

//        #region DefaultSlack

//        CreateMap<DefaultSlack, DefaultSlackDto>().ReverseMap();
//        CreateMap<DefaultSlack, CreateDefaultSlackDto>().ReverseMap();
//        CreateMap<DefaultSlack, UpdateDefaultSlackDto>().ReverseMap();

//        #endregion


//        #region ImportSessions

//        CreateMap<ImportSessions, ImportSessionDto>().ReverseMap();
//        CreateMap<ImportSessions, CreateImportSessionDto>().ReverseMap();
//        CreateMap<ImportSessions, UpdateImportSessionDto>().ReverseMap();

//        #endregion

//        #region PAMSSectionSegment

//        CreateMap<PAMSSectionSegment, PAMSSectionSegmentDto>().ReverseMap();
//        CreateMap<PAMSSectionSegment, CreatePAMSSectionSegmentDto>().ReverseMap();
//        CreateMap<PAMSSectionSegment, UpdatePAMSSectionSegmentDto>().ReverseMap();

//        #endregion

//        #region ParameterEntity

//        CreateMap<ParameterEntity, ParameterDto>().ReverseMap();
//        CreateMap<ParameterEntity, CreateParameterDto>().ReverseMap();
//        CreateMap<ParameterEntity, UpdateParameterDto>().ReverseMap();

//        #endregion

//        #region Project

//        CreateMap<Project, ProjectDto>().ReverseMap();
//        CreateMap<Project, CreateProjectDto>().ReverseMap();
//        CreateMap<Project, UpdateProjectDto>().ReverseMap();

//        #endregion

//        #region ProjectTreatment

//        CreateMap<ProjectTreatment, ProjectTreatmentDto>().ReverseMap();
//        CreateMap<ProjectTreatment, CreateProjectTreatmentDto>().ReverseMap();
//        CreateMap<ProjectTreatment, UpdateProjectTreatmentDto>().ReverseMap();

//        #endregion


//        #region Scenario

//        CreateMap<Scenario, ScenarioDto>().ReverseMap();
//        CreateMap<Scenario, CreateScenarioDto>().ReverseMap();
//        CreateMap<Scenario, UpdateScenarioDto>().ReverseMap();

//        #endregion



//        #region ScenarioBudget

//        CreateMap<ScenarioBudget, ScenarioBudgetDto>().ReverseMap();
//        CreateMap<ScenarioBudget, CreateScenarioBudgetDto>().ReverseMap();
//        CreateMap<ScenarioBudget, UpdateScenarioBudgetDto>().ReverseMap();

//        #endregion

//        #region ScenarioBudgetFlat

//        CreateMap<ScenarioBudgetFlat, ScenarioBudgetFlatDto>().ReverseMap();
//        CreateMap<ScenarioBudgetFlat, CreateScenarioBudgetFlatDto>().ReverseMap();
//        CreateMap<ScenarioBudgetFlat, UpdateScenarioBudgetFlatDto>().ReverseMap();

//        #endregion


//        #region ScenarioParamater

//        CreateMap<ScenarioParamater, ScenarioParameterDto>().ReverseMap();
//        CreateMap<ScenarioParamater, CreateScenarioParameterDto>().ReverseMap();
//        CreateMap<ScenarioParamater, UpdateScenarioParameterDto>().ReverseMap();

//        #endregion

//        #region Treatment

//        CreateMap<Treatment, TreatmentDto>().ReverseMap();
//        CreateMap<Treatment, CreateTreatmentDto>().ReverseMap();
//        CreateMap<Treatment, UpdateTreatmentDto>().ReverseMap();

//        #endregion

//        #region TreatmentCancellationMatrixEntity

//        CreateMap<TreatmentCancellationMatrixEntity, TreatmentCancellationMatrixDto>().ReverseMap();
//        CreateMap<TreatmentCancellationMatrixEntity, CreateTreatmentCancellationMatrixDto>().ReverseMap();
//        CreateMap<TreatmentCancellationMatrixEntity, UpdateTreatmentCancellationMatrixDto>().ReverseMap();

//        #endregion

//        #region TreatmentParameterEntity

//        CreateMap<TreatmentParameterEntity, TreatmentParameterDto>().ReverseMap();
//        CreateMap<TreatmentParameterEntity, CreateTreatmentParameterDto>().ReverseMap();
//        CreateMap<TreatmentParameterEntity, UpdateTreatmentParameterDto>().ReverseMap();

//        #endregion

//        #region TreatmentType

//        CreateMap<TreatmentType, TreatmentTypeDto>().ReverseMap();
//        CreateMap<TreatmentType, CreateTreatmentTypeDto>().ReverseMap();
//        CreateMap<TreatmentType, UpdateTreatmentTypeDto>().ReverseMap();

//        #endregion

//        #region UserClaimEntity

//        CreateMap<UserClaimEntity, UserClaimDto>().ReverseMap();
//        CreateMap<UserClaimEntity, CreateUserClaimDto>().ReverseMap();
//        CreateMap<UserClaimEntity, UpdateUserClaimDto>().ReverseMap();

//        #endregion

//        #region UserTreatment

//        CreateMap<UserTreatment, UserTreatmentDto>().ReverseMap();
//        CreateMap<UserTreatment, CreateUserTreatmentDto>().ReverseMap();
//        CreateMap<UserTreatment, UpdateUserTreatmentDto>().ReverseMap();

//        #endregion


//        #region UserTreatmentType

//        CreateMap<UserTreatmentType, UserTreatmentTypeDto>().ReverseMap();
//        CreateMap<UserTreatmentType, CreateUserTreatmentTypeDto>().ReverseMap();
//        CreateMap<UserTreatmentType, UpdateUserTreatmentTypeDto>().ReverseMap();

//        #endregion


//        #region User

//        CreateMap<UserEntity, UserModel>().ReverseMap();
//        #endregion
//    }
//}
