using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface IFilterUnitOfWork : IUnitOfWork
    {
        public IRepository<ScenarioModel> ScenarioRepo { get;}
        public IProjectRepository ProjectRepo { get;}
        public IRepository<CountyModel> CountyRepo { get; }
        public IRepository<CargoAttributesModel> CargoAttributesRepo { get; }
        public IRepository<CargoDataModel> CargoDataRepo { get; }
        public IRepository<TreatmentTypeModel> TreatmentTypeRepo { get; }        
        public IProjectTreatmentRepository ProjectTreatmentRepo { get; }
        public ICandidatePoolRepository CandidatePoolRepo { get;}
        public ITreatmentRepository TreatmentRepo { get;}
        public IUserRepository UserRepo { get; }
       
    }
}
