using System.Collections.Specialized;
using System.Text.Json;
using BackendAccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BackendAccountService.Data.Infrastructure;

public class AuditEntry
{
    private readonly Guid? _userId;
    private readonly Guid? _organisationId;
    private readonly string? _serviceId;
    private readonly DateTimeOffset _timestamp;
    private readonly string _entity;
    private readonly string _operation;
    private int? _internalId;
    private Guid? _externalId;
    private readonly OrderedDictionary _oldValues = new();
    private readonly OrderedDictionary _newValues = new();
    private readonly List<PropertyEntry> _generatedProperties = new();
    private readonly List<string> _changes = new();

    public AuditEntry(EntityEntry entry, Guid? userId, Guid? organisationId, string? serviceId, DateTimeOffset timestamp)
    {
        _userId = userId;
        _organisationId = organisationId;
        _serviceId = serviceId;
        _timestamp = timestamp;

        _entity = entry.Entity.GetType().Name;
        _operation = entry.State.ToString();

        foreach (var property in entry.Properties)
        {
            TryExtractInternalId(property);
            TryExtractExternalId(property);
            TryExtractOldValues(entry, property);
            TryExtractNewValues(entry, property);
            TryExtractChanges(entry, property);
        }
    }

    private void TryExtractInternalId(PropertyEntry property)
    {
        if (property.Metadata.Name == "Id")
        {
            _internalId = property.CurrentValue as int?;
        }
    }

    private void TryExtractExternalId(PropertyEntry property)
    {
        if (property.Metadata.Name == "ExternalId")
        {
            _externalId = property.CurrentValue as Guid?;
        }
        else if (property.Metadata.Name == "UserId" && property.EntityEntry.Entity.GetType() == typeof(User))
        {
            _externalId = property.CurrentValue as Guid?;
        }
    }

    private void TryExtractOldValues(EntityEntry entry, PropertyEntry property)
    {
        if (entry.State is EntityState.Deleted or EntityState.Modified)
        {
            _oldValues[property.Metadata.Name] = property.OriginalValue;
        }
    }

    private void TryExtractNewValues(EntityEntry entry, PropertyEntry property)
    {
        if (entry.State is EntityState.Modified or EntityState.Added)
        {
            _newValues[property.Metadata.Name] = property.CurrentValue;

            if (property.IsTemporary || IsValueGeneratedOnAdd(entry, property) || IsValueGeneratedOnUpdate(entry, property))
            {
                _generatedProperties.Add(property);
            }
        }
    }

    private static bool IsValueGeneratedOnAdd(EntityEntry entry, PropertyEntry property)
    {
        return entry.State is EntityState.Added &&
               property.Metadata.ValueGenerated.HasFlag(ValueGenerated.OnAdd);
    }

    private static bool IsValueGeneratedOnUpdate(EntityEntry entry, PropertyEntry property)
    {
        return entry.State is EntityState.Modified &&
               (property.Metadata.ValueGenerated.HasFlag(ValueGenerated.OnUpdate) ||
                property.Metadata.ValueGenerated.HasFlag(ValueGenerated.OnUpdateSometimes));
    }

    private void TryExtractChanges(EntityEntry entry, PropertyEntry property)
    {
        if (entry.State is EntityState.Modified && property.IsModified)
        {
            _changes.Add(property.Metadata.Name);
        }
    }

    public bool HasGeneratedProperties() => _generatedProperties.Count > 0;

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            UserId = _userId,
            OrganisationId = _organisationId,
            ServiceId = _serviceId,
            Timestamp = _timestamp,
            Entity = _entity,
            Operation = _operation,
            InternalId = _internalId,
            ExternalId = _externalId,
            OldValues = _oldValues.Count > 0 ? JsonSerializer.Serialize(_oldValues) : null,
            NewValues = _newValues.Count > 0 ? JsonSerializer.Serialize(_newValues) : null,
            Changes = _changes.Count > 0 ? JsonSerializer.Serialize(_changes) : null
        };
    }

    public void UpdateGeneratedProperties()
    {
        foreach (var property in _generatedProperties)
        {
            TryExtractInternalId(property);
            TryExtractExternalId(property);

            _newValues[property.Metadata.Name] = property.CurrentValue;
        }
    }
}