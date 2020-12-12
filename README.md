## A simple library to help you validate properties of classes that implements `INotifyPropertyChanged`.

### Installation

 * Available on NuGet: [PropertyValidator](http://www.nuget.org/packages/PropertyValidator) [![NuGet](https://img.shields.io/nuget/v/PropertyValidator.svg?label=NuGet)](https://www.nuget.org/packages/PropertyValidator)
 * Available on GitHub: [PropertyValidator](https://github.com/mr5z/PropertyValidator/packages/385702) [![GitHub Release](https://img.shields.io/github/release/mr5z/PropertyValidator.svg?style=flat)]()
 
### Result
![Xamarin.Android](https://i.imgur.com/rVw3k6T.gif)

XAML of this example: [ItemsPage.xaml](https://github.com/mr5z/PropertyValidator/blob/master/PropertyValidator.Test/PropertyValidator.Test/Pages/ItemsPage.xaml)

## Example usage
```c#
validationService.For(this)
    .AddRule(e => e.FirstName, new RequiredRule())
    .AddRule(e => e.LastName, new LengthRule(50))
    .AddRule(e => e.EmailAddress, new RequiredRule(), new LengthRule(100), new EmailFormatRule());
```

### Service interface

The interface is pretty simple and self-documenting:

``` c#
//  (yet with comments)
public interface IValidationService
{
    // For registration
    RuleCollection<TNotifiableModel> For<TNotifiableModel>(TNotifiableModel notifiableModel)
        where TNotifiableModel : INotifyPropertyChanged;

    // Retrieve error messages per property
    string GetErrorMessage<TNotifiableModel>(
        TNotifiableModel notifiableModel,
        Expression<Func<TNotifiableModel, object>> expression)
        where TNotifiableModel : INotifyPropertyChanged;

    // Manually trigger the validation
    bool Validate();

    // Subscribe to error events (cleared/raised)
    event EventHandler<ValidationResultArgs> PropertyInvalid;
}
```

### Setup

1. Create the validation rule models by extending the `ValidationRule<T>` or `MultiValidationRule<T>`, where `T` is the type of the target property.

``` c#
// For email address
public class EmailFormatRule : ValidationRule<string>
{
    public override string ErrorMessage => "Not a valid email format";

    public override bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        return regex.IsMatch(value);
    }
}

// For required field
public class RequiredRule : ValidationRule<string>
{
    public override string ErrorMessage => "Izz required!";

    public override bool IsValid(string value)
    {
        return !string.IsNullOrEmpty(value);
    }
}

// If you want to limit the string to a certain length
public class LengthRule : ValidationRule<string>
{
    public override string ErrorMessage => string.Format(Strings.MaxCharacters, max);

    private readonly int max;

    public LengthRule(int max)
    {
        this.max = max;
    }

    public override bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return value.Length < max;
    }
}

// A multi-property validation model. You can also reuse other ValidationRules here!
public class AddressRule : MultiValidationRule<Address>
{
    protected override RuleCollection<Address> ConfigureRules(RuleCollection<Address> ruleCollection)
    {
        return ruleCollection
            .AddRule(e => e.City, new RequiredRule())
            .AddRule(e => e.CountryIsoCode, new CountryIsoCodeRule())
            .AddRule(e => e.PostalCode, new PostalCodeRule())
            .AddRule(e => e.StreetAddress, new RequiredRule(), new LengthRule(100));
    }
}
```


2. Use the validation rules in our classes that implements (implicitly from the base class) `INotifyPropertyChanged`.
The example below is implemented in Xamarin Forms together with [Prism](https://github.com/PrismLibrary/Prism) library to register the service in the [DI](https://stackoverflow.com/q/130794/2304737) container, and [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged) for automatic INPC generation of getters/setters.
Take note that this library is not limited to Xamarin only, it's available to all platforms supported by .NET family.

``` c#
public class ItemsPageViewModel : BaseViewModel, IInitialize
{
    private readonly IValidationService validationService;

    public ItemsPageViewModel(INavigationService navigationService, IValidationService validationService) : base(navigationService)
    {
        this.validationService = validationService;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public Address PhysicalAddress { get; set; } = new Address();

    public string FirstNameError { get; set; }
    public string LastNameError { get; set; }
    public string EmailAddressError { get; set; }
    public string PhysicalAddressError { get; set; }

    // You must do this only once in the initialization part of your class model.
    public void Initialize(INavigationParameters parameters)
    {
        validationService.For(this)
            .AddRule(e => e.FirstName, new RequiredRule())
            .AddRule(e => e.LastName, new LengthRule(50))
            .AddRule(e => e.EmailAddress, new RequiredRule(), new LengthRule(100), new EmailFormatRule())
            // The error message have been overriden to "Deez nuts" since an aggregated error messages is awful.
            .AddRule(e => e.PhysicalAddress, "Deez nuts", new AddressRule()); 

        validationService.PropertyInvalid += ValidationService_PropertyInvalid;
    }

    private void ValidationService_PropertyInvalid(object sender, ValidationResultArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(FirstName):
                FirstNameError = e.FirstError;
                break;
            case nameof(LastName):
                LastNameError = e.FirstError;
                break;
            case nameof(EmailAddress):
                EmailAddressError = e.FirstError;
                break;
            case nameof(PhysicalAddress):
                PhysicalAddressError = e.FirstError;
                break;
        }
        
        // To retrieve all the error message of the property, use:
        var errorMessages = e.ErrorMessages;
        
        // If you have a bunch of error properties, skip the tall switch-case and be more productive by using this:
        e.FillErrorProperty(this);
        // This will basically auto-fill the error properties you have in the target instance but,
        // you must follow this convention: "<PropertyName>" + "Error"
    }
}
```

3. If you wish not to use `PropertyInvalid` event to check every time the property have changed, you can also invoke manually the `ValidationService#Validate()`, check the return, if it's false, find the error message using `ValidationService#GetErrorMessage(...)`

``` c#
private void ShowValidationResult()
{
    FirstNameError = validationService.GetErrorMessage(this, e => e.FirstName);
    LastNameError = validationService.GetErrorMessage(this, e => e.LastName);
    EmailAddressError = validationService.GetErrorMessage(this, e => e.EmailAddress);
    PhysicalAddressError = validationService.GetErrorMessage(this, e => e.PhysicalAddress);
}

private void Register()
{
    if (!validationService.Validate())
    {
        ShowValidationResult();
        return;
    }

    ...
}	
```

### Autofill
When autofill is enabled, each property you registered in the `.AddRule(...)` chain must have a backing error property and it must also follow the naming convention
```c#
public string <PropertyName>Error { get; set; }
```
Thus having a property `FirstName` must have a corresponding error property of `FirstNameError`.
Once this is enabled, subscribing to the `PropertyInvalid` event becomes optional.
```c#
validationService.For(this, autofill: true)
    .AddRule(e => e.FirstName, new RequiredRule())
    .AddRule(e => e.LastName, new LengthRule(50))
    .AddRule(e => e.EmailAddress, new RequiredRule(), new LengthRule(100), new EmailFormatRule())
    .AddRule(e => e.PhysicalAddress, "Deez nuts", new AddressRule()); 
    
// We don't need to subscribe to the event anymore
// validationService.PropertyInvalid += ValidationService_PropertyInvalid;
```

### Delay
![Impatient UI](https://i.redd.it/emd3wuhfty361.png)

Don't be this guy. To solve this, use **delay**!
```c#
validationService.For(this, delay: TimeSpan.FromSeconds(0.7))
    .AddRule(e => e.FirstName, new RequiredRule())
    .AddRule(e => e.LastName, new LengthRule(50))
    .AddRule(e => e.EmailAddress, new RequiredRule(), new LengthRule(100), new EmailFormatRule())
    .AddRule(e => e.PhysicalAddress, "Deez nuts", new AddressRule()); 
```

## Support

Feel free to contribute if you find some issues or you have more ideas to add :)

ETH: 0x11bafdeCfb4Aa03D029ef10c9cE8DCB41B83aFb1
