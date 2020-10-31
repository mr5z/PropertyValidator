using PropertyValidator.Test.Models;
using Prism.Navigation;
using PropertyValidator.Services;
using PropertyValidator.Test.Validation;
using PropertyValidator.Models;
using PropertyValidator.Test.Helpers;
using Prism.Commands;
using PropertyValidator.Test.Services;

namespace PropertyValidator.Test.ViewModels
{
    public class ItemsPageViewModel : BaseViewModel, IInitialize
    {
        private readonly IValidationService validationService;
        private readonly IToastService toastService;

        public ItemsPageViewModel(
            INavigationService navigationService,
            IValidationService validationService,
            IToastService toastService) : base(navigationService)
        {
            this.validationService = validationService;
            this.toastService = toastService;

            SubmitCommand = new DelegateCommand(Submit);
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public Address PhysicalAddress { get; set; } = new Address();
        public int PostalCode { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string CountryIsoCode { get; set; }
        public DelegateCommand SubmitCommand { get; set; }

        public string FirstNameError { get; set; }
        public string LastNameError { get; set; }
        public string EmailAddressError { get; set; }
        public string PhysicalAddressError { get; set; }

        public void Initialize(INavigationParameters parameters)
        {
            validationService.For(this)
                .AddRule(e => e.FirstName, new RequiredRule(), new MinLengthRule(2))
                .AddRule(e => e.LastName, new MaxLengthRule(5))
                .AddRule(e => e.EmailAddress, new EmailFormatRule())
                .AddRule(e => e.PhysicalAddress, "Deez nuts!", new AddressRule());

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

            e.FillErrorProperty(this);

            // To retrieve all the error message of the property, use:
            var errorMessages = e.ErrorMessages;

            Debug.Log("error key: {0}, value: {1}", e.PropertyName, e.FirstError);
        }

        private void ShowValidationResult()
        {
            FirstNameError = validationService.GetErrorMessage(this, e => e.FirstName);
            LastNameError = validationService.GetErrorMessage(this, e => e.LastName);
            EmailAddressError = validationService.GetErrorMessage(this, e => e.EmailAddress);
            PhysicalAddressError = validationService.GetErrorMessage(this, e => e.PhysicalAddress);
        }

        private void Submit()
        {
            if (!validationService.Validate())
            {
                ShowValidationResult();
                var errors = $@"
FirstName: {FirstNameError}
LastName: {LastNameError}
EmailAddress: {EmailAddressError}
PhysicalAddress: {PhysicalAddressError}
";
                toastService.ShowMessage("Errors: {0}", errors);
            }
        }
    }
}