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

### Prepare INPC class

```csharp
using PropertyValidator;

class ViewModel : INotifyPropertyChanged, INotifiableModel
{
    // We are just converting the function to readonly property. This will be accessed by XAML later.
    public IDictionary<string, string?> Errors => validationService.GetErrors();

    // Implement INotifiableModel so it propagates changes to XAML
    public void NotifyErrorPropertyChanged() => RaisePropertyChanged(nameof(Errors));

    // I have only included this here for clarity. Substitute with your own implentation.
    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Setup INPC class in its initialization code
```csharp
var validationService = new ValidationService();

// Register your INPC instance for validation
validationService.For(this, delay: TimeSpan.FromSeconds(0.7))
    .AddRule(e => e.FirstName, new StringRequiredRule(), new MinLengthRule(2))
    .AddRule(e => e.LastName, new StringRequiredRule(), new MaxLengthRule(5))
    .AddRule(e => e.EmailAddress, new StringRequiredRule(), new EmailFormatRule(), new RangeLengthRule(10, 15))
```

### Consume in XAML
```xaml
<Entry
    Text="{Binding EmailAddress}"
    Keyboard="Email"
    Placeholder="Email address" />
<Label
    Text="{Binding Errors[EmailAddress]}"
    TextColor="Red"
    FontSize="Small" />
...
```

### Result
![output](https://github.com/mr5z/PropertyValidator/assets/6318395/410f7c92-e76e-4a80-b309-d0dd0bc1afbd)

### MAUI complete example
[Test.Maui](https://github.com/mr5z/Test.Maui)

## Features
PropertyValidator offers a range of features to streamline your validation process:

- Simple and intuitive API for defining validation rules.
- Supports both single-property rules and multi-property rules.
- Allows you to create custom and simple to create validation rules.
- Provides options to manually trigger validation or automatically ensure properties are valid.
- Offers event-based error handling through the `PropertyInvalid` event.
- Supports delayed validation to enhance user experience.

## ValidationPack - Common Validation Rules
[ValidationPack](https://github.com/mr5z/PropertyValidator.ValidationPack) contains a set of common validation rules to cover popular input validation scenarios. The ValidationPack includes the following rules:
- StringRequiredRule: Ensures that a string property is not empty or null.
- MaxLengthRule: Validates that a string does not exceed a specified maximum length.
- MinLengthRule: Checks that a string meets a specified minimum length.
- RangeLengthRule: Validates that a string falls within a specific length range.
- EmailFormatRule: Ensures that a string follows a valid email format.

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
By following these steps, you can create and use custom validation rules tailored to your specific validation needs in your project.

## Support
Feel free to contribute to the project, report issues, or provide feedback to help us improve `PropertyValidator`.
