using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Repositories
{
    public class ScenarioParametersRepository : EfCoreRepository<ProjectBuilderDbContext,ScenarioParamater, string>, IScenarioParametersRepository
    {
        private readonly ProjectBuilderDbContext _context;
        private readonly IObjectMapper _objectMapper;

        public ScenarioParametersRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ILogger<EfCoreRepository<ProjectBuilderDbContext, ScenarioParamater, string>> logger, ProjectBuilderDbContext dbContext, IObjectMapper objectMapper)
                                     : base(projectBuilderDbContext)
        {
            _context = dbContext;
            _objectMapper = objectMapper;
        }
        public async Task UpdateAsync(ScenarioParamater newValue, params string[] properties)
        {
            //var newEntity = Mapper.Map<ScenarioParamater>(newValue);
            var target = await _context.ScenarioParameters.FindAsync(newValue.ScenarioId, newValue.Id);
            if (target is null)
            {
              //  RaiseOnErrorOccured("Could not update the value in the database.", LogLevel.Error);
                return;
            }
            target.ParameterValue = newValue.ParameterValue;
        }
        protected  async Task<bool> HandleUpdatingConflicts(IReadOnlyList<EntityEntry> entries, CancellationToken token = default)
        {
            foreach (var entry in entries)
            {
                if (entry.Entity as ScenarioParamater is null)
                    continue;
                _context.ScenarioParameters.Add(entry.Entity as ScenarioParamater);
            }
            try
            {
                await _context.SaveChangesAsync(token);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<ScenarioParamater> FindScenarioParameter(int? scenid , string parameterid)
        {
            // Use the unit-of-work DbContext (via the provider) rather than the
            // separately-injected _context. Reading on a different connection than
            // the one that just wrote (within the same transactional UoW) blocks on
            // the uncommitted write locks and times out.
            var dbContext = await GetDbContextAsync();
            return await dbContext.ScenarioParameters
                .FirstOrDefaultAsync(sp => sp.ScenarioId == scenid && sp.Id == parameterid);
        }
        public async Task<bool> DeleteScenarioParameterAsync(int scenid, string scenparamid)
        {
            var entity = await DbContext.ScenarioParameters
                .FirstOrDefaultAsync(p => p.ScenarioId == scenid && p.Id == scenparamid);

            if (entity == null)
                return false;

            DbContext.ScenarioParameters.Remove(entity);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<ScenarioParamater>> GetByScenarioAsync(int scenid)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.ScenarioParameters
                .Where(p => p.ScenarioId == scenid)
                .ToListAsync();
        }

        public async Task<List<TreatmentShiftRow>> GetTreatmentShiftsAsync(int scenid)
        {
            var dbContext = await GetDbContextAsync();

            // ProjectTreatment reads go through the vw_pb_ui_ScenarioTreatments view, which
            // (a) LEFT JOINs tbl_pb_Counties + tbl_pb_UserTreatments on every row just to
            // compute columns (County, etc.) this page never uses, and (b) filters to
            // IsSelected = 1 — treatments the optimizer picked in a completed run. Treatment
            // Shift settings configure the *next* run, so they must be visible against every
            // imported candidate treatment, not just ones a prior run already selected —
            // filtering on IsSelected left this tab empty for any scenario that hadn't been
            // run yet. Query the base table directly instead (only the UserTreatments join
            // the TreatmentType substitution needs, no IsSelected filter), matching what the
            // legacy app queried for this exact lookup.
            var rawTreatmentNames = await dbContext.Database
                .SqlQuery<string>($@"
                    SELECT DISTINCT
                        CASE WHEN t.IsUserTreatment = 1 THEN u.UserTreatmentName ELSE t.Treatment END AS Value
                    FROM tbl_pb_ExtendedImportedTreatments t
                    LEFT JOIN tbl_pb_UserTreatments u ON u.UserTreatmentTypeNo = t.UserTreatmentTypeNo
                    WHERE t.ScenId = {scenid}")
                .ToListAsync();

            var treatmentNames = rawTreatmentNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var saved = await dbContext.ScenarioTreatmentShifts
                .Where(s => s.ScenId == scenid)
                .ToListAsync();

            var savedLookup = saved
                .Where(s => !string.IsNullOrWhiteSpace(s.Treatment))
                .GroupBy(s => s.Treatment.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            return treatmentNames.Select(name =>
            {
                savedLookup.TryGetValue(name, out var s);
                return new TreatmentShiftRow
                {
                    Treatment = name,
                    MaxDelay = NormalizeShift(s?.MaxDelay ?? 0),
                    MaxAccelerate = s == null ? 5 : NormalizeShift(s.MaxAccelerate)
                };
            }).ToList();
        }

        public async Task SaveParametersPageAsync(int scenid, Dictionary<string, double?> parameterValues, List<TreatmentShiftRow> shifts, string notes)
        {
            var dbContext = await GetDbContextAsync();

            // 1) Parameter values (entities are tracked → mutate in place).
            if (parameterValues != null && parameterValues.Count > 0)
            {
                var ids = parameterValues.Keys.ToList();
                var targets = await dbContext.ScenarioParameters
                    .Where(p => p.ScenarioId == scenid && ids.Contains(p.Id))
                    .ToListAsync();
                foreach (var target in targets)
                {
                    if (parameterValues.TryGetValue(target.Id, out var value) && value.HasValue)
                    {
                        target.ParameterValue = value.Value;
                    }
                }
            }

            // 2) Treatment shifts (add/update posted, remove the rest).
            if (shifts != null && shifts.Count > 0)
            {
                var existing = await dbContext.ScenarioTreatmentShifts
                    .Where(s => s.ScenId == scenid)
                    .ToListAsync();

                var posted = new HashSet<string>(shifts.Select(r => r.Treatment?.Trim() ?? string.Empty), StringComparer.OrdinalIgnoreCase);

                foreach (var row in shifts)
                {
                    var name = row.Treatment?.Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var match = existing.FirstOrDefault(s => string.Equals(s.Treatment, name, StringComparison.OrdinalIgnoreCase));
                    if (match == null)
                    {
                        dbContext.ScenarioTreatmentShifts.Add(new ScenarioTreatmentShift
                        {
                            ScenId = scenid,
                            Treatment = name,
                            MaxDelay = NormalizeShift(row.MaxDelay),
                            MaxAccelerate = NormalizeShift(row.MaxAccelerate)
                        });
                    }
                    else
                    {
                        match.MaxDelay = NormalizeShift(row.MaxDelay);
                        match.MaxAccelerate = NormalizeShift(row.MaxAccelerate);
                    }
                }

                var toRemove = existing.Where(s => !posted.Contains(s.Treatment)).ToList();
                if (toRemove.Count > 0)
                {
                    dbContext.ScenarioTreatmentShifts.RemoveRange(toRemove);
                }
            }

            // 3) Mark the scenario stale.
            var scenario = await dbContext.Scenarios.FirstOrDefaultAsync(s => s.Id == scenid);
            if (scenario != null)
            {
                scenario.Stale = true;
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    scenario.Notes = notes;
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static int NormalizeShift(int value)
        {
            if (value <= 0) return 0;
            return value == 99 ? 0 : value;
        }

    }
}
