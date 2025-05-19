using LinqKit;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class FilterUnitOfWork : IFilterUnitOfWork
    {
        public FilterUnitOfWork(IRepository<ScenarioModel> scenarioRepo, IProjectRepository projectRepo,
            ITreatmentRepository treatmentRepo, IRepository<CountyModel> countyRepo, IRepository<CargoAttributesModel> cargoAttributesRepo, IRepository<CargoDataModel> cargoDataRepo, IRepository<TreatmentTypeModel> treatmentTypeRepo, IProjectTreatmentRepository projectTreatmentRepo, ICandidatePoolRepository libraryRepo, IUserRepository user)
        {                          
            ScenarioRepo = scenarioRepo;
            ProjectRepo = projectRepo;
            CountyRepo = countyRepo;
            CargoAttributesRepo = cargoAttributesRepo;
            CargoDataRepo = cargoDataRepo;
            TreatmentRepo = treatmentRepo;
            CandidatePoolRepo = libraryRepo;
            TreatmentTypeRepo = treatmentTypeRepo;
            ProjectTreatmentRepo = projectTreatmentRepo;
            UserRepo = user;    
            ScenarioRepo.ErrorOccured += RaiseOnErrorOccured;
            ProjectRepo.ErrorOccured += RaiseOnErrorOccured;
            CountyRepo.ErrorOccured += RaiseOnErrorOccured;
            CargoAttributesRepo.ErrorOccured += RaiseOnErrorOccured;
            CargoDataRepo.ErrorOccured += RaiseOnErrorOccured;
            TreatmentRepo.ErrorOccured += RaiseOnErrorOccured;
            CandidatePoolRepo.ErrorOccured += RaiseOnErrorOccured;
        }
        public IRepository<ScenarioModel> ScenarioRepo { get; }
        public IProjectRepository ProjectRepo { get; }
        public IRepository<CountyModel> CountyRepo { get; }
        public IRepository<CargoAttributesModel> CargoAttributesRepo { get; }
        public IRepository<CargoDataModel> CargoDataRepo { get; }
        public IRepository<TreatmentTypeModel> TreatmentTypeRepo { get; }
        public ITreatmentRepository TreatmentRepo { get; }
        public ICandidatePoolRepository CandidatePoolRepo { get; }
        public bool IsPending { get { return ScenarioRepo.IsPending | ProjectRepo.IsPending | CountyRepo.IsPending | CargoAttributesRepo.IsPending | CargoDataRepo.IsPending | TreatmentRepo.IsPending | CandidatePoolRepo.IsPending; } }

        public IProjectTreatmentRepository ProjectTreatmentRepo { get; }

        public IUserRepository UserRepo { get; }

        public event Action<ErrorEventArgs> ErrorOccured;

        public void ClearPendingOperations()
        {
            ScenarioRepo.ClearPendingChanges();
            ProjectRepo.ClearPendingChanges();
            CountyRepo.ClearPendingChanges();
            CargoAttributesRepo.ClearPendingChanges();
            CargoDataRepo.ClearPendingChanges();
            TreatmentRepo.ClearPendingChanges();
            CandidatePoolRepo.ClearPendingChanges();
        }
        public async Task SaveChangesAsync(CancellationToken token = default)
        {
            if (IsPending)
            {
                await ScenarioRepo.SaveChangesAsync(token);
            }
        }

        private void RaiseOnErrorOccured(ErrorEventArgs error)
        {
            ErrorOccured?.Invoke(error);
        }
    }

}