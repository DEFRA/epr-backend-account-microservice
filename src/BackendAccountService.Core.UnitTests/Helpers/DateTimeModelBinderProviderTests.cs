using BackendAccountService.Core.Helpers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace BackendAccountService.Core.UnitTests.Helpers;

[TestClass]
public class DateTimeModelBinderProviderTests
{
    private class CustomModelBinderProviderContext : ModelBinderProviderContext
    {
        private readonly ModelMetadata _modelMetadata;

        public CustomModelBinderProviderContext(Type modelType)
        {
            _modelMetadata = new DefaultModelMetadata(
                new EmptyModelMetadataProvider(),
                new DefaultCompositeMetadataDetailsProvider(Array.Empty<IMetadataDetailsProvider>()),
                new DefaultMetadataDetails(ModelMetadataIdentity.ForType(modelType), ModelAttributes.GetAttributesForType(modelType))
            );
        }

        public override BindingInfo BindingInfo => null;
        public override ModelMetadata Metadata => _modelMetadata;
        public override IModelMetadataProvider MetadataProvider { get; }
        public override IModelBinder CreateBinder(ModelMetadata metadata) => null;
    }

    [TestMethod]
    public void GetBinder_ShouldReturnDateTimeModelBinder_ForDateTimeType()
    {
        // Arrange
        var provider = new DateTimeModelBinderProvider();
        var context = new CustomModelBinderProviderContext(typeof(DateTime));

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.IsNotNull(binder);
        Assert.IsInstanceOfType(binder, typeof(DateTimeModelBinder));
    }

    [TestMethod]
    public void GetBinder_ShouldReturnDateTimeModelBinder_ForNullableDateTimeType()
    {
        // Arrange
        var provider = new DateTimeModelBinderProvider();
        var context = new CustomModelBinderProviderContext(typeof(DateTime?));

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.IsNotNull(binder);
        Assert.IsInstanceOfType(binder, typeof(DateTimeModelBinder));
    }

    [TestMethod]
    public void GetBinder_ShouldReturnNull_ForNonDateTimeType()
    {
        // Arrange
        var provider = new DateTimeModelBinderProvider();
        var context = new CustomModelBinderProviderContext(typeof(string));

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.IsNull(binder);
    }
}