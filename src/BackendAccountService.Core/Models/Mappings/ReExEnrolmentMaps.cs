using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Models.Mappings
{
    public class ReExEnrolmentMaps : IReExEnrolmentMaps
    {
        public Enrolment GetAdminEnrolmentForCurrentUser(
            ReprocessorExporterAddOrganisation account,
            Person person)
        {
            var enrolment = new Enrolment
            {
                ServiceRoleId = Data.DbConstants.ServiceRole.ReprocessorExporter.AdminUser.Id,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
                Connection = new PersonOrganisationConnection
                {
                    JobTitle = account.User.JobTitle,
                    OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer,
                    PersonId = person.Id,
                    PersonRoleId = Data.DbConstants.PersonRole.Admin,
                    // have helper for org mapping from Organisation?
                    Organisation = new Organisation
                    {
                        OrganisationTypeId = OrganisationTypeMappings.GetOrganisationTypeId(account.Organisation.OrganisationType),
                        ProducerTypeId = OrganisationMappings.GetProducerTypeId(account.Organisation.ProducerType),//may have to add to account model
                        CompaniesHouseNumber = account.Organisation.CompaniesHouseNumber,
                        Name = account.Organisation.Name,
                        TradingName = account.Organisation.TradingName,
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
                        NationId = NationMappings.GetNationId((Nation)account.Organisation.Nation),
                        ExternalId = Guid.NewGuid()
                    }
                }
            };

            return enrolment;
        }

        public Enrolment GetApprovedPersonEnrolmentForCurrentUser(
            ReprocessorExporterAddOrganisation account,
            Person person,
            PersonOrganisationConnection connection)
        {
            return new Enrolment
            {
                ServiceRoleId = Data.DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Enrolled,
                Connection = connection,
                ApprovedPersonEnrolment = new ApprovedPersonEnrolment
                {
                    NomineeDeclaration = $"{person.FirstName} {person.LastName}",
                    NomineeDeclarationTime = (DateTimeOffset)account.DeclarationTimeStamp
                }
            };
        }

        public Enrolment GetEnrolmentForInvitedApprovedUser(
            Organisation organisation,
            InvitedApprovedUserModel invitedApprovedUser,
            string inviterEmail,
            Person personToInvite)
        {
            var enrolment = new Enrolment
            {
                ServiceRoleId = Data.DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
                EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Invited,
                Connection = new PersonOrganisationConnection
                {
                    JobTitle = invitedApprovedUser.JobTitle,
                    OrganisationRoleId = Data.DbConstants.OrganisationRole.Employer,
                    Person = personToInvite,
                    PersonRoleId = Data.DbConstants.PersonRole.Employee,
                    Organisation = organisation
                }
            };

            //As per architects it should be set to member if its LP or LLP
            if ((organisation.ProducerTypeId ?? 0) is Data.DbConstants.ProducerType.LimitedPartnership
                or Data.DbConstants.ProducerType.LimitedLiabilityPartnership)
            {
                enrolment.Connection.PersonRoleId = Data.DbConstants.PersonRole.Member;
            }

            return enrolment;
        }
    }
}
