# Common Data to Waste Organisations: Agent Guide

## Purpose

This guide explains how organisation and registration data moves from the `epr-common-data-api`,
through the `epr-prn-integration-function`, into the `waste-organisations` service.

Use it when:

- assessing which service owns an organisation identifier or registration state;
- tracing why an organisation was created, updated, or cancelled in Waste Organisations;
- examining another domain flow that publishes organisation data;
- designing an API-first replacement for stored-procedure and scheduled-copy integrations; or
- identifying duplicated, overloaded, or implicit contracts before rationalising services.

This is a description of the current integration, not a proposed target architecture.

## Repositories and source-of-truth locations

The repositories are expected to be cloned as siblings:

```text
epr-backend-account-microservice/
epr-common-data-api/
epr-prn-integration-function/
waste-organisations/
```

The code was inspected at these commits:

| Repository | Commit | Relevant responsibility |
|---|---:|---|
| `epr-backend-account-microservice` | `add07da` | Source model for organisations, compliance schemes and selected schemes |
| `epr-common-data-api` | `dc6ba06` | Delta API and stored-procedure execution |
| `epr-prn-integration-function` | `f622068` | Scheduling, mapping and outbound PUT requests |
| `waste-organisations` | `4ab344d` | Organisation upsert and registration persistence |

Start investigations in these files:

| Boundary | File |
|---|---|
| Common Data HTTP endpoint | `epr-common-data-api/src/EPR.CommonDataService.Api/Controllers/ProducerDetailsController.cs` |
| Stored-procedure invocation | `epr-common-data-api/src/EPR.CommonDataService.Core/Services/ProducerDetailsService.cs` |
| Delta query | `epr-common-data-api/src/EPR.CommonDataService.Data/Scripts/Stored Procedures/sp_Organisations_Delta_Extract.sql` |
| Common Data response model | `epr-common-data-api/src/EPR.CommonDataService.Data/Entities/UpdatedProducersResponseModelV2.cs` |
| Scheduled integration | `epr-prn-integration-function/src/EprPrnIntegration.Api/Functions/UpdateWasteOrganisationsFunction.cs` |
| Common Data client | `epr-prn-integration-function/src/EprPrnIntegration.Common/RESTServices/CommonService/CommonDataService.cs` |
| Request mapper | `epr-prn-integration-function/src/EprPrnIntegration.Common/Mappers/WasteOrganisationsApiUpdateRequestMapper.cs` |
| Waste Organisations client | `epr-prn-integration-function/src/EprPrnIntegration.Common/RESTServices/WasteOrganisationsService/WasteOrganisationsService.cs` |
| Waste Organisations PUT endpoint | `waste-organisations/src/Api/Endpoints/Organisations/Put.cs` |
| Waste Organisations patch semantics | `waste-organisations/src/Api/Services/OrganisationRegistrationService.cs` |

## End-to-end flow

![End-to-end organisation data flow](organisation-diagrams/03-end-to-end-data-flow.png)

At a high level:

1. `UpdateWasteOrganisationsFunction` runs on a timer.
2. It reads the last successful extraction checkpoint.
3. It sets the upper boundary to the current UTC time.
4. It calls the Common Data endpoint:

   ```http
   GET /api/producer-details/updated-producers?from={lastUpdate}&to={utcNow}
   ```

5. Common Data executes:

   ```sql
   EXECUTE [dbo].[sp_Organisations_Delta_Extract] @From_Date, @To_Date
   ```

6. The stored procedure returns zero or more denormalised organisation-registration records.
7. The function maps each result to the Waste Organisations write contract.
8. It sends:

   ```http
   PUT /organisations/{pEPRID}
   ```

9. Waste Organisations creates the organisation when the ID is unknown, or patches the existing
   organisation and the registration identified by `(type, registrationYear)`.
10. When the batch completes successfully, the function saves `utcNow` as the next checkpoint.

The transport is synchronous HTTP at both service boundaries. The integration function processes
records sequentially.

## Source data model

![Organisation and compliance-scheme data model](organisation-diagrams/01-organisation-compliance-scheme-data-model.png)

The producer-to-scheme link is:

![Producer-to-scheme membership](organisation-diagrams/02-producer-to-scheme-membership.png)

The important distinction is that a compliance-scheme operator and a compliance scheme are
different records:

- `Organisations` represents the legal/operator organisation.
- `ComplianceSchemes` represents a scheme registration, including its scheme name, nation and
  external ID.
- `OrganisationsConnections` links a producer organisation to a scheme-operator organisation.
- `SelectedSchemes` connects that producer/operator relationship to a particular
  `ComplianceSchemes` row.

In simplified form:

```text
Producer Organisation
  -> OrganisationsConnection
       From role: Producer
       To organisation: Compliance-scheme operator
       To role: Compliance Scheme
  -> SelectedScheme
  -> ComplianceScheme
```

## What the Common Data API exposes

The endpoint returns `200 OK` with an array when records exist and `204 No Content` when no records
exist. The integration client converts `204` into an empty list.

The API response is shaped as:

```json
{
  "organisationName": "string",
  "tradingName": "string",
  "organisationType": "DP or CS",
  "companiesHouseNumber": "string",
  "addressLine1": "string",
  "addressLine2": "string",
  "town": "string",
  "county": "string",
  "country": "string",
  "postcode": "string",
  "pEPRID": "UUID",
  "status": "Registered or Deleted",
  "businessCountry": "England, Northern Ireland, Scotland or Wales",
  "updatedDateTime": "date-time",
  "registrationYear": "YYYY"
}
```

This is a denormalised integration projection. It is not a canonical organisation resource:

- `pEPRID` has different meanings for different organisation types.
- `status` is translated into registration status by the integration function.
- `updatedDateTime` controls delta selection but is not sent to Waste Organisations.
- names and addresses come from different source tables depending on the record type.

## Stored-procedure selection logic

The procedure builds two intermediate sets.

### `latest_record`

This set:

- reads submitted parent organisation rows from `rpd.CompanyDetails`;
- excludes subsidiaries using `subsidiary_id IS NULL`;
- joins file metadata by `FileName`;
- derives the registration year from `SubmissionPeriod`;
- identifies the latest submission per organisation, reference number and year; and
- labels a submission `DP` when metadata does not resolve to a compliance scheme, otherwise `CS`.

`SubmittedBy = 'DP'` is therefore a submission-channel inference. It is not a check that the
organisation currently has no `SelectedSchemes` membership.

### `Active_ComplianceScheme`

This set selects the latest `CompanyDetails` file for each compliance scheme and submission period.
Despite its name, it does not itself filter on `ComplianceSchemes.IsDeleted`.

## The four emitted record classes

![Record transformation](organisation-diagrams/04-record-transformation.png)

### 1. Registered direct producer

Selection:

- active `Organisations` row;
- `IsComplianceScheme = 0`;
- latest parent submission classified as `DP`;
- organisation size `L`, with null also treated as `L`; and
- submission timestamp within the requested delta window.

Identity:

```text
pEPRID = Organisations.ExternalId
```

Data lineage:

- name, trading name and address come from submitted `CompanyDetails`;
- Companies House number comes from `Organisations`;
- business country comes from the organisation's `NationId`;
- registration year comes from the submission period.

Waste Organisations registration:

```json
{
  "type": "LARGE_PRODUCER",
  "status": "REGISTERED"
}
```

### 2. Deleted direct producer

Selection:

- deleted `Organisations` row;
- `IsComplianceScheme = 0`;
- latest parent submission classified as `DP`;
- organisation size `L`, with null also treated as `L`; and
- `Organisations.LastUpdatedOn` within the delta window.

Identity:

```text
pEPRID = Organisations.ExternalId
```

The latest submitted details are still used for name and address. The deletion event is translated
into a cancelled registration:

```json
{
  "type": "LARGE_PRODUCER",
  "status": "CANCELLED"
}
```

The organisation document is not deleted from Waste Organisations.

### 3. Registered compliance scheme

Selection requires an active chain:

```text
active scheme-operator Organisation
  <- active OrganisationsConnection
  <- active SelectedScheme
  -> active ComplianceScheme
  -> latest CompanyDetails submission metadata
```

Identity:

```text
pEPRID = ComplianceSchemes.ExternalId
```

Data lineage:

- legal name, Companies House number and address come from the operator `Organisation`;
- trading name comes from `ComplianceSchemes.Name`;
- business country comes from `ComplianceSchemes.NationId`;
- registration year comes from the scheme submission period.

Waste Organisations registration:

```json
{
  "type": "COMPLIANCE_SCHEME",
  "status": "REGISTERED"
}
```

The operator's `Organisation.ExternalId` is not used as the target ID. One operator may therefore
produce multiple Waste Organisations resources when it owns multiple scheme registrations.

The query also requires at least one selected-scheme relationship. A scheme with no
`SelectedSchemes` row is not emitted. The `cs_not_sub` join in the procedure does not remove this
requirement and does not contribute fields to the output.

### 4. Deleted compliance scheme

Selection requires:

- a compliance-scheme operator organisation;
- an organisation connection and selected-scheme link;
- a linked deleted `ComplianceSchemes` row;
- prior `CompanyDetails` submission metadata; and
- `ComplianceSchemes.LastUpdatedOn` within the delta window.

Unlike the registered branch, this branch does not require the organisation connection or selected
scheme to remain active.

Identity:

```text
pEPRID = ComplianceSchemes.ExternalId
```

Waste Organisations registration:

```json
{
  "type": "COMPLIANCE_SCHEME",
  "status": "CANCELLED"
}
```

The deletion cancels the registration for the scheme ID; it does not delete the Waste Organisations
document or cancel every registration held by that document.

## Mapping into the Waste Organisations write contract

The function maps every source record to:

```http
PUT /organisations/{pEPRID}
```

```json
{
  "name": "string",
  "tradingName": "string or null",
  "businessCountry": "GB-ENG, GB-NIR, GB-SCT, GB-WLS or null",
  "companiesHouseNumber": "string or null",
  "address": {
    "addressLine1": "string or null",
    "addressLine2": "string or null",
    "town": "string or null",
    "county": "string or null",
    "postcode": "string or null",
    "country": "string or null"
  },
  "registration": {
    "status": "REGISTERED or CANCELLED",
    "type": "LARGE_PRODUCER or COMPLIANCE_SCHEME",
    "registrationYear": 2025
  }
}
```

Field transformations:

| Common Data value | Waste Organisations value |
|---|---|
| `DP` | `LARGE_PRODUCER` |
| `CS` | `COMPLIANCE_SCHEME` |
| `Registered` | `REGISTERED` |
| `Deleted` | `CANCELLED` |
| `England` | `GB-ENG` |
| `Northern Ireland` | `GB-NIR` |
| `Scotland` | `GB-SCT` |
| `Wales` | `GB-WLS` |
| unknown business country | `null` |
| string `registrationYear` | integer `registrationYear` |

`pEPRID` is used only as the URL resource identifier. `updatedDateTime` is not included in the
request body.

Mapping fails before the PUT when:

- `pEPRID` is null;
- organisation name is null;
- organisation type is not `DP` or `CS`;
- status is not `Registered` or `Deleted`; or
- registration year cannot be parsed as an integer.

These mapper exceptions occur inside the function's per-record HTTP helper. Except for
`TaskCanceledException`, an exception thrown there is logged and converted into a failed record,
after which processing continues. If no later terminating error occurs, the batch checkpoint is
still advanced. A malformed record can therefore be skipped without being selected again by the
same delta window.

## Waste Organisations persistence semantics

`PUT /organisations/{id}` is an authenticated create-or-update operation.

When `{id}` does not exist:

- a new organisation document is created using `{id}`;
- the supplied organisation details are stored; and
- the supplied registration becomes the first registration.

When `{id}` already exists:

- name, trading name, business country, Companies House number and the complete address are
  overwritten with the supplied values;
- the supplied registration is keyed by `(registration.type, registration.registrationYear)`;
- that one registration is added or updated; and
- registrations with other types or years are preserved.

If the registration status is unchanged, the existing registration timestamps are preserved. If
the status changes, for example from `REGISTERED` to `CANCELLED`, its `updated` timestamp changes.

This means the Common Data feed is simultaneously:

- synchronising mutable organisation details; and
- applying one registration-state assertion per request.

Those are separate domain concerns combined into one write contract.

## Scheduling, checkpoints and failure behaviour

The configured schedule is:

```text
1,31 0-7 * * *
```

The default extraction start date is `2024-01-01` when no checkpoint exists.

Checkpoint behaviour:

- the upper boundary is captured before calling Common Data;
- after a non-empty batch completes, that upper boundary is saved as the next checkpoint;
- if Common Data returns no records, the function exits without advancing the checkpoint;
- a Common Data exception prevents checkpoint advancement;
- a timeout prevents checkpoint advancement;
- transient HTTP responses and response statuses `401`, `403` and `404` terminate the batch and
  prevent checkpoint advancement;
- other non-success HTTP responses are logged per record while processing continues;
- non-timeout exceptions inside an individual mapping or PUT attempt are logged per record while
  processing continues; and
- successfully processed earlier records may be sent again when a later terminating failure causes
  the same window to be retried.

The integration therefore relies on idempotent PUT behaviour.

The stored procedure uses inclusive `BETWEEN @From_Date AND @To_Date` conditions. Because the next
window starts from the previous upper boundary, records exactly on a boundary may be returned
again. This is another reason consumers must tolerate replay.

When a run finds no records, retaining the old checkpoint means later runs query from the same
older lower boundary rather than from the previous empty run.

## Important modelling observations

### `pEPRID` is an overloaded identifier

| Source record | Meaning of `pEPRID` |
|---|---|
| Direct producer | `Organisations.ExternalId` |
| Compliance scheme | `ComplianceSchemes.ExternalId` |

Agents must not assume it always identifies an account-service organisation.

### Submission channel is being used as organisation classification

The direct-producer branches classify records from file metadata. They do not inspect the
producer's current `SelectedSchemes` membership.

Consequences:

- a producer submitted by a scheme is not emitted as an individual `DP` record;
- a producer that submitted directly may be emitted as `DP` without proving it is currently a
  direct registrant; and
- current membership and historical submission channel can diverge.

### Scheme identity and operator identity are collapsed into one document shape

For a compliance-scheme record:

- the Waste Organisations ID and trading name describe the scheme;
- the legal name, Companies House number and address describe the operator organisation; and
- the business country describes the scheme's nation.

The result is a projection containing attributes from two source entities.

### Cancellation is registration-level, not organisation-level

`Deleted` from Common Data becomes `CANCELLED` for one registration key. The Waste Organisations
resource remains available and can retain other registrations.

### The integration contract is implicit

The actual contract is spread across:

- stored-procedure column aliases;
- keyless Common Data EF models;
- integration DTO property names;
- mapper string constants; and
- the Waste Organisations API DTO.

A change can compile in one repository while breaking deserialisation or domain meaning elsewhere.

### Known query asymmetries

- Registered compliance schemes use `COALESCE(BuildingName, BuildingNumber)` for address line one;
  deleted compliance schemes use only `BuildingName`.
- The stored-procedure history says a deleted scheme can derive registration year from
  `LastUpdatedOn` when the submission period is null, but the current query returns
  `Active_ComplianceScheme.SubmissionPeriodYear` without that fallback.
- `Active_ComplianceScheme` can contain one latest row per scheme and submission period, not one
  row per scheme overall.
- The four result branches use `UNION ALL`; `DISTINCT` only deduplicates within each branch.

## Agent investigation playbook

When assessing another part of the domain, trace the flow in this order.

### 1. Identify the domain event

State the business event without reference to tables or transport:

```text
Examples:
- a producer registers for a year;
- a producer leaves a compliance scheme;
- a compliance scheme is cancelled;
- an operator changes address.
```

Determine whether the current flow represents that event directly, infers it from state, or infers
it from submission history.

### 2. Identify all domain identities

Record each identifier and its owning aggregate:

| Question | Example in this flow |
|---|---|
| What identifies the legal organisation? | `Organisations.ExternalId` |
| What identifies a scheme registration? | `ComplianceSchemes.ExternalId` |
| What identifies the membership? | `SelectedSchemes.ExternalId` |
| What identifies the downstream projection? | overloaded `pEPRID` |
| What identifies registration state? | `(type, registrationYear)` |

Do not use a generic name such as `organisationId` until its semantics are established.

### 3. Trace field ownership

For every outbound field, record:

- source table or API;
- transformation;
- authoritative owner;
- freshness trigger;
- nullability;
- whether it belongs to the identified resource; and
- whether another service can overwrite it.

Pay particular attention to composite projections such as compliance schemes, where legal
organisation fields and scheme fields are merged.

### 4. Trace lifecycle semantics

For create, update, cancellation and deletion, determine:

- whether the operation changes an entity or a registration;
- whether deletion means hard delete, soft delete or status transition;
- which timestamp enters the delta feed;
- whether historical details remain available; and
- whether replay produces the same result.

### 5. Trace extraction and delivery guarantees

Capture:

- polling schedule;
- initial checkpoint;
- checkpoint storage and advancement;
- inclusive or exclusive date boundaries;
- ordering;
- pagination or batch limits;
- retry behaviour;
- poison-record behaviour;
- partial-success behaviour; and
- idempotency key.

### 6. Compare source and destination contracts

Check for:

- overloaded identifiers;
- string-coded enums;
- fields dropped by the mapper;
- source values silently mapped to null;
- date or timezone loss;
- one source record updating multiple destination concerns; and
- destination capabilities that the integration does not use.

### 7. Produce an API-first candidate

Describe a candidate contract in domain language before proposing service boundaries.

Prefer separate resources and operations where the current feed combines concerns. For example:

```http
PUT /organisations/{organisationId}
PUT /compliance-schemes/{schemeId}
PUT /organisations/{organisationId}/registrations/{type}/{year}
PUT /compliance-schemes/{schemeId}/registrations/{year}
PUT /compliance-schemes/{schemeId}/members/{producerOrganisationId}
DELETE /compliance-schemes/{schemeId}/members/{producerOrganisationId}
```

These paths are examples for analysis, not an agreed target API.

For each candidate API, specify:

- resource identity and owner;
- request and response schema;
- lifecycle/status model;
- idempotency behaviour;
- event or change-feed requirements;
- authorisation boundary;
- source-of-truth service;
- migration and coexistence strategy; and
- consumers that currently depend on the denormalised projection.

## Questions for rationalising this flow

Agents assessing a target architecture should answer:

1. Should Waste Organisations use legal organisation identity, scheme identity, or support both as
   explicit resource types?
2. Should organisation details and registration updates remain one PUT operation?
3. Which service is authoritative for operator address and Companies House data?
4. Which service is authoritative for compliance-scheme name and nation?
5. Should a producer's direct/indirect classification come from current membership rather than
   submission metadata?
6. Should membership changes be represented as explicit API operations or events?
7. Should Common Data expose a stored-procedure projection, or should source services publish
   versioned domain contracts?
8. How should replays, out-of-order updates and partial batch failures be handled?
9. Should the downstream identifier be split into `organisationId` and `complianceSchemeId`?
10. Which other domains write organisation details or registrations into Waste Organisations, and
    can they overwrite each other?

## Minimum evidence required for future assessments

An agent proposing a change should provide:

- a current-state sequence or flow diagram;
- an identity table;
- field-level lineage for the proposed contract;
- all writers and readers of the affected data;
- lifecycle and deletion semantics;
- delivery and replay guarantees;
- known discrepancies between code, database and API terminology;
- a proposed API/resource model;
- unresolved ownership decisions; and
- a migration plan that accounts for existing consumers.

Avoid proposing a rationalised service solely from table joins. Start with domain identities,
events and ownership, then use the existing data flow as evidence of current coupling.
