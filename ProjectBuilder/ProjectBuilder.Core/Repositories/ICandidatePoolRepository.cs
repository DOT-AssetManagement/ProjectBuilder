using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
	public interface ICandidatePoolRepository : IRepository<CandidatePoolModel>
	{
		public long UserId { get; set; }
		Task<Dictionary<Guid, string>> GetSource(List<Guid> libraryIds);
		//Task<Guid?> CheckStale(int? scenId);

	}
}
