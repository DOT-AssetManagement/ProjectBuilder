using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AutoMapper.Internal;
using LinqKit;
using System.Data.Entity.Core.Objects;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;
using System.Security.Policy;
using System.Runtime.CompilerServices;

namespace ProjectBuilder.DataAccess
{
    public class ProjectBuilderRepository<Entity,Model,Id> : IRepository<Model> where Entity : class,IEntity<Id> where Model : class,new()
    {
        private ProjectBuilderDbContext _projectBuilderDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectBuilderRepository<Entity, Model,Id>> _logger;
        private bool disposedValue;

        public event Action<ErrorEventArgs> ErrorOccured;
        public ProjectBuilderRepository(ProjectBuilderDbContext projectBuilderDbContext, IMapper mapper,ILogger<ProjectBuilderRepository<Entity, Model,Id>> logger)
        {
            _projectBuilderDbContext = projectBuilderDbContext;
            _logger = logger;
            _mapper = mapper;
            CurrentQuery = InitializeCurrrentQuery();
        }

        protected IQueryable<Entity> CurrentQuery { get; set; }
        protected ProjectBuilderDbContext ProjectBuilderDbContext { get { return _projectBuilderDbContext; } }
        protected IMapper Mapper { get { return _mapper; } }
        protected ILogger<ProjectBuilderRepository<Entity, Model, Id>> Logger { get { return _logger; } }

        protected virtual IQueryable<Entity> InitializeCurrrentQuery(Expression<Func<Entity,bool>> filter = null)
        {
            if (filter is null)
                return ProjectBuilderDbContext.Set<Entity>().OrderBy(e => e.EntityId);
            return ProjectBuilderDbContext.Set<Entity>().Where(filter).OrderBy(e => e.EntityId);
        }
        public bool IsPending
        { 
            get  
            { 
                return ProjectBuilderDbContext.ChangeTracker.HasChanges(); 
            }  
        }
        public virtual async Task<long> GetCountAsync(CancellationToken token = default)
        {
            try
            {
                return await CurrentQuery.LongCountAsync(token);
            }
            catch (TaskCanceledException taskCancelled)
            {
                Logger.Log(LogLevel.Information, taskCancelled, taskCancelled.Message);
                RaiseOnErrorOccured("the current operation has been canceled",LogLevel.Information);
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, ex.Message);
                RaiseOnErrorOccured("Could not get the items count", LogLevel.Error);
                #if DEBUG
                 throw;
                #endif
                return 0;
            }
        } 
        public virtual async Task<Model> FindAsync(params object[] parameters)
        {
            try
            {
                Entity? result = await ProjectBuilderDbContext.Set<Entity>().FindAsync(parameters);
                return Mapper.Map<Model>(result);
            }
            catch (TaskCanceledException taskCancelled)
            {
                Logger.Log(LogLevel.Information, taskCancelled, taskCancelled.Message);
                RaiseOnErrorOccured("the current operation has been canceled", LogLevel.Information);
                return default;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, ex.Message);
                RaiseOnErrorOccured("Could not find the requested item", LogLevel.Error);
                #if DEBUG
                throw;
                #endif
                return default;
            }
        }
        public async Task<Model> FindAsync(CancellationToken token, params object[] parameters)
        {
            try
            {
               Entity? result = await ProjectBuilderDbContext.Set<Entity>().FindAsync(parameters, token);
               return Mapper.Map<Model>(result);
            }
            catch (TaskCanceledException taskCancelled)
            {
                Logger.Log(LogLevel.Information, taskCancelled, taskCancelled.Message);
                RaiseOnErrorOccured("the current operation has been canceled", LogLevel.Information);
                return default;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, ex.Message);
                RaiseOnErrorOccured("Couldnt find the requested item", LogLevel.Error);
                #if DEBUG
                throw;
                #endif
                return default;
            }
        }

        public virtual async Task<List<Model>> GetAllAsync(CancellationToken token = default)
        {
            try
            {
                var result = await CurrentQuery.AsNoTracking()
                                         .AsExpandable()
                                         .ProjectTo<Model>(Mapper.ConfigurationProvider)
                                         .ToListAsync(token);
                return result;                                
            }
            catch (TaskCanceledException taskCancelled)
            {
                Logger.Log(LogLevel.Information, taskCancelled, taskCancelled.Message);
                RaiseOnErrorOccured("the current operation has been canceled", LogLevel.Information);
                return Enumerable.Empty<Model>().ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error,ex,ex.Message);
                RaiseOnErrorOccured("Could not get the requested items.", LogLevel.Error);
                #if DEBUG
                throw;
                #endif
                return Enumerable.Empty<Model>().ToList();
            }
        }
        public virtual async Task<List<Model>> GetRangeAsync(int startIndex, int count, CancellationToken token = default)
        {
            try
            {
                return await CurrentQuery.Skip(startIndex).Take(count)
                                         .AsNoTracking()
                                         .AsExpandable()
                                         .ProjectTo<Model>(Mapper.ConfigurationProvider)
                                         .ToListAsync(token);
                                                        
            }
            catch (TaskCanceledException taskCancelled)
            {
                Logger.Log(LogLevel.Information, taskCancelled, taskCancelled.Message);
                RaiseOnErrorOccured("the current operation has been canceled", LogLevel.Information);
                return Enumerable.Empty<Model>().ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, ex.Message);
                RaiseOnErrorOccured("Could not get the requested items.", LogLevel.Error);
                 #if DEBUG
                   throw;
                 #endif
                return Enumerable.Empty<Model>().ToList();
            }
        }

        public void Insert(Model model)
        {
            var entity = Mapper.Map<Entity>(model);
            ProjectBuilderDbContext.Set<Entity>().Add(entity);
        }
        public virtual async Task<Model> InsertAndSave(Model model) 
        {
            var entity = Mapper.Map<Entity>(model);
            ProjectBuilderDbContext.Set<Entity>().Add(entity);
            await SaveChangesAsync();
            return Mapper.Map<Model>(entity);
        }
        public void ClearPendingChanges()
        {
            ProjectBuilderDbContext.ChangeTracker.Clear();
        }
        public Task Refresh()
        {
           return Task.CompletedTask;
        }
        public async Task UpdateAsync<TId>(TId modelId, Dictionary<string, object> propertiesValuesPairs)
        {
            var targetEntity = await ProjectBuilderDbContext.Set<Entity>().FindAsync(modelId);
            if (targetEntity is null)
                return;
            var entityType = typeof(Entity);
                foreach (var property in propertiesValuesPairs.Keys)
                {
                    var currentProperty = entityType.GetProperty(property);
                    if (currentProperty is not null)
                    {
                       currentProperty.SetValue(targetEntity, propertiesValuesPairs[property]);
                    }
                }     
        }
        public async Task UpdateAsync(Dictionary<string, object> propertiesValuesPairs, params object[] parameters)
        {
            var targetEntity = await ProjectBuilderDbContext.Set<Entity>().FindAsync(parameters);
            if (targetEntity is null)
                return;
            var entityType = typeof(Entity);
            foreach (var property in propertiesValuesPairs.Keys)
            {
                var currentProperty = entityType.GetProperty(property);
                if (currentProperty is not null)
                {
                    currentProperty.SetValue(targetEntity, propertiesValuesPairs[property]);
                }
            }
        }
        public virtual Task UpdateAsync(Model newValue, params string[] properties)
        {
            return Task.CompletedTask;    
        }
        public async Task<bool> DeleteAsync<TId>(TId modelId, CancellationToken token = default)
        {
            var target = await ProjectBuilderDbContext.Set<Entity>().FindAsync(modelId);
            if (target is null)
                return false;
            ProjectBuilderDbContext.Set<Entity>().Remove(target);
            return true;
        }
        public async Task<bool> DeleteAsync(params object[] parameters)
        {
            var target = await ProjectBuilderDbContext.Set<Entity>().FindAsync(parameters);
            if (target is null)
                return false;
            ProjectBuilderDbContext.Set<Entity>().Remove(target);
            return true;
        }
        #region Filter Methods
        public virtual void ApplyFilter(Dictionary<string, object> propertyValuePairs)
        {
            CurrentQuery = InitializeCurrrentQuery();
            var expression = BuildExpression(propertyValuePairs);
            CurrentQuery = InitializeCurrrentQuery(expression);
        }
        protected Expression<Func<Entity, bool>> BuildExpression(Dictionary<string, object> propertyValuePairs)
        {
            var expression = PredicateBuilder.New<Entity>(true);
            if (propertyValuePairs is null || propertyValuePairs.Count < 1)
                return expression;
            var entityType = typeof(Entity);
            var parameter = Expression.Parameter(typeof(Entity), "e");
            foreach (var property in propertyValuePairs.Keys)
            {
                var value = propertyValuePairs[property];
                var propertyExpression = Expression.PropertyOrField(parameter, property);
                var pType = entityType.GetProperty(property).PropertyType;
                var predicate = Expression.Lambda<Func<Entity, bool>>(Expression.Equal(propertyExpression, Expression.Convert(Expression.Constant(value), pType)), parameter);
                expression = PredicateBuilder.And(expression, predicate);
            }
            return expression;
        }
        public void ApplyFilter(Model filter)
        {
            var propertiesValues = new Dictionary<string, object>();
            var entity = Mapper.Map<Entity>(filter);
            var entityType = typeof(Entity);
            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType.IsNullableType() && property.GetValue(entity) is not null)
                     propertiesValues.Add(property.Name, property.GetValue(entity));
                else if (property.PropertyType.IsNumericType())
                {
                    try
                    {
                        if(Convert.ToDouble(property.GetValue(entity)) > 0)
                            propertiesValues.Add(property.Name, property.GetValue(entity));
                    }
                    catch
                    {
                        continue;
                    }
                }
                else if (property.PropertyType.IsBoolean())
                    propertiesValues.Add(property.Name, property.GetValue(entity));
                else if(property.PropertyType.IsString() && !string.IsNullOrEmpty(property.GetValue(entity)?.ToString()))
                    propertiesValues.Add(property.Name, property.GetValue(entity));
            }
            ApplyFilter(propertiesValues);
        }
        #endregion
        public async Task SaveChangesAsync(CancellationToken token = default)
        { 
            try
            {
                await ProjectBuilderDbContext.SaveChangesAsync(token);
                   ProjectBuilderDbContext.ChangeTracker.Clear();
            }
            catch (TaskCanceledException taskCancelled)
            {
                Logger.Log(LogLevel.Information, taskCancelled, taskCancelled.Message);
                RaiseOnErrorOccured("the current operation has been canceled", LogLevel.Information);
            }
            catch (DbUpdateConcurrencyException updateException)
            {
                if (await HandleUpdatingConflicts(updateException.Entries,token))
                    return;
                Logger.Log(LogLevel.Error,updateException, updateException.Message);
                RaiseOnErrorOccured("Could not update the target value because it was not found in the database.", LogLevel.Information);
                #if DEBUG
                throw;
                #endif
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, ex.Message);
                RaiseOnErrorOccured("Could not save the data", LogLevel.Error);
                #if DEBUG
                 throw;
                #endif
            }
        }

        protected virtual Task<bool> HandleUpdatingConflicts(IReadOnlyList<EntityEntry> entries,CancellationToken token= default)
        {
            return Task.FromResult(false);
        }
        public void Dispose()
        {
            ProjectBuilderDbContext.Dispose();
        }
        protected void RaiseOnErrorOccured(string message,LogLevel level)
        {
            ErrorOccured?.Invoke(new ErrorEventArgs(message, level));
        }

        public virtual Task<int> DeleteAsync(Model model)
        {
            return Task.FromResult(0);
        }
    }
}

