using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Core.Services;
using BackendAccountService.Core.UnitTests.TestHelpers;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net;

namespace BackendAccountService.Core.UnitTests.Services
{
    [TestClass]
    public class RegulatorOrganisationServiceTests
    {
        private AccountsDbContext _accountContext = null!;        
        private RegulatorOrganisationService _regulatorOrganisationService = null!;
        private DbContextOptions<AccountsDbContext> _dbContextOptions = null!;

        [TestInitialize]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
                .UseInMemoryDatabase("AccountsDatabase")
                .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _accountContext = new AccountsDbContext(_dbContextOptions);
            
            RegulatorAccountsTestHelpers.SetupDatabaseForOrganisation(_accountContext);

            _regulatorOrganisationService = new RegulatorOrganisationService(_accountContext);
        }

        [TestMethod]        
        public async Task CreateNewRegulatorOrganisationAsync_WhenInvalidRequestSent_ReturnBadRequest()
        {
            // Arrange
            var request = new CreateRegulatorOrganisationRequest
            {
                Name = "Test Organisation",
                NationId = 1,
                ServiceId = "Regulator"
            };

            // Act
            var result = await _regulatorOrganisationService.CreateNewRegulatorOrganisationAsync(request);

            // Assert
            result.ErrorMessage.Should().NotBeNullOrEmpty();

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task CreateNewRegulatorOrganisationAsync_WhenValidRequestSent_ReturnOk()
        {
            // Arrange
            var request = new CreateRegulatorOrganisationRequest
            {
                Name = "Test New Organisation",
                NationId = 2,
                ServiceId = "Regulator"
            };

            // Act
            var result = await _regulatorOrganisationService.CreateNewRegulatorOrganisationAsync(request);

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            result.Should().BeOfType<Result<CreateRegulatorOrganisationResponse>>();
        }

        [TestMethod]
        public async Task GetRegulatorOrganisationByNationAsync_WhenOrganisationWithNationExist_ReturnOrganisation()
        {
            // Act
            var result = await _regulatorOrganisationService.GetRegulatorOrganisationByNationAsync("England");

            // Assert
            result.Should().BeOfType<CheckOrganisationExistResponseModel>();
        }

        [TestMethod]
        public async Task GetRegulatorOrganisationByNationAsync_WhenOrganisationWithNationNotExist_ReturnNull()
        {
            // Act
            var result = await _regulatorOrganisationService.GetRegulatorOrganisationByNationAsync("Scotland");

            // Assert
            result.Should().BeNull();
        }
    }
}
