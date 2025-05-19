using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ProjectBuilder.Core;
using System;
using ProjectBuilder.Core.Models;
using ProjectBuilder.DataAccess.Entities;
using System.Linq;
using System.Reflection;

namespace ProjectBuilder.DataAccess
{
    public static class RegisterDependencies
    {
        public static IServiceCollection RegisterAutoMapper(this IServiceCollection services)
        {
            services.RegisterProfiles()
                    .AddSingleton(sr =>
                    {
                        var profiles = sr.GetServices<Profile>();
                        var config = new MapperConfiguration(cfg => cfg?.AddProfiles(profiles));
                        return config.CreateMapper();
                    });
            return services;
        }
        private static IServiceCollection RegisterProfiles(this IServiceCollection serviceCollection)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly is not null)
             assembly.GetTypes().Where(type => type.IsClass && type.Name.EndsWith("Profile"))
                                .ToList()
                                .ForEach(type => serviceCollection.AddTransient(typeof(Profile), type));
            return serviceCollection;
        }
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<IRepository<ScenarioModel>, ProjectBuilderRepository<Scenario, ScenarioModel, int>>()
                    .AddTransient<ITreatmentRepository, TreatmentRepository>()
                    .AddTransient<IRepository<UserRoleModel>, ProjectBuilderRepository<UserRoleEntity, UserRoleModel, int>>()
                    .AddTransient<IRepository<CountyModel>, ProjectBuilderRepository<CountyEntity, CountyModel, int?>>()
                    .AddTransient<IRepository<CargoAttributesModel>, ProjectBuilderRepository<CargoAttributes, CargoAttributesModel, int>>()
                    .AddTransient<IRepository<CargoDataModel>, ProjectBuilderRepository<CargoData, CargoDataModel, Guid>>()
                    .AddTransient<IProjectTreatmentRepository, ProjectTreatmentRepository>()
                    .AddTransient<IRepository<PAMSSectionSegmentModel>, ProjectBuilderRepository<PAMSSectionSegment, PAMSSectionSegmentModel, Guid>>()
                    .AddTransient<IRepository<TreatmentTypeModel>, ProjectBuilderRepository<TreatmentType, TreatmentTypeModel, string>>()
                    .AddTransient<IProjectRepository, ProjectsRepository>()
                    .AddTransient<IRepository<RoleModel>, ProjectBuilderRepository<RoleEntity, RoleModel, int>>()
                    .AddTransient<IUserRepository, UserRepository>()
                    .AddTransient<IRepository<ScenarioParameterModel>, ScenarioParametersRepository>()
                    .AddTransient<IRepository<ScenarioBudgetFlatModel>, ScenarioBudgetFlattenedRepository>()
                    .AddTransient<ICandidatePoolRepository, CandidatePoolRepository>()
                    .AddTransient<IFilterUnitOfWork, FilterUnitOfWork>()
                    .AddTransient<IRepository<DefaultSlackModel>, ProjectBuilderRepository<DefaultSlack, DefaultSlackModel, string>>()
                    .AddTransient<IRepository<AllNeedsModel>,ProjectBuilderRepository<AllNeedsEntity,AllNeedsModel,int?>>()
                    .AddTransient<IRepository<BridgeNeedsModel>, ProjectBuilderRepository<BridgeNeedsEntity, BridgeNeedsModel, int?>>()
                    .AddTransient<IRepository<PavementNeedsModel>, ProjectBuilderRepository<PavementNeedsEntity, PavementNeedsModel, int?>>()
                    .AddTransient<IRepository<AllPotentialBenefitsModel>, ProjectBuilderRepository<AllPotentialBenefitEntity, AllPotentialBenefitsModel, int?>>()
                    .AddTransient<IRepository<BridgePotentialBenefitsModel>, ProjectBuilderRepository<BridgePotentialBenefitEntity, BridgePotentialBenefitsModel, int?>>()
                    .AddTransient<IRepository<PavementPotentialBenefitsModel>, ProjectBuilderRepository<PavementPotentialBenefitEntity, PavementPotentialBenefitsModel, int?>>()
                    .AddTransient<IRepository<ProjectSummaryModel>, ProjectBuilderRepository<ProjectSummaryEntity, ProjectSummaryModel, int?>>()
                    .AddTransient<IRepository<TreatmentSummaryModel>, ProjectBuilderRepository<TreatmentSummaryEntity, TreatmentSummaryModel, int>>()
                    .AddTransient<IRepository<BudgetModel>, ProjectBuilderRepository<BudgetEntity, BudgetModel, int?>>()
                    .AddTransient<IRepository<BudgetSpentModel>, ProjectBuilderRepository<BudgetSpentEntity, BudgetSpentModel, int?>>()
                    .AddTransient<IRepository<CombinedProjectModel>, ProjectBuilderRepository<CombinedProjectSummaryEntity, CombinedProjectModel, int?>>()
                    .AddTransient<IRepository<CombinedProjectModel>, ProjectBuilderRepository<CombinedProjectSummaryEntity, CombinedProjectModel, int?>>()
                    .AddTransient<IRepository<AraEnumerationsModel>, ProjectBuilderRepository<AraEnumerations, AraEnumerationsModel, string>>()
                    .AddTransient<IRepository<ParameterModel>, ProjectBuilderRepository<ParameterEntity, ParameterModel, string>>()
                    .AddTransient<IRepository<TreatmentParameterModel>, ProjectBuilderRepository<TreatmentParameterEntity, TreatmentParameterModel, int>>()
                    .AddTransient<IRepository<TreatmentCancellationMatrixModel>, ProjectBuilderRepository<TreatmentCancellationMatrixEntity, TreatmentCancellationMatrixModel, string>>()
                    .AddTransient<IRepository<PAMSSectionSegmentModel>, ProjectBuilderRepository<PAMSSectionSegment, PAMSSectionSegmentModel, Guid>>()
                    .AddTransient<IRepository<BridgeToPavementsModel>,ProjectBuilderRepository<BridgeToPavements,BridgeToPavementsModel,string>>()
                    .AddTransient<IRepository<ImportSessionsModel>, ProjectBuilderRepository<ImportSessions, ImportSessionsModel, Guid>>()
                    .AddTransient<IRepository<UserTreatmentTypeModel>, ProjectBuilderRepository<UserTreatmentType, UserTreatmentTypeModel, int>>();
           
            return services;
        }
    }
}
