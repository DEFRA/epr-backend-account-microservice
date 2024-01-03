using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BackendAccountService.Core.Services
{
    public class RegulatorOrganisationService : IRegulatorOrganisationService
    {
        private readonly AccountsDbContext _accountsDbContext;

        public RegulatorOrganisationService(AccountsDbContext accountsDbContext)
        {
            _accountsDbContext = accountsDbContext;
        }

        public async Task<CheckOrganisationExistResponseModel?> GetRegulatorOrganisationByNationAsync(string nation)
        {
            return await GetRegulatorOrganisationByNationInternalAsync(nation);
        }

        public Task<Result<CreateRegulatorOrganisationResponse>> CreateNewRegulatorOrganisationAsync(CreateRegulatorOrganisationRequest request)
        {
            return CreateNewRegulatorOrganisationInternalAsync(request);
        }

        private async Task<Result<CreateRegulatorOrganisationResponse>> CreateNewRegulatorOrganisationInternalAsync(CreateRegulatorOrganisationRequest request)
        {
            if (await _accountsDbContext.Organisations.AnyAsync(x => 
                x.OrganisationTypeId == Data.DbConstants.OrganisationType.Regulators &&
                x.NationId == request.NationId))
            {
                return Result<CreateRegulatorOrganisationResponse>.FailedResult(
                $"Error: regulator organisation with name {request.Name} already exists",
                            HttpStatusCode.BadRequest);
            }

            var result = await _accountsDbContext.Organisations.AddAsync(new Organisation
            {
                Name = request.Name,
                OrganisationTypeId = Data.DbConstants.OrganisationType.Regulators,
                NationId = request.NationId
            });

            await _accountsDbContext.SaveChangesAsync(request.ServiceId);

            string nation = (await _accountsDbContext.Nations.SingleAsync(x => x.Id == result.Entity.NationId)).Name;

            return Result<CreateRegulatorOrganisationResponse>.SuccessResult(new CreateRegulatorOrganisationResponse
            {
                ExternalId = result.Entity.ExternalId,
                Nation = nation,
            });
        }

        private async Task<CheckOrganisationExistResponseModel> GetRegulatorOrganisationByNationInternalAsync(string nation)
        {
            var result = await _accountsDbContext.Organisations
                .Include(org => org.Nation)
                .Where(o => o.Nation.Name == nation && o.OrganisationTypeId == Data.DbConstants.OrganisationType.Regulators)
                .Select(org => new CheckOrganisationExistResponseModel 
                { 
                    CreatedOn = org.CreatedOn,
                    ExternalId = org.ExternalId,
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
