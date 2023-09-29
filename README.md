[![NuGet Version](https://img.shields.io/nuget/v/PropertyValidator.svg)](https://www.nuget.org/packages/PropertyValidator/)
[![NuGet Pre-release](https://img.shields.io/nuget/vpre/PropertyValidator.svg)](https://www.nuget.org/packages/PropertyValidator/)
[![GitHub Release](https://img.shields.io/github/release/mr5z/PropertyValidator.svg?style=flat)](https://github.com/mr5z/PropertyValidator/packages/385702)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PropertyValidator.svg)](https://www.nuget.org/packages/PropertyValidator/)

`PropertyValidator` is a versatile library designed to simplify property validation for classes that implement the `INotifyPropertyChanged` interface. It offers a straightforward way to define and apply validation rules to properties, making it easier to ensure the integrity of your data.

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
`PropertyValidator` offers a range of features to streamline your validation process:

- Simple and intuitive API for defining validation rules.
- Supports both single-property rules and multi-property rules.
- Allows you to create custom and simple to create validation rules.
- Provides options to manually trigger validation or automatically ensure properties are valid.
- Offers event-based error handling through the `PropertyInvalid` event.
- Supports delayed validation to enhance user experience.

## Getting Started
1. Install the PropertyValidator library via NuGet.
2. Create validation rule models by extending ValidationRule<T> or MultiValidationRule<T>, where T is the property type.
3. Implement the library in your classes that implement INotifyPropertyChanged.
4. Register your model for validation using the provided API.
5. Optionally, handle validation errors using the PropertyInvalid event or manually check for errors.

## Support
Feel free to contribute to the project, report issues, or provide feedback to help us improve `PropertyValidator`.

`PropertyValidator` is a versatile library that simplifies property validation for classes implementing `INotifyPropertyChanged`. It empowers you to ensure data integrity in your applications effortlessly. If you have any questions or suggestions, please don't hesitate to reach out and contribute to the project.
