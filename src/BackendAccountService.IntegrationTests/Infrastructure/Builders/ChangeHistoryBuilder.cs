using System.Text.Json;
using BackendAccountService.Core.Models;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds an active ChangeHistory row tied to the producer's Person+Organisation. The mapper
/// (ChangeHistoryMappings.GetChangeHistoryModelFromChangeHistory) deserialises both NewValues
/// and OldValues without null-checking, so both columns are populated with valid JSON.
/// Returns the ChangeHistory.ExternalId.
/// </summary>
public sealed class ChangeHistoryBuilder(TestBuilders builders, BuiltEnrolment producer)
{
    public async Task<Guid> Build()
    {
        var externalId = Guid.NewGuid();
        var oldValues = JsonSerializer.Serialize(new UserDetailsChangeModel
        {
            FirstName = producer.Person.FirstName,
            LastName = producer.Person.LastName,
            JobTitle = "Old Job",
        });
        var newValues = JsonSerializer.Serialize(new UserDetailsChangeModel
        {
            FirstName = "Updated",
            LastName = "Name",
            JobTitle = "Updated Job",
        });
        await builders.WithDbContextAsync(ctx =>
        {
            ctx.ChangeHistory.Add(new ChangeHistory
            {
                ExternalId = externalId,
                PersonId = producer.Person.Id,
                OrganisationId = producer.Organisation.Id,
                IsActive = true,
                DeclarationDate = DateTimeOffset.UtcNow,
                OldValues = oldValues,
                NewValues = newValues,
            });
            return Task.CompletedTask;
        }, save: true);
        return externalId;
    }
}
