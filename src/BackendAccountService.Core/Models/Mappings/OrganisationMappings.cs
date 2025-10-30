using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Models.Mappings;

public static class OrganisationMappings
{
    public static int? GetProducerTypeId(ProducerType? producerType)
    {
        if (producerType == null)
        {
            return null;
        }

        return producerType switch
        {
            ProducerType.NotSet => Data.DbConstants.ProducerType.NotSet,
            ProducerType.Partnership => Data.DbConstants.ProducerType.Partnership,
            ProducerType.UnincorporatedBody => Data.DbConstants.ProducerType.UnincorporatedBody,
            ProducerType.NonUkOrganisation => Data.DbConstants.ProducerType.NonUkOrganisation,
            ProducerType.SoleTrader => Data.DbConstants.ProducerType.SoleTrader,
            ProducerType.Other => Data.DbConstants.ProducerType.Other,
            ProducerType.LimitedPartnership => Data.DbConstants.ProducerType.LimitedPartnership,
            ProducerType.LimitedLiabilityPartnership => Data.DbConstants.ProducerType.LimitedLiabilityPartnership,
            _ => throw new ArgumentException($"Unrecognised organisation type: '{producerType}'")
        };
    }
    
    public static ProducerType? GetProducerType(int? producerTypeId)
    {
        if (producerTypeId == null)
        {
            return null;
        }

        return producerTypeId switch
        {
            Data.DbConstants.ProducerType.NotSet => ProducerType.NotSet,
            Data.DbConstants.ProducerType.Partnership => ProducerType.Partnership,
            Data.DbConstants.ProducerType.UnincorporatedBody => ProducerType.UnincorporatedBody,
            Data.DbConstants.ProducerType.NonUkOrganisation => ProducerType.NonUkOrganisation,
            Data.DbConstants.ProducerType.SoleTrader => ProducerType.SoleTrader,
            Data.DbConstants.ProducerType.Other => ProducerType.Other,
            Data.DbConstants.ProducerType.LimitedPartnership => ProducerType.LimitedPartnership,
            Data.DbConstants.ProducerType.LimitedLiabilityPartnership => ProducerType.LimitedLiabilityPartnership,
            _ => throw new ArgumentException($"Unrecognised organisation type ID: '{producerTypeId}'")
        };
    }

    public static OrganisationResponseModel GetOrganisationModelFromOrganisation(Data.Entities.Organisation organisation)
    {
        var organisationModel = new OrganisationResponseModel();
        organisationModel.OrganisationType = OrganisationTypeMappings.GetOrganisationType(organisation.OrganisationTypeId);
        organisationModel.ProducerType = GetProducerType(organisation.ProducerTypeId);
        organisationModel.CompaniesHouseNumber = organisation.CompaniesHouseNumber;
        organisationModel.CreatedOn = organisation.CreatedOn;
        organisationModel.IsComplianceScheme = organisation.IsComplianceScheme;
        organisationModel.ValidatedWithCompaniesHouse = organisation.ValidatedWithCompaniesHouse;
        organisationModel.Name = organisation.Name;
        organisationModel.Address = new AddressModel
        {
            SubBuildingName = organisation.SubBuildingName,
            BuildingName = organisation.BuildingName,
            BuildingNumber = organisation.BuildingNumber,
            Street = organisation.Street,
            Locality = organisation.Locality,
            DependentLocality = organisation.DependentLocality,
            Town = organisation.Town,
            County = organisation.County,
            Postcode = organisation.Postcode,
            Country = organisation.Country
        };

        if (organisation.OrganisationRelationships != null)
        {
            var orgRelationship = organisation.OrganisationRelationships.FirstOrDefault(s => s.RelationToDate == null && s.SecondOrganisationId == organisation.Id);
            if (orgRelationship != null)
            {
                organisationModel.OrganisationRelationship = new OrganisationRelationshipModel
                {
                    JoinerDate = orgRelationship.JoinerDate,
                    LeaverCodeId = orgRelationship.LeaverCodeId,
                    LeaverDate = orgRelationship.LeaverDate,
                    OrganisationChangeReason = orgRelationship.OrganisationChangeReason,
                    SecondOrganisationId = orgRelationship.SecondOrganisationId,
                    FirstOrganisationId = orgRelationship.FirstOrganisationId,
                    LastUpdatedById = orgRelationship.LastUpdatedById,
                    LastUpdatedByOrganisationId = orgRelationship.LastUpdatedByOrganisationId,
                    OrganisationRegistrationTypeId = orgRelationship.OrganisationRegistrationTypeId,
                    OrganisationRelationshipTypeId = 1
                };
            }
        }

        organisationModel.ReferenceNumber = organisation.ReferenceNumber;
        organisationModel.ExternalId = organisation.ExternalId;

        if (organisation.NationId != null)
        {
            organisationModel.Nation = NationMappings.GetNation(organisation.NationId.Value);
        }

        organisationModel.Id = organisation.Id;

        return organisationModel;
    }


    public static OrganisationResponseModel GetOrganisationModelFromOrganisation(Organisation organisation, Organisation parentOrganisation)
    {
        if (organisation is null)
        {
            return null;
        }

        var organisationModel = new OrganisationResponseModel();
        organisationModel.OrganisationType = OrganisationTypeMappings.GetOrganisationType(organisation.OrganisationTypeId);
        organisationModel.ProducerType = GetProducerType(organisation.ProducerTypeId);
        organisationModel.CompaniesHouseNumber = organisation.CompaniesHouseNumber;
        organisationModel.CreatedOn = organisation.CreatedOn;
        organisationModel.IsComplianceScheme = organisation.IsComplianceScheme;
        organisationModel.ValidatedWithCompaniesHouse = organisation.ValidatedWithCompaniesHouse;
        organisationModel.Name = organisation.Name;
        organisationModel.ParentCompanyName = parentOrganisation?.Name;      
        organisationModel.ReferenceNumber = organisation.ReferenceNumber;
        organisationModel.ExternalId = organisation.ExternalId;

        if (organisation.NationId != null)
        {
            organisationModel.Nation = NationMappings.GetNation(organisation.NationId.Value);
        }

        organisationModel.Id = organisation.Id;

        return organisationModel;
    }

    public static OrganisationDetailModel GetOrganisationDetailModel(Organisation organisation)
    {
        return new OrganisationDetailModel
        {
            Name = organisation.Name,
            OrganisationNumber = organisation.ReferenceNumber,
        };
    }

    public static OrganisationResponseModel GetOrganisationResponseModel(Organisation organisation)
    {
        return new OrganisationResponseModel
        {
            Id = organisation.Id,
            Name = organisation.Name,
            CompaniesHouseNumber = organisation.CompaniesHouseNumber,
            ReferenceNumber = organisation.ReferenceNumber,
            ExternalId = organisation.ExternalId,
        };
    }

    public static Organisation GetOrganisationFromOrganisationModel(OrganisationModel organisation)
    {
        return new Organisation
        {
            OrganisationTypeId = OrganisationTypeMappings.GetOrganisationTypeId(organisation.OrganisationType),
            ProducerTypeId = GetProducerTypeId(organisation.ProducerType),
            CompaniesHouseNumber = organisation.CompaniesHouseNumber,
            Name = organisation.Name,
            SubBuildingName = organisation.Address.SubBuildingName,
            BuildingName = organisation.Address.BuildingName,
            BuildingNumber = organisation.Address.BuildingNumber,
            Street = organisation.Address.Street,
            Locality = organisation.Address.Locality,
            DependentLocality = organisation.Address.DependentLocality,
            Town = organisation.Address.Town,
            County = organisation.Address.County,
            Country = organisation.Address.Country,
            Postcode = organisation.Address.Postcode,
            ValidatedWithCompaniesHouse = organisation.ValidatedWithCompaniesHouse,
            IsComplianceScheme = organisation.IsComplianceScheme,
            NationId = NationMappings.GetNationId((Nation)organisation.Nation),
            OrganisationRelationships = new List<OrganisationRelationship>()
        };
    }

    public static OrganisationRelationship GetOrganisationRelationshipFromOrganisationRelationshipModel(OrganisationRelationshipModel organisationRelationship)
    {
        return new OrganisationRelationship
        {
            FirstOrganisationId = organisationRelationship.FirstOrganisationId,
            SecondOrganisationId = organisationRelationship.SecondOrganisationId,
            OrganisationRelationshipTypeId = organisationRelationship.OrganisationRelationshipTypeId,
            RelationFromDate = (DateTime)organisationRelationship.RelationFromDate,
            LastUpdatedById = organisationRelationship.LastUpdatedById,
            LastUpdatedByOrganisationId = organisationRelationship.LastUpdatedByOrganisationId,
            JoinerDate = organisationRelationship.JoinerDate,
            LeaverCodeId = organisationRelationship.LeaverCodeId,
            LeaverDate = organisationRelationship.LeaverDate,
            OrganisationChangeReason = organisationRelationship.OrganisationChangeReason,
        };
    }

    public static SubsidiaryOrganisation GetSubsidiaryOrganisationFromOrganisationModel(string subsidiaryId, Organisation organisation)
    {
        return new SubsidiaryOrganisation
        {
            OrganisationId = organisation.Id,
            SubsidiaryId = subsidiaryId,
            CreatedOn = DateTime.UtcNow,
            LastUpdatedOn = DateTime.UtcNow
        };
    }

    public static List<UpdatedProducersResponseModel> GetUpdatedProducers(List<Organisation> organisation)
    {
        return organisation.Select(org => new UpdatedProducersResponseModel
        {
            ProducerName = org.Name,
            IsDeleted = org.IsDeleted,
            ReferenceNumber = org.ReferenceNumber,
            BuildingName = org.BuildingName,
            BuildingNumber = org.BuildingNumber,
            CompaniesHouseNumber = org.CompaniesHouseNumber,
            Country = org.Country,
            County = org.County,
            DependentLocality = org.DependentLocality,
            IsComplianceScheme = org.IsComplianceScheme,
            Locality = org.Locality,
            OrganisationId = org.Id,
            Postcode = org.Postcode,
            ProducerTypeId = org.ProducerTypeId,
            Street = org.Street,
            SubBuildingName = org.SubBuildingName,
            Town = org.Town,
            TradingName = org.TradingName,
            ValidatedWithCompaniesHouse = org.ValidatedWithCompaniesHouse,
            ExternalId = org.ExternalId.ToString()
        }).ToList();
    }
}
