using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Tests
{
    [TestFixture]
    public class ProjectBuilderRepositoryTests
    {
        ProjectBuilderRepository<Scenario,ScenarioModel,int> _projectBuilderRepository;
        private IMapper _mapper;
        private Mock<ILogger<ProjectBuilderRepository<Scenario, ScenarioModel, int>>> _logger;
        private ScenarioModel _scenarioEntity;
        private ProjectBuilderDbContext _pbConxtext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var mappingConfiguration = new MapperConfiguration(m =>
            {
                m.AddProfile(new ScenarioProfile());
            });
            _mapper = mappingConfiguration.CreateMapper(); 
        }
        [SetUp]
        public void SetUp()
        {
            _scenarioEntity = new ScenarioModel() { ScenarioId = 20, ScenarioName = "Scenario1",CreatedBy = "" };
            _logger = new();
            var options = new DbContextOptionsBuilder<ProjectBuilderDbContext>().UseInMemoryDatabase("PBDatabase").Options;                             
             _pbConxtext = new ProjectBuilderDbContext(options);
            _projectBuilderRepository = new(_pbConxtext, _mapper, _logger.Object);
        }
        [TearDown]
        public void TearDown()
        {
            _projectBuilderRepository.ClearPendingChanges();
            _pbConxtext.Scenarios.RemoveRange(_pbConxtext.Scenarios);
            _pbConxtext.SaveChanges();
        }
        [Test]
        public async Task Insert_AddsEntityToDataBase_InsertOneRecord()
        {
           // _projectBuilderRepository.Refresh();
            _projectBuilderRepository.Insert(_scenarioEntity);
            await _projectBuilderRepository.SaveChangesAsync();
            var count = await _projectBuilderRepository.GetCountAsync();
            Assert.AreEqual(1, count);
        }
        [Test]
        public async Task Insert_AddsEntityToDataBase_Fails()
        {
            // _projectBuilderRepository.Refresh();
            _projectBuilderRepository.Insert(_scenarioEntity);
            await _projectBuilderRepository.SaveChangesAsync();
            var count = await _projectBuilderRepository.GetCountAsync();
            Assert.AreEqual(1, count);
        }
        [Test]
        public async Task FindAsync_FindsAnEntityBasedOnPrimaryKey_ReturnsFoundsEntity()
        {
            _projectBuilderRepository.Insert(_scenarioEntity);
            await _projectBuilderRepository.SaveChangesAsync();
            var foundedEntity = await _projectBuilderRepository.FindAsync(_scenarioEntity.ScenarioId);
            Assert.IsNotNull(foundedEntity);
        }
        [Test]
        public async Task FindAsync_FindsAnEntityBasedOnPrimaryKey_ReturnsNullIfEntityWasNotFound()
        {
            var foundedEntity = await _projectBuilderRepository.FindAsync(10);
            Assert.IsNull(foundedEntity);
        }
        [Test]
        public async Task GetCountAsync_ReturnsItemsCountFromCurrentQuery()
        {
            _projectBuilderRepository.Insert(_scenarioEntity);
            await _projectBuilderRepository.SaveChangesAsync();
            var actual = await _projectBuilderRepository.GetCountAsync();
            Assert.AreEqual(1, actual);
        }
        [Test]
        public async Task GetAllAsync_ReturnsAllItemsFoundUsingCurrentQuery()
        {
            await InsertData(2);
            var actual = await _projectBuilderRepository.GetAllAsync();
            Assert.AreEqual(2,actual.Count);
        }
        [Test]
        public async Task GetRangeAsync_RetunrsRangeBasedOnGivenStartCountParameters()
        {
            await InsertData(10);
            var actual = await _projectBuilderRepository.GetRangeAsync(2, 2);
            Assert.AreEqual(actual.Count, 2);
        }
        [Test]
        public async Task DeleteEntity_DeletesEntityFromDatabase()
        {
            _projectBuilderRepository.Insert(_scenarioEntity);
           await _projectBuilderRepository.SaveChangesAsync();
            _projectBuilderRepository.DeleteAsync(_scenarioEntity.ScenarioId);
            await _projectBuilderRepository.SaveChangesAsync();
            var actual = await _projectBuilderRepository.GetCountAsync();
            Assert.AreEqual(0, actual);
        }
        [Test]
        public async Task UpdateEntity_UpdatesOldEntityWithNewEntity()
        {
            _projectBuilderRepository.Insert(_scenarioEntity);
            await _projectBuilderRepository.SaveChangesAsync();
            var valuePairs = new Dictionary<string,object> { { "ScenarioName" , "NewEntity" }};
            await _projectBuilderRepository.UpdateAsync(_scenarioEntity.ScenarioId,valuePairs);
            await _projectBuilderRepository.SaveChangesAsync();
            var updatedValue = await _projectBuilderRepository.FindAsync(20);
            Assert.AreEqual(_scenarioEntity.ScenarioName, updatedValue.ScenarioName);
        }
        [Test]
        public async Task IsPending_TrueIfThereAreAnyPendingChangesOtherwiseFalse()
        {
            _projectBuilderRepository.Insert(_scenarioEntity);
            Assert.IsTrue(_projectBuilderRepository.IsPending);
            await _projectBuilderRepository.SaveChangesAsync();
            Assert.IsFalse(_projectBuilderRepository.IsPending);
        }
        [Test]
        public void ClearPendingChanges_ClearsAnyPendingOperations()
        {
            _projectBuilderRepository.Insert(_scenarioEntity);
            _projectBuilderRepository.ClearPendingChanges();
            Assert.IsFalse(_projectBuilderRepository.IsPending);
        }
        [Test]
        public async Task ApplyFilter_FiltersDataBasedOnGivenEntity()
        {
            _projectBuilderRepository.Insert(new ScenarioModel { ScenarioId = 1, ScenarioName = $"Scenario{1}", CreatedBy = "" });
            _projectBuilderRepository.Insert(new ScenarioModel { ScenarioId = 2, ScenarioName = $"Scenario2", CreatedBy = "" });
            await _projectBuilderRepository.SaveChangesAsync();
            _projectBuilderRepository.ApplyFilter(new ScenarioModel { ScenarioId = 2 });
            var result = await _projectBuilderRepository.GetAllAsync();
            Assert.AreEqual(1, result.Count);
            var actual = result.FirstOrDefault(s => s.ScenarioId == 2);
            Assert.IsNotNull(actual);
        }
        [Test]
        public async Task ApplyFilter_FiltersDataBasedOnGivenPropertyValuePairs()
        {
            var expectedValue = $"Scenario{2}";
            await InsertData(10);
            _projectBuilderRepository.ApplyFilter(new Dictionary<string, object> { { "ScenarioName" , expectedValue } });
            var result = await _projectBuilderRepository.GetAllAsync();
            Assert.AreEqual(1, result.Count);
            var actual = result.FirstOrDefault(s => s.ScenarioName == expectedValue);
            Assert.IsNotNull(actual);
        }
        //[Test]
        //public void SaveChangesAsync_SavesDataToDataBase_AddsDataIfItNotExistWhenUpdating()
        //{
        //    var scenario = new ScenarioModel { ScenarioId = 1, CreatedBy = "",ScenarioName = "Test" };
        //    var scenarioEntity = _mapper.Map<Scenario>(scenario);
        //    _pbConxtext.Scenarios.Add(scenarioEntity);
        //    _projectBuilderRepository.UpdateAs(scenario,new ScenarioModel { ScenarioName = "Updated"});
        //    var task = _projectBuilderRepository.SaveChangesAsync();
        //    Assert.ThrowsAsync(typeof(DbUpdateConcurrencyException),new AsyncTestDelegate(() => task));
        //}
        private async Task InsertData(int count)
        {
            for (int i = 1; i < count +1; i++)
            {
                _projectBuilderRepository.Insert(new ScenarioModel { ScenarioId = i,ScenarioName = $"Scenario{i}",CreatedBy = "" });
            }
            await _projectBuilderRepository.SaveChangesAsync();
        }
    }
}
 