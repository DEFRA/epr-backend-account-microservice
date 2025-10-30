using BackendAccountService.Core.Models;
using FluentValidation;

namespace BackendAccountService.Api.Validators;

public class PersonsDetailsRequestValidator : AbstractValidator<PersonsDetailsRequestDto>
{
    public PersonsDetailsRequestValidator()
    {
        RuleFor(x => x.UserIds)
            .NotNull().WithMessage("UserIds list can not be null")
            .Must(list => list != null && list.Count > 0)
            .WithMessage("UserIds list must contain at least one item.");

        RuleFor(x => x.OrgId)
        .Must(id => id == null || id != Guid.Empty)
        .WithMessage("OrgId must be a valid non-empty Guid if provided.");
    }
}
