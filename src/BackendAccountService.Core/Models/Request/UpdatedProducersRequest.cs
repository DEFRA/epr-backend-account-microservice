using System.ComponentModel.DataAnnotations;
using BackendAccountService.Core.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BackendAccountService.Core.Models.Request;

public class UpdatedProducersRequest : IValidatableObject
{
    [Required(ErrorMessage = "From date is required.")]
    [ModelBinder(BinderType = typeof(DateTimeModelBinder))]

    public DateTime From { get; set; }

    [Required(ErrorMessage = "To date is required.")]
    [ModelBinder(BinderType = typeof(DateTimeModelBinder))]
    public DateTime To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From == DateTime.MinValue || From == DateTime.MaxValue || From == default(DateTime))
        {
            yield return new ValidationResult("From date must be a valid date and not the minimum, maximum, or default date.", new[] { nameof(From) });
        }

        if (To == DateTime.MinValue || To == DateTime.MaxValue || To == default(DateTime))
        {
            yield return new ValidationResult("To date must be a valid date and not the minimum, maximum, or default date.", new[] { nameof(To) });
        }

        if (To < From)
        {
            yield return new ValidationResult("The 'To' date must not be earlier than the 'From' date.", new[] { nameof(To) });
        }
    }
}