# Generating Tests

When generating tests, follow the following guidelines.

None of the guidelines are mandatory.
If a guideline does not make sense in the context of the current prompt, you don't have to follow it, but inform the user about the deviation and why you made the decision to not follow the guideline.

## Unit tests

For code in the BackendAccountService.Api project:

* unit tests should be created in the BackendAccountService.Api.UnitTests project
* copy the folder structure in the unit test project to match the structure of the BackendAccountService.Api project. E.g. if generating an unit test for /Controllers/XController, put the tests in /Controllers/XControllerTests
* use MSTest as the test framework
* if mocking is required, use Moq
* use fluentassertions for the assertions
* feel free to drive the test from data, such as by using the [DataRow] attribute in MSTest
* add comments to mark the Arrange, Act, and Assert sections of the test. If necessary you can combine sections, e.g. Act and Assert
* unit tests should test individual bits of functionality (single responsibility). if there is an issue with the code under test, the tests that fail should help pinpoint the exact issue. A single issue shouldn't cause all tests to fail because all tests also have a common assertion
* write minimally passing tests. The input for a unit test should be the simplest information needed to verify the behavior you're currently testing.
* in the assertions, don't use any data passed to the code under test for the expected values. E.g if the code under test accepts an object, don't use a property of the object directly as an expected value as the code under test could have mutated it. Prefer well-named constants (or literals) for expected values.
* avoid coding logic in unit tests, e.g. if a method concatenates two strings, don't use a string concatenation in the test to create the expected value. Instead, use a well-named constant or literal for the expected value.
* aim for 100% code coverage
* ensure the tests don't contain any warnings and won't get flagged for issues by Sonar Qube (or other common static code analysers)
* validate private (non public in general) methods by testing the public methods that call them. If a private method is complex enough to warrant its own test, consider refactoring it into a separate class with its own public method.
* if other similar tests use a common test base class, use that base class to avoid code duplication. If there is no such base class, consider creating one.
* feel free to use the latest c# features supported by the .NET version of the project (which is currently .net 8)
* name the test according to Microsoft's naming convention for unit tests:
 
The name of your test should consist of three parts:

Name of the method being tested
Scenario under which the method is being tested
Expected behavior when the scenario is invoked

Separate the three parts with underscores.

I.E.: `MethodName_WhenCondition_ThenExpectedBehavior`

There is no need to include the words When or Then

E.g.`Add_SingleNumber_ReturnsSameNumber()`

Further unit testing guidelines can be found here: 
https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

## Integration tests
