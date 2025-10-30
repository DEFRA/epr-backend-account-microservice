using System;
using System.Collections.Generic;
using System.Linq;
using BackendAccountService.Api.Validators;
using BackendAccountService.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackendAccountService.Api.UnitTests.Validators;

[TestClass]
public class PersonsDetailsRequestValidatorTests
{
    private PersonsDetailsRequestValidator _validator;

    [TestInitialize]
    public void TestInitialize()
    {
        _validator = new PersonsDetailsRequestValidator();
    }

    [TestMethod]
    public void Should_Not_Have_Validation_Errors_When_Request_Is_Valid()
    {
        var request = new PersonsDetailsRequestDto
        {
            UserIds = new List<Guid> { Guid.NewGuid() },
            OrgId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Should_Have_Error_When_UserIds_Is_Null()
    {
        var request = new PersonsDetailsRequestDto
        {
            UserIds = null,
            OrgId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.IsTrue(result.Errors.Any(e =>
            e.PropertyName == "UserIds" &&
            e.ErrorMessage == "UserIds list can not be null"));
    }

    [TestMethod]
    public void Should_Have_Error_When_UserIds_Is_Empty()
    {
        var request = new PersonsDetailsRequestDto
        {
            UserIds = new List<Guid>(),
            OrgId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        Assert.IsTrue(result.Errors.Any(e =>
            e.PropertyName == "UserIds" &&
            e.ErrorMessage == "UserIds list must contain at least one item."));
    }

    [TestMethod]
    public void Should_Have_Error_When_OrgId_Is_Empty_Guid()
    {
        var request = new PersonsDetailsRequestDto
        {
            UserIds = new List<Guid> { Guid.NewGuid() },
            OrgId = Guid.Empty
        };

        var result = _validator.Validate(request);

        Assert.IsTrue(result.Errors.Any(e =>
            e.PropertyName == "OrgId" &&
            e.ErrorMessage == "OrgId must be a valid non-empty Guid if provided."));
    }

    [TestMethod]
    public void Should_Not_Have_Error_When_OrgId_Is_Null()
    {
        var request = new PersonsDetailsRequestDto
        {
            UserIds = new List<Guid> { Guid.NewGuid() },
            OrgId = null
        };

        var result = _validator.Validate(request);

        Assert.IsTrue(result.IsValid);
    }
}

