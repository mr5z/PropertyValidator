using Prism.Navigation;
using PropertyValidator.Exceptions;
using PropertyValidator.Helpers;
using PropertyValidator.Models;
using PropertyValidator.Services;
using PropertyValidator.Test.Helpers;
using PropertyValidator.Test.Models;
using PropertyValidator.Test.Services;
using PropertyValidator.Test.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using XamarinUtility.Commands;

namespace PropertyValidator.Test.ViewModels
{
    public class ItemsPageViewModel : BaseViewModel, IInitialize, INotifiableModel
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

        public IDictionary<string, string?> Errors => validationService.GetErrors();

        public void Initialize(INavigationParameters parameters)
        {
            validationService.For(this,
                delay: TimeSpan.FromSeconds(0.7))
                .AddRule(e => e.FirstName, new RequiredRule(), new MinLengthRule(2))
                .AddRule(e => e.LastName, new RequiredRule(), new MaxLengthRule(5))
                .AddRule(e => e.EmailAddress, new RequiredRule(), new EmailFormatRule())
                .AddRule(e => e.PhysicalAddress, "Deez nuts!", new AddressRule());

            //validationService.PropertyInvalid += ValidationService_PropertyInvalid;
        }

        public void NotifyPropertyChanged() => RaisePropertyChanged(nameof(Errors));

        private void ValidationService_PropertyInvalid(object sender, ValidationResultArgs e)
        {
            Debug.Log("error key: {0}, value: {1}", e.PropertyName, e.FirstError);
        }

        private void Submit()
        {
            try
            {
                Debug.Log("LastName: {0}", LastName);
                validationService.EnsurePropertiesAreValid();
            }
            catch (PropertyException)
            {
            }

            if (!validationService.Validate())
            {
                var errors = string.Join(", ", Errors.Select(e => e.Value));
                toastService.ShowMessage("Errors: {0}", errors);
            }
        }
    }
}