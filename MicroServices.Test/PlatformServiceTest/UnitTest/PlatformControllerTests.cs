using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using PlatformService.Controllers;
using PlatformService.Interfaces;
using PlatformService.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MicroServices.API.Common;

namespace MicroServices.Test.PlatformServiceTest.UnitTest
{
    [TestFixture]
    class PlatformControllerTests
    {
        private Mock<IPlatformDomain> _platformRepoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private PlatformController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _platformRepoMock = new Mock<IPlatformDomain>();
            _mapperMock = new Mock<IMapper>();
            _controller = new PlatformController(_platformRepoMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetAllPlatforms_ReturnsOk_WhenPlatformsExist()
        {
            // Arrange
            var mockPlatforms = new List<PlatformDomainEntity> { new PlatformDomainEntity { Id = 1, Name = "name_test", Description = "description_test", Price = 0, Owner= "owner_test", IsDeleted= false } };
            _platformRepoMock.Setup(r => r.GetAllPlatformsAsync()).ReturnsAsync(mockPlatforms);

            // Act
            var result = await _controller.GetAllPlatforms();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task GetAllPlatforms_ReturnsNotFound_WhenNoPlatformExist()
        {
            // Arrange: return an empty list
            var emptyPlatforms = new List<PlatformDomainEntity>();
            _platformRepoMock.Setup(r => r.GetAllPlatformsAsync()).ReturnsAsync(emptyPlatforms);

            // Act
            var result = await _controller.GetAllPlatforms();

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task GetPlatformById_ReturnsFound_WhenPlatformExist()
        {
            // Arrange         
            var mockPlatform = new PlatformDomainEntity { Id = 1, Name = "name_test", Description = "description_test", Price = 0, Owner = "owner_test", IsDeleted = false };
            _platformRepoMock.Setup(r => r.GetPlatformByIdAsync(It.IsAny<int>())).ReturnsAsync(mockPlatform);
            // Act
            var result = await _controller.GetPlatformById(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));

            //// Verify controller wrapped it properly
            var apiResult = okResult.Value as ApiResult<PlatformDomainEntity>;
            Assert.That(apiResult?.Payload, Is.EqualTo(mockPlatform));
        }

        [Test]
        public async Task GetPlatformById_ReturnsNotFound_WhenPlatformDoesNotExist()
        {
            // Arrange
            _platformRepoMock.Setup(r => r.GetPlatformByIdAsync(It.IsAny<int>())).ReturnsAsync(default(PlatformDomainEntity)!);

            // Act
            var result = await _controller.GetPlatformById(999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task AddPlatform_ReturnsCreated_WhenValid()
        {
            // Arrange
            var input = new PlatformAddDomainEntity { Name = "name_test", Description = "description_test", Price = 0, Owner = "owner_test", IsDeleted = false };
            var added = new PlatformDomainEntity { Id = 1, Name = "name_test", Description = "description_test", Price = 0, Owner = "owner_test", IsDeleted = false };

            _platformRepoMock.Setup(r => r.AddPlatformAsync(input)).ReturnsAsync(added);

            // Act
            var result = await _controller.AddPlatform(input);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.StatusCode, Is.EqualTo(201));


            var apiResult = createdResult.Value as ApiResult<PlatformDomainEntity>;
            Assert.That(apiResult, Is.Not.Null);
            Assert.That(apiResult!.IsSuccess, Is.True);
            Assert.That(apiResult.Message, Is.EqualTo("Platform added successfully"));
            Assert.That(apiResult.StatusCode, Is.EqualTo(201));
            Assert.That(apiResult.Payload, Is.EqualTo(added));
        }

        [Test]
        public async Task DeletePlatform_ReturnsNotFound_WhenNotExist()
        {
            // Arrange
            _platformRepoMock.Setup(r => r.GetPlatformByIdAsync(It.IsAny<int>())).ReturnsAsync(default(PlatformDomainEntity)!);

            // Act
            var result = await _controller.DeletePlatform(100);

            // Assert
            var notFound = result as NotFoundObjectResult;
            Assert.That(notFound, Is.Not.Null);
            Assert.That(notFound!.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeletePlatform_ReturnsOk_WhenPlatformDeleted()
        {
            // Arrange
            var platform = new PlatformDomainEntity { Id = 1, Name = "name_test", Description = "description_test", Price = 0, Owner = "owner_test", IsDeleted = false };
            _platformRepoMock.Setup(r => r.GetPlatformByIdAsync(It.IsAny<int>())).ReturnsAsync(platform);

            // Act
            var result = await _controller.DeletePlatform(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));

            var apiResult = okResult.Value as ApiResult<string>;
            Assert.That(apiResult, Is.Not.Null);
            Assert.That(apiResult!.IsSuccess, Is.True);
            Assert.That(apiResult.Message, Is.EqualTo("Platform deleted successfully"));
            Assert.That(apiResult.StatusCode, Is.EqualTo(204)); 
            Assert.That(apiResult.Payload, Is.Null);
        }
    }
}



