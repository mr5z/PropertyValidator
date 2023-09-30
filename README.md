[![NuGet Version](https://img.shields.io/nuget/v/PropertyValidator.svg)](https://www.nuget.org/packages/PropertyValidator/)
[![NuGet Pre-release](https://img.shields.io/nuget/vpre/PropertyValidator.svg)](https://www.nuget.org/packages/PropertyValidator/)
[![GitHub Release](https://img.shields.io/github/release/mr5z/PropertyValidator.svg?style=flat)](https://github.com/mr5z/PropertyValidator/packages/385702)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PropertyValidator.svg)](https://www.nuget.org/packages/PropertyValidator/)

PropertyValidator is a versatile library designed to simplify property validation for classes that implement the `INotifyPropertyChanged` interface. It offers a straightforward way to define and apply validation rules to properties, making it easier to ensure the integrity of your data.

## Installation

You can install the PropertyValidator library via NuGet:

```shell
Install-Package PropertyValidator
```

## Example Usage
Here's an example of how to use PropertyValidator in your code:

```csharp
using PropertyValidator;

// ...

var validationService = new ValidationService();

// Register your model for validation
validationService.For(this, delay: TimeSpan.FromSeconds(0.7))
    .AddRule(e => e.FirstName, new RequiredRule())
    .AddRule(e => e.LastName, new LengthRule(50))
    .AddRule(e => e.EmailAddress, new RequiredRule(), new LengthRule(100), new EmailFormatRule());
```

## Features
PropertyValidator offers a range of features to streamline your validation process:

- Simple and intuitive API for defining validation rules.
- Supports both single-property rules and multi-property rules.
- Allows you to create custom and simple to create validation rules.
- Provides options to manually trigger validation or automatically ensure properties are valid.
- Offers event-based error handling through the `PropertyInvalid` event.
- Supports delayed validation to enhance user experience.

## ValidationPack - Common Validation Rules
PropertyValidator now includes the [ValidationPack](https://github.com/mr5z/PropertyValidator.ValidationPack), a set of common validation rules to cover popular input validation scenarios. The ValidationPack includes the following rules:
- StringRequiredRule: Ensures that a string property is not empty or null.
- MaxLengthRule: Validates that a string does not exceed a specified maximum length.
- MinLengthRule: Checks that a string meets a specified minimum length.
- RangeLengthRule: Validates that a string falls within a specific length range.
- EmailFormatRule: Ensures that a string follows a valid email format.

### Customizable Error Messages
The error messages for these validation rules are provided through the ErrorMessages.resx file in the ValidationPack. However, you can easily customize these error messages to suit your application's needs.

To replace the default error messages with your custom messages, follow these steps:
1. Create your own .resx file with custom error messages.
2. Use the ErrorMessageHelper.UpdateResource<T>() method, where T is the name of your custom .resx file. For example:
```csharp
using PropertyValidator.ValidationPack;

// Replace "YourCustomErrorMessages" with the name of your custom .resx file
ErrorMessageHelper.UpdateResource<YourCustomErrorMessages>();
```

## Creating Custom Validation Rules
PropertyValidator allows you to create custom validation rules to suit your specific validation requirements. To create a custom validation rule, follow these steps:

1. Create a new class that inherits from `ValidationRule<T>`, where `T` is the type of the property you want to validate.

2. Implement the `IsValid(T value)` method in your custom rule class. This method should return `true` if the value is valid according to your rule, and `false` otherwise.

3. Override the `ErrorMessage` property to provide a custom error message that will be displayed when the validation fails.

Here's an example of creating a custom validation rule to check if a string contains only alphanumeric characters:

```csharp
using PropertyValidator;

public class AlphanumericRule : ValidationRule<string>
{
    public override string ErrorMessage => "Only alphanumeric characters are allowed.";

    public override bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        // Use regular expression to check if the string contains only alphanumeric characters
        const string pattern = "^[a-zA-Z0-9]*$";
        return Regex.IsMatch(value, pattern);
    }
}
```
Now, you can use your custom validation rule in your validation service. Here's an example of how to use the `AlphanumericRule`:

```csharp
var validationService = new ValidationService();

// Register your model for validation and include the custom rule
validationService.For(this)
    .AddRule(e => e.Username, new AlphanumericRule());
```
By following these steps, you can create and use custom validation rules tailored to your specific validation needs in your PropertyValidator library.

## Getting Started
1. Install the PropertyValidator library via NuGet.
2. Create validation rule models by extending ValidationRule<T> or MultiValidationRule<T>, where T is the property type.
3. Implement the library in your classes that implement INotifyPropertyChanged.
4. Register your model for validation using the provided API.
5. Optionally, handle validation errors using the PropertyInvalid event or manually check for errors.

## Support
Feel free to contribute to the project, report issues, or provide feedback to help us improve `PropertyValidator`.

PropertyValidator is a versatile library that simplifies property validation for classes implementing `INotifyPropertyChanged`. It empowers you to ensure data integrity in your applications effortlessly. If you have any questions or suggestions, please don't hesitate to reach out and contribute to the project.
