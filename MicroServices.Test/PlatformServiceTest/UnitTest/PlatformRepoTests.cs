using AutoMapper;
using Moq;
using NUnit.Framework;
using PlatformService.Interfaces;
using PlatformService.Models.Domain;
using PlatformService.Repo;
using System.Data;
using MicroServices.DataAccess.Interfaces;
using PlatformService.Models.Entities;

namespace MicroServices.Test.PlatformServiceTest.UnitTest
{
    [TestFixture]
    class PlatformRepoTests
    {
        private Mock<IConnection> _connectionMock = null!;
        private Mock<IPlatformDataProcessor> _platformDataProcessorMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private PlatformRepo _platformrepo = null!;

        [SetUp]
        public void SetUp()
        {
            _connectionMock = new Mock<IConnection>();
            _platformDataProcessorMock = new Mock<IPlatformDataProcessor>();
            _mapperMock = new Mock<IMapper>();
            _platformrepo = new PlatformRepo(_connectionMock.Object, _platformDataProcessorMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task AddPlatformsAsync_ReturnsMappedDomainEntity_WhenValidInput()
        {
            // Arrange
            var domainInput = new PlatformAddDomainEntity
            {
                Name = "Test Platform",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            var platformEntity = new PlatformAddEntity
            {
                Name = "Test Platform",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            var returnedEntity = new PlatformEntity
            {
                Id = 1,
                Name = "Test Platform",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            var expectedDomain = new PlatformDomainEntity
            {
                Id = 1,
                Name = "Test Platform",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            // Setup mocks
            _mapperMock.Setup(m => m.Map<PlatformAddEntity>(domainInput)).Returns(platformEntity);
            _platformDataProcessorMock.Setup(p => p.AddAsync(platformEntity)).ReturnsAsync(returnedEntity);
            _mapperMock.Setup(m => m.Map<PlatformDomainEntity>(returnedEntity)).Returns(expectedDomain);

            _connectionMock
              .Setup(c => c.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), IsolationLevel.Serializable, 6, 0))
              .Returns((Func<Task>func, IsolationLevel _, int __, int ___) => func());

            // Act
            var result = await _platformrepo.AddPlatformAsync(domainInput);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Test Platform"));
        }

        [Test]
        public async Task UpdatePlatformAsync_ReturnsMappedDomainEntity_WhenValidInput()
        {
            // Arrange
            var domainInput = new PlatformUpdateDomainEntity
            {
                Id = 1,
                Name = "Test Platform Updated",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            var platformUpdateEntity = new PlatformUpdateEntity
            {
                Id = 1,
                Name = "Test Platform",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            var returnedEntity = new PlatformEntity
            {
                Id = 1,
                Name = "Test Platform Updated",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            var expectedDomain = new PlatformDomainEntity
            {
                Id = 1,
                Name = "Test Platform Updated",
                Description = "Test Description",
                Price = 10,
                Owner = "Test Owner",
                IsDeleted = false
            };

            // Setup mocks
            _mapperMock.Setup(m => m.Map<PlatformUpdateEntity>(domainInput)).Returns(platformUpdateEntity);
            _platformDataProcessorMock.Setup(p => p.UpdateAsync(platformUpdateEntity));
            _platformDataProcessorMock.Setup(p => p.GetAsync(It.IsAny<int>())).ReturnsAsync(returnedEntity);

            _mapperMock.Setup(m => m.Map<PlatformDomainEntity>(returnedEntity)).Returns(expectedDomain);

            _connectionMock
              .Setup(c => c.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), IsolationLevel.Serializable, 6, 0))
              .Returns((Func<Task> func, IsolationLevel _, int __, int ___) => func());

            // Act
            var result = await _platformrepo.UpdatePlatformAsync(domainInput);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Test Platform Updated"));
        }

        [Test]
        public async Task GetAllPlatformsAsync_ReturnsMappedDomainEntity_WhenValidInput()
        {
            // Arrange
            var returnedEntity = new List<PlatformEntity> {
                new PlatformEntity{
                    Id = 1,
                    Name = "Test Platform",
                    Description = "Test Description",
                    Price = 10,
                    Owner = "Test Owner",
                    IsDeleted = false
                }
            };

            IEnumerable<PlatformDomainEntity> expectedDomain = new List<PlatformDomainEntity>();

            // Setup mocks
            _platformDataProcessorMock.Setup(p => p.GetAllAsync()).ReturnsAsync(returnedEntity);

            // Mock the List mapping
            _mapperMock.Setup(m => m.Map<IEnumerable<PlatformDomainEntity>>(returnedEntity)).Returns(expectedDomain); 

            _connectionMock
              .Setup(c => c.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), IsolationLevel.Serializable, 6, 0))
              .Returns((Func<Task> func, IsolationLevel _, int __, int ___) => func());

            // Act
            var result = await _platformrepo.GetAllPlatformsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdsAsync_ReturnsMappedDomainEntities_WhenValidIds()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };
            var platformEntities = new List<PlatformEntity>
            {
                new PlatformEntity { Id = 1, Name = "Platform 1", Description = "Desc", Price = 10, Owner = "Owner", IsDeleted = false },
                new PlatformEntity { Id = 2, Name = "Platform 2", Description = "Desc", Price = 20, Owner = "Owner", IsDeleted = false },
                new PlatformEntity { Id = 3, Name = "Deleted Platform", Description = "Desc", Price = 30, Owner = "Owner", IsDeleted = true }
            };

            _platformDataProcessorMock.Setup(p => p.GetByIdsAsync(ids)).ReturnsAsync(platformEntities);
            _mapperMock.Setup(m => m.Map<PlatformDomainEntity>(platformEntities[0])).Returns(new PlatformDomainEntity { Id = 1, Name = "Platform 1", Description = "Desc", Price = 10, Owner = "Owner", IsDeleted = false });
            _mapperMock.Setup(m => m.Map<PlatformDomainEntity>(platformEntities[1])).Returns(new PlatformDomainEntity { Id = 2, Name = "Platform 2", Description = "Desc", Price = 20, Owner = "Owner", IsDeleted = false });

            _connectionMock
                .Setup(c => c.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), IsolationLevel.Serializable, 6, 0))
                .Returns((Func<Task> func, IsolationLevel _, int __, int ___) => func());

            // Act
            var result = await _platformrepo.GetByIdsAsync(ids);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(p => p.Id == 3), Is.False);
        }

        [Test]
        public async Task GetPlatformByIdAsync_ReturnsMappedDomainEntity_WhenPlatformExists()
        {
            // Arrange
            var id = 1;
            var platformEntity = new PlatformEntity
            {
                Id = id,
                Name = "Platform 1",
                Description = "Desc",
                Price = 10,
                Owner = "Owner",
                IsDeleted = false
            };

            var expectedDomain = new PlatformDomainEntity
            {
                Id = id,
                Name = "Platform 1",
                Description = "Desc",
                Price = 10,
                Owner = "Owner",
                IsDeleted = false
            };

            _platformDataProcessorMock.Setup(p => p.GetAsync(id)).ReturnsAsync(platformEntity);
            _mapperMock.Setup(m => m.Map<PlatformDomainEntity>(platformEntity)).Returns(expectedDomain);

            _connectionMock
                .Setup(c => c.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), IsolationLevel.Serializable, 6, 0))
                .Returns((Func<Task> func, IsolationLevel _, int __, int ___) => func());

            // Act
            var result = await _platformrepo.GetPlatformByIdAsync(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(id));
        }

        [Test]
        public async Task GetPlatformByIdAsync_ReturnsNull_WhenPlatformIsDeleted()
        {
            // Arrange
            var id = 1;
            var platformEntity = new PlatformEntity { Id = id, IsDeleted = true };

            _platformDataProcessorMock.Setup(p => p.GetAsync(id)).ReturnsAsync(platformEntity);

            _connectionMock
                .Setup(c => c.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), IsolationLevel.Serializable, 6, 0))
                .Returns((Func<Task> func, IsolationLevel _, int __, int ___) => func());

            // Act
            var result = await _platformrepo.GetPlatformByIdAsync(id);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeletePlatformAsync_ExecutesDelete()
        {
            // Arrange

            var deletedEntity = new PlatformEntity
            {
                Id = 1,
                Name = "Deleted Platform",
                Description = "Deleted Description",
                Price = 0,
                Owner = "Deleted Owner",
                IsDeleted = true
            };

            _platformDataProcessorMock.Setup(p => p.DeletePlatformAsync(deletedEntity.Id)).ReturnsAsync(deletedEntity); 

            _connectionMock
                .Setup(c => c.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), IsolationLevel.Serializable, 6, 0))
                .Returns((Func<Task> func, IsolationLevel _, int __, int ___) => func());

            // Act
            await _platformrepo.DeletePlatformAsync(deletedEntity.Id);

            // Assert
            _platformDataProcessorMock.Verify(p => p.DeletePlatformAsync(deletedEntity.Id), Times.Once);
        }
    }
}
