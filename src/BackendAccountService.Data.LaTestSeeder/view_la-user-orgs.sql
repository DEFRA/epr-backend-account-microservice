SELECT Users.Id AS UserPrimaryKey
      ,Users.UserId AS UserGuid
      ,Users.Email AS Username
      ,Persons.Id AS PersonPrimaryKey
      ,Persons.FirstName
      ,Persons.LastName
      ,Persons.Email
      ,Persons.Telephone
      ,Persons.ExternalId
      ,PersonOrgs.JobTitle
      ,PersonOrgs.Id AS ConnectionId
      ,Orgs.Name AS OrgName
      ,Orgs.ExternalId AS OrgId
      ,Orgs.CompaniesHouseNumber
      ,LaOrgs.DistrictCode
      ,Orgs.ReferenceNumber
      ,ServiceRoles.Name
      ,Services.Name
      ,Services.[Key]
  FROM Users
  INNER JOIN Persons ON Persons.UserId = Users.Id
  LEFT JOIN PersonOrganisationConnections AS PersonOrgs ON Persons.Id = PersonOrgs.PersonId
  LEFT JOIN Organisations AS Orgs ON Orgs.Id = PersonOrgs.OrganisationId
  LEFT JOIN LaOrganisations AS LaOrgs ON Orgs.Id = LaOrgs.OrganisationId
  LEFT JOIN Enrolments ON Enrolments.ConnectionId = PersonOrgs.Id
  LEFT JOIN ServiceRoles ON ServiceRoles.Id = Enrolments.ServiceRoleId
  LEFT JOIN Services ON Services.Id = ServiceRoles.ServiceId