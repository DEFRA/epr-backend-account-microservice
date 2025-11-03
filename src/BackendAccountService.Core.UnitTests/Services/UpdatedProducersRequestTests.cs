using BackendAccountService.Core.Models.Request;
using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class UpdatedProducersRequestTests
{
    private static List<ValidationResult> ValidateModel(UpdatedProducersRequest request)
    {
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);
        validationResults.AddRange(request.Validate(new ValidationContext(request)));
        return validationResults;
    }

    [TestMethod]
    public void Validate_ShouldReturnError_WhenFromIsMinValue()
    {
        var request = new UpdatedProducersRequest
        {
            From = DateTime.MinValue,
            To = DateTime.UtcNow
        };

        var results = ValidateModel(request);

        Assert.IsTrue(results.Exists(v => v.ErrorMessage.Contains("From date must be a valid date and not the minimum, maximum, or default date.")));
    }

    [TestMethod]
    public void Validate_ShouldReturnError_WhenToIsMaxValue()
    {
        var request = new UpdatedProducersRequest
        {
            From = DateTime.UtcNow.AddDays(-1),
            To = DateTime.MaxValue
        };

        var results = ValidateModel(request);

        Assert.IsTrue(results.Exists(v => v.ErrorMessage.Contains("To date must be a valid date and not the minimum, maximum, or default date.")));
    }

    [TestMethod]
    public void Validate_ShouldReturnError_WhenFromIsDefaultDate()
    {
        var request = new UpdatedProducersRequest
        {
            From = default,
            To = DateTime.UtcNow
        };

        var results = ValidateModel(request);

        Assert.IsTrue(results.Exists(v => v.ErrorMessage.Contains("From date must be a valid date and not the minimum, maximum, or default date.")));
    }

    [TestMethod]
    public void Validate_ShouldReturnError_WhenToIsDefaultDate()
    {
        var request = new UpdatedProducersRequest
        {
            From = DateTime.UtcNow,
            To = default
        };

        var results = ValidateModel(request);

        Assert.IsTrue(results.Exists(v => v.ErrorMessage.Contains("To date must be a valid date and not the minimum, maximum, or default date.")));
    }

    [TestMethod]
    public void Validate_ShouldReturnError_WhenToIsEarlierThanFrom()
    {
        var request = new UpdatedProducersRequest
        {
            From = DateTime.UtcNow,
            To = DateTime.UtcNow.AddDays(-1)
        };

        var results = ValidateModel(request);

        Assert.IsTrue(results.Exists(v => v.ErrorMessage.Contains("The 'To' date must not be earlier than the 'From' date.")));
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenDatesAreValidAndToIsAfterFrom()
    {
        var request = new UpdatedProducersRequest
        {
            From = DateTime.UtcNow.AddDays(-5),
            To = DateTime.UtcNow
        };

        var results = ValidateModel(request);

        Assert.IsLessThanOrEqualTo(0, results.Count); // No validation errors
    }
}