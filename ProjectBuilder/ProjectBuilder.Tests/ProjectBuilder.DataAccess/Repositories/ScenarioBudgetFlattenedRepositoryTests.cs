using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectBuilder.Tests
{
    [TestFixture]
    public class ScenarioBudgetFlattenedRepositoryTests
    {
        private ScenarioBudgetFlattenedRepository _scenarioBudgetFlattenedRepository;
        private Mock<ILogger<ProjectBuilderRepository<ScenarioBudget, ScenarioBudgetFlatModel, int>>> _logger;
        private IMapper _mapper;
        private ProjectBuilderDbContext _pbConxtext;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {

            var mappingConfiguration = new MapperConfiguration(m =>
            {
                m.AddProfile(new ScenarioBudgetFlattenedProfile());
            });
            _mapper = mappingConfiguration.CreateMapper();
        }
        [SetUp]
        public async Task Setup()
        {
            _logger = new();
            var options = new DbContextOptionsBuilder<ProjectBuilderDbContext>().UseInMemoryDatabase("PBDatabase").Options;
            _pbConxtext = new ProjectBuilderDbContext(options);
            var scenarioBudget = new List<ScenarioBudget>
            {
                new ScenarioBudget { AssetType = "B",Budget = 5000,District = 1,IsInterstate = true,ScenarioId = 9,EntityId = 2020},
                new ScenarioBudget { AssetType = "B",Budget = 6000,District = 1,IsInterstate = false,ScenarioId = 9,EntityId = 2020},
                new ScenarioBudget { AssetType = "P",Budget = 8000,District = 1,IsInterstate = true,ScenarioId = 9,EntityId = 2020},
                new ScenarioBudget { AssetType = "P",Budget = 4000,District = 1,IsInterstate = false,ScenarioId = 9,EntityId = 2020}
            };
            _pbConxtext.ScenariosBudgets.AddRange(scenarioBudget);
            await _pbConxtext.SaveChangesAsync();
            _scenarioBudgetFlattenedRepository = new(_pbConxtext,_mapper,_logger.Object);
        }
        [TearDown]
        public void TearDown()
        {
            _pbConxtext.ScenariosBudgets.RemoveRange(_pbConxtext.ScenariosBudgets);
            _pbConxtext.SaveChanges();
        }
        [Test]
        public async Task GetAllScenarioBudget_GetsAllScenarioBudgets()
        {
            var budgets = await _scenarioBudgetFlattenedRepository.GetAllAsync();
            Assert.AreEqual(1,budgets.Count);
            var budget = budgets.First();
            Assert.IsNotNull(budget.PavementInterstateBudget);
            Assert.IsNotNull(budget.PavementNonInterstateBudget);
            Assert.IsNotNull(budget.BridgeInterstateBudget);
            Assert.IsNotNull(budget.BridgeNonInterstateBudget);
        }
        [Test]
        public async Task UpdateBudget_UpdatesScenarioBudgetBasedOnGivenPropertyName()
        {
            var expectedValue = 55555;
            var budgets = await _scenarioBudgetFlattenedRepository.GetAllAsync();
            var budget = budgets.First();
            budget.BridgeInterstateBudget = expectedValue;
            await _scenarioBudgetFlattenedRepository.UpdateAsync(budget,nameof(budget.BridgeInterstateBudget));
            await _scenarioBudgetFlattenedRepository.SaveChangesAsync();
            var updatedBudgets = await _scenarioBudgetFlattenedRepository.GetAllAsync();
            var updatedBudget = updatedBudgets.First(); 
            Assert.AreEqual(expectedValue,updatedBudget.BridgeInterstateBudget);    
        }
    }
}
