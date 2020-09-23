using PropertyValidator.Test.Models;
using Prism.Navigation;
using PropertyValidator.Services;
using PropertyValidator.Test.Validation;
using PropertyValidator.Models;

namespace PropertyValidator.Test.ViewModels
{
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
        public int PostalCode { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string CountryIsoCode { get; set; }

        public string FirstNameError { get; set; }
        public string LastNameError { get; set; }
        public string EmailAddressError { get; set; }
        public string PhysicalAddressError { get; set; }

        public void Initialize(INavigationParameters parameters)
        {
            validationService.For(this)
                .AddRule(e => e.FirstName, new RequiredRule())
                .AddRule(e => e.LastName, new LengthRule(20))
                .AddRule(e => e.EmailAddress, new EmailFormatRule())
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
}