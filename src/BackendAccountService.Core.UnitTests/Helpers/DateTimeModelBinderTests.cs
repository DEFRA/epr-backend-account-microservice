using BackendAccountService.Core.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System.Globalization;

namespace BackendAccountService.Core.UnitTests.Helpers;

[TestClass]
public class DateTimeModelBinderTests
{
    private const string ExpectedDateFormat = "yyyy-MM-ddT00:00:00";

    [TestMethod]
    public async Task BindModelAsync_ValidDate_SetsModelSuccessfully()
    {
        // Arrange
        var binder = new DateTimeModelBinder(ExpectedDateFormat);
        var modelName = "testDate";
        var validDate = "2024-10-01T00:00:00";

        var mockValueProvider = new Mock<IValueProvider>();
        mockValueProvider.Setup(v => v.GetValue(modelName)).Returns(new ValueProviderResult(validDate));

        var bindingContext = new DefaultModelBindingContext
        {
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ValueProvider = mockValueProvider.Object
        };

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        Assert.IsNotNull(bindingContext.Result.Model);
        Assert.AreEqual(DateTime.ParseExact(validDate, ExpectedDateFormat, CultureInfo.InvariantCulture), bindingContext.Result.Model);
    }

    [TestMethod]
    public async Task BindModelAsync_InvalidDate_AddsModelStateError()
    {
        // Arrange
        var binder = new DateTimeModelBinder(ExpectedDateFormat);
        var modelName = "testDate";
        var invalidDate = "10/01/2024"; // Not in yyyy-MM-ddT00:00:00 format

        var mockValueProvider = new Mock<IValueProvider>();
        mockValueProvider.Setup(v => v.GetValue(modelName)).Returns(new ValueProviderResult(invalidDate));

        var bindingContext = new DefaultModelBindingContext
        {
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ValueProvider = mockValueProvider.Object
        };

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        Assert.IsFalse(bindingContext.ModelState.IsValid);
        Assert.IsTrue(bindingContext.ModelState.ContainsKey(modelName));
        Assert.AreEqual($"Invalid date and time format.", bindingContext.ModelState[modelName].Errors[0].ErrorMessage);
        Assert.IsNull(bindingContext.Result.Model);
    }

    [TestMethod]
    public async Task BindModelAsync_NullValue_DoesNotSetModelOrAddError()
    {
        // Arrange
        var binder = new DateTimeModelBinder(ExpectedDateFormat);
        var modelName = "testDate";

        var mockValueProvider = new Mock<IValueProvider>();
        mockValueProvider.Setup(v => v.GetValue(modelName)).Returns(ValueProviderResult.None);

        var bindingContext = new DefaultModelBindingContext
        {
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ValueProvider = mockValueProvider.Object
        };

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        Assert.IsNull(bindingContext.Result.Model);
    }

    [TestMethod]
    public async Task BindModelAsync_EmptyString_DoesNotSetModelOrAddError()
    {
        // Arrange
        var binder = new DateTimeModelBinder(ExpectedDateFormat);
        var modelName = "testDate";

        var mockValueProvider = new Mock<IValueProvider>();
        mockValueProvider.Setup(v => v.GetValue(modelName)).Returns(new ValueProviderResult(string.Empty));

        var bindingContext = new DefaultModelBindingContext
        {
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ValueProvider = mockValueProvider.Object
        };

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        Assert.IsNull(bindingContext.Result.Model);
    }
}