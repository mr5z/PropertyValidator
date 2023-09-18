using Prism.Commands;
using Prism.Navigation;
using PropertyValidator.Exceptions;
using PropertyValidator.Models;
using PropertyValidator.Services;
using PropertyValidator.Test.Helpers;
using PropertyValidator.Test.Models;
using PropertyValidator.Test.Services;
using PropertyValidator.Test.Validation;
using System;
using System.Windows.Input;
using XamarinUtility.Commands;

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

            SubmitCommand = new AdaptiveCommand(Submit);
        }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public Address PhysicalAddress { get; set; } = new Address();
        public int PostalCode { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? CountryIsoCode { get; set; }
        public ICommand SubmitCommand { get; set; }

        public string? FirstNameError { get; set; }
        public string? LastNameError { get; set; }
        public string? EmailAddressError { get; set; }
        public string? PhysicalAddressError { get; set; }

        public void Initialize(INavigationParameters parameters)
        {
            validationService.For(this,
                autofill: true,
                delay: TimeSpan.FromSeconds(0.7))
                .AddRule(e => e.FirstName, new RequiredRule(), new MinLengthRule(2))
                .AddRule(e => e.LastName, new RequiredRule(), new MaxLengthRule(5))
                .AddRule(e => e.EmailAddress, new RequiredRule(), new EmailFormatRule())
                .AddRule(e => e.PhysicalAddress, "Deez nuts!", new AddressRule());

            //validationService.PropertyInvalid += ValidationService_PropertyInvalid;
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

        private void Submit()
        {
            try
            {
                Debug.Log("LastName: {0}", LastName);
                validationService.EnsurePropertiesAreValid();
            }
            catch (PropertyException ex)
            {
                var msg = ex.Message;
                var args = ex.ValidationResultArgs;
                toastService.ShowMessage(msg);
                args.FillErrorProperty(this);
            }

            if (!validationService.Validate())
            {
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