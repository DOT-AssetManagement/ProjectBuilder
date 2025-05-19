using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Runtime.InteropServices;
using System.Data.Entity;

namespace ProjectBuilder.DataAccess
{
	public class CandidatePoolRepository : ProjectBuilderRepository<CandidatePool, CandidatePoolModel, Guid>, ICandidatePoolRepository
	{
		private ProjectBuilderDbContext _context;
		public long UserId { get; set; }

		public CandidatePoolRepository(ProjectBuilderDbContext projectBuilderDbContext, IMapper mapper, ILogger<ProjectBuilderRepository<CandidatePool, CandidatePoolModel, Guid>> logger)
			: base(projectBuilderDbContext, mapper, logger)
		{
			_context = projectBuilderDbContext;
		}
		protected override IQueryable<CandidatePool> InitializeCurrrentQuery(Expression<Func<CandidatePool, bool>> filter = null)
		{
			if (filter == null)
				filter = PredicateBuilder.New<CandidatePool>(l => (l.UserId == UserId || l.IsShared) && (l.IsActive.HasValue ? l.IsActive.Value : false));
			return ProjectBuilderDbContext.Libraries.Where(filter).OrderBy(e => e.EntityId).Join(ProjectBuilderDbContext.Users, l => l.UserId, u => u.EntityId, (l, u) => new CandidatePool
			{
				IsActive = l.IsActive,
				EntityId = l.EntityId,
				Description = l.Description,
				IsShared = l.IsShared,
				CandidatePoolNumber = l.CandidatePoolNumber,
				Name = l.Name,
				UserId = l.UserId,
				Owner = u.Name,
				CreatedAt = l.CreatedAt,
				PopulatedAt = ProjectBuilderDbContext.CustomTreatments.Where(t => t.LibraryId == l.EntityId).Select(t => t.PopulatedAt).FirstOrDefault(),
				BridgeTreatmentsCount = ProjectBuilderDbContext.CustomTreatments.Count(t => t.LibraryId == l.EntityId && t.AssetType == "B"),
				PavementTreatmentsCount = ProjectBuilderDbContext.CustomTreatments.Count(t => t.LibraryId == l.EntityId && t.AssetType == "P"),
				TreatmentsCount = ProjectBuilderDbContext.CustomTreatments.Count(t => t.LibraryId == l.EntityId),
				ScenarioCount = ProjectBuilderDbContext.Scenarios.Count(s => s.LibraryId == l.EntityId)
			});
		}

		public async Task<Dictionary<Guid, string>> GetSource(List<Guid> libraryIds)
		{
			try
			{
				var sources = new Dictionary<Guid, string>();
				string sourceBAMS, sourcePAMS = "";
				string source = "";
				foreach (var pool in libraryIds)
				{
					sourceBAMS = "";
					sourcePAMS = "";
					source = "";
					var userTreatmentBAMS = ProjectBuilderDbContext.CustomTreatments.FirstOrDefault(t => t.LibraryId == pool && t.AssetType == "B");
					var userTreatmentPAMS = ProjectBuilderDbContext.CustomTreatments.FirstOrDefault(t => t.LibraryId == pool && t.AssetType == "P");
					if (userTreatmentBAMS != default)
					{
						Guid importTimeGeneratedId = userTreatmentBAMS.EntityId;
						string assetType = userTreatmentBAMS.AssetType;
						string targetTable = "tbl_import_BAMS_Treatments";

						try
						{
							using (SqlConnection conn = new SqlConnection(ProjectBuilderDbContext.Database.GetConnectionString()))
							{
								conn.Open();
								using (SqlCommand command = new SqlCommand())
								{
									command.Connection = conn;
									command.CommandText = $"Select TOP 1 ImportSource FROM {targetTable} bt" +
										$" JOIN tbl_pb_ImportSessions ims ON ims.Id = bt.ImportSessionId" +
										$" WHERE bt.ImportTimeGeneratedId = '{importTimeGeneratedId}' ";
									command.CommandType = CommandType.Text;
									command.CommandTimeout = 12000;
									sourceBAMS = (string)command.ExecuteScalar();
								}
							}
						}

						catch (Exception ex)
						{
							throw;
						}
					}

					if (userTreatmentPAMS != default)
					{
						Guid importTimeGeneratedId = userTreatmentPAMS.EntityId;
						string assetType = userTreatmentPAMS.AssetType;
						string targetTable = "tbl_import_PAMS_Treatments";

						try
						{
							using (SqlConnection conn = new SqlConnection(ProjectBuilderDbContext.Database.GetConnectionString()))
							{
								conn.Open();
								using (SqlCommand command = new SqlCommand())
								{
									command.Connection = conn;
									command.CommandText = $"Select TOP 1 ImportSource FROM {targetTable} bt" +
										$" JOIN tbl_pb_ImportSessions ims ON ims.Id = bt.ImportSessionId" +
										$" WHERE bt.ImportTimeGeneratedId = '{importTimeGeneratedId}' ";
									command.CommandType = CommandType.Text;
									command.CommandTimeout = 12000;
									sourcePAMS = (string)command.ExecuteScalar();
								}
							}
						}

						catch (Exception ex)
						{
							throw;
						}
					}

					source = !string.IsNullOrEmpty(sourceBAMS) ? sourceBAMS : "";
					source = !string.IsNullOrEmpty(sourcePAMS) ? !string.IsNullOrEmpty(source)
						? source + ", " + sourcePAMS : sourcePAMS : source;

					sources.Add(pool, source);

				}
				return sources;
			}
			catch (Exception ex)
			{
				throw;

			}

		}


		//public async Task<Guid?> CheckStale(int? scenId)
		//{
		//	try
		//	{
		//		Guid? libraryId = _context.Scenarios
		//			.Where(a => a.EntityId == scenId)
		//			.Select(a => a.LibraryId)
		//			.FirstOrDefault();

		//		return libraryId;
		//	}
		//	catch (Exception)
		//	{
		//		throw;
		//	}
		//}



	}
}
