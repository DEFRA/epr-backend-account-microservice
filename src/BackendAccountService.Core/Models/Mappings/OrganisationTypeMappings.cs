namespace BackendAccountService.Core.Models.Mappings
{
    public static class OrganisationTypeMappings
    {
        public static int GetOrganisationTypeId(OrganisationType organisationType)
        {
            return organisationType switch
            {
                OrganisationType.NotSet => Data.DbConstants.OrganisationType.NotSet,
                OrganisationType.CompaniesHouseCompany => Data.DbConstants.OrganisationType.CompaniesHouseCompany,
                OrganisationType.NonCompaniesHouseCompany => Data.DbConstants.OrganisationType.NonCompaniesHouseCompany,
                OrganisationType.WasteCollectionAuthority => Data.DbConstants.OrganisationType.WasteCollectionAuthority,
                OrganisationType.WasteDisposalAuthority => Data.DbConstants.OrganisationType.WasteDisposalAuthority,
                OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority => Data.DbConstants.OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority,
                OrganisationType.Regulators => Data.DbConstants.OrganisationType.Regulators,
                _ => throw new ArgumentException($"Unrecognised Organisation Type: '{organisationType}'")
            };
        }

        public static OrganisationType GetOrganisationType(int typeId)
        {
            return typeId switch
            {
                Data.DbConstants.OrganisationType.NotSet => OrganisationType.NotSet,
                Data.DbConstants.OrganisationType.CompaniesHouseCompany => OrganisationType.CompaniesHouseCompany,
                Data.DbConstants.OrganisationType.NonCompaniesHouseCompany => OrganisationType.NonCompaniesHouseCompany,
                Data.DbConstants.OrganisationType.WasteCollectionAuthority => OrganisationType.WasteCollectionAuthority,
                Data.DbConstants.OrganisationType.WasteDisposalAuthority => OrganisationType.WasteDisposalAuthority,
                Data.DbConstants.OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority => OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority,
                Data.DbConstants.OrganisationType.Regulators => OrganisationType.Regulators,
                _ => throw new ArgumentException($"Unrecognised Organisation Type: '{typeId}'")
            };
        }
    }
}

