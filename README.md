## A simple library to help you validate properties of classes that implements `INotifyPropertyChanged`.

### Setup

 * Available on NuGet: [PropertyValidator](http://www.nuget.org/packages/PropertyValidator) [![NuGet](https://img.shields.io/nuget/v/PropertyValidator.svg?label=NuGet)](https://www.nuget.org/packages/PropertyValidator)
 * Available on GitHub: [PropertyValidator](https://github.com/mr5z/PropertyValidator/packages/385702?version=1.0.3) [![GitHub Release](https://img.shields.io/github/release/mr5z/PropertyValidator.svg?style=flat)]() 

### Service interface

The interface is pretty simple and self-documenting:

``` c#
public interface IValidationService
{
    RuleCollection<TNotifiableModel> For<TNotifiableModel>(TNotifiableModel notifiableModel) where TNotifiableModel : INotifyPropertyChanged;
    string GetErrorMessage<TNotifiableModel>(TNotifiableModel notifiableModel, Expression<Func<TNotifiableModel, object>> expression) where TNotifiableModel : INotifyPropertyChanged;
    bool Validate();
    event EventHandler<ValidationResultArgs> PropertyInvalid;
}
```

### Usage:

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
The example below is used in Xamarin Forms along with the Prism library to register the service in the Dependency Injection library, but it can be used also in other platforms supported by the .NET family.

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
    }
}
```

3. If you wish not to use `PropertyInvalid` event to check every time the property have changed, you can also invoke manually the `IValidationService.Validate()`, check the return, if it's false, find the error message using `GetErrorMessage(...)`

``` c#
private void ShowValidationResult()
{
    ErrorFirstName = validationService.GetErrorMessage(this, e => e.FirstName);
    ErrorLastName = validationService.GetErrorMessage(this, e => e.LastName);
    ErrorEmailAddress = validationService.GetErrorMessage(this, e => e.EmailAddress);
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

### Result
![Xamarin.Android](https://i.imgur.com/SjSeUst.gif)

## Support

Feel free to contribute if you find some issues or you have more ideas to add :)
ETH: 0x11bafdeCfb4Aa03D029ef10c9cE8DCB41B83aFb1
