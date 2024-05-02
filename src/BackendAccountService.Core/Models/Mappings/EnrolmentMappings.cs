using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Models.Mappings
{
    public static class EnrolmentMappings
    {
        public static Enrolment GetEnrolmentFromAccountModel(AccountModel account, int serviceRoleId)
        {
            return new Enrolment
            {
                ServiceRoleId = serviceRoleId,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Pending,
                Connection = new PersonOrganisationConnection
                {
                    JobTitle = account.Connection.JobTitle,
                    OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer,
                    PersonRoleId = Data.DbConstants.PersonRole.Admin,
                    Person = new Person
                    {
                        FirstName = account.Person.FirstName,
                        LastName = account.Person.LastName,
                        Email = account.Person.ContactEmail,
                        Telephone = account.Person.TelephoneNumber,
                        User = new User
                        {
                            UserId = account.User.UserId,
                            ExternalIdpId = account.User.ExternalIdpId,
                            ExternalIdpUserId = account.User.ExternalIdpUserId,
                            Email = account.User.Email
                        }
                    },
                    Organisation = new Organisation
                    {
                        OrganisationTypeId = OrganisationTypeMappings.GetOrganisationTypeId(account.Organisation.OrganisationType),
                        ProducerTypeId = OrganisationMappings.GetProducerTypeId(account.Organisation.ProducerType),
                        CompaniesHouseNumber = account.Organisation.CompaniesHouseNumber,
                        Name = account.Organisation.Name,
                        SubBuildingName = account.Organisation.Address.SubBuildingName,
                        BuildingName = account.Organisation.Address.BuildingName,
                        BuildingNumber = account.Organisation.Address.BuildingNumber,
                        Street = account.Organisation.Address.Street,
                        Locality = account.Organisation.Address.Locality,
                        DependentLocality = account.Organisation.Address.DependentLocality,
                        Town = account.Organisation.Address.Town,
                        County = account.Organisation.Address.County,
                        Country = account.Organisation.Address.Country,
                        Postcode = account.Organisation.Address.Postcode,
                        ValidatedWithCompaniesHouse = account.Organisation.ValidatedWithCompaniesHouse,
                        IsComplianceScheme = account.Organisation.IsComplianceScheme,
                        NationId = NationMappings.GetNationId(account.Organisation.Nation)
                    }
                }
            };
        }
        
        public static Enrolment GetEnrolmentFromApprovedUserAccountModel(ApprovedUserAccountModel account, int serviceRoleId, UserModel user)
        {
            return new Enrolment
            {
                ServiceRoleId = serviceRoleId,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Pending,
                Connection = new PersonOrganisationConnection
                {
                    JobTitle = account.Connection.JobTitle,
                    OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer,
                    PersonRoleId = Data.DbConstants.PersonRole.Admin,
                    Person = new Person
                    {
                        FirstName = account.Person.FirstName,
                        LastName = account.Person.LastName,
                        Email = account.Person.ContactEmail,
                        Telephone = account.Person.TelephoneNumber,
                        User = new User
                        {
                            UserId = user.UserId,
                            ExternalIdpId = user.ExternalIdpId,
                            ExternalIdpUserId = user.ExternalIdpUserId,
                            Email = user.Email
                        }
                    },
                    Organisation = new Organisation
                    {
                        OrganisationTypeId = OrganisationTypeMappings.GetOrganisationTypeId(account.Organisation.OrganisationType),
                        ProducerTypeId = OrganisationMappings.GetProducerTypeId(account.Organisation.ProducerType),//may have to add to account model
                        CompaniesHouseNumber = account.Organisation.CompaniesHouseNumber,
                        Name = account.Organisation.Name,
                        SubBuildingName = account.Organisation.Address.SubBuildingName,
                        BuildingName = account.Organisation.Address.BuildingName,
                        BuildingNumber = account.Organisation.Address.BuildingNumber,
                        Street = account.Organisation.Address.Street,
                        Locality = account.Organisation.Address.Locality,
                        DependentLocality = account.Organisation.Address.DependentLocality,
                        Town = account.Organisation.Address.Town,
                        County = account.Organisation.Address.County,
                        Country = account.Organisation.Address.Country,
                        Postcode = account.Organisation.Address.Postcode,
                        ValidatedWithCompaniesHouse = account.Organisation.ValidatedWithCompaniesHouse,
                        IsComplianceScheme = account.Organisation.IsComplianceScheme,
                        NationId = NationMappings.GetNationId(account.Organisation.Nation)
                    }
                },
                ApprovedPersonEnrolment = new ApprovedPersonEnrolment
                {
                    NomineeDeclaration = account.DeclarationFullName,
                    NomineeDeclarationTime = (DateTimeOffset)account.DeclarationTimeStamp
                }                
            };
        }
    }
}
