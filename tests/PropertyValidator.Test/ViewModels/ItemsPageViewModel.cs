using Prism.Navigation;
using PropertyValidator.Exceptions;
using PropertyValidator.Models;
using PropertyValidator.Services;
using PropertyValidator.Test.Helpers;
using PropertyValidator.Test.Models;
using PropertyValidator.Test.Services;
using PropertyValidator.Test.Validation;
using PropertyValidator.ValidationPack;
using System;
using System.Collections.Generic;
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
        public ICommand SubmitCommand { get; set; }

        public IDictionary<string, string?> Errors => validationService.GetErrors();

        public void Initialize(INavigationParameters parameters)
        {
            validationService.For(this,
                delay: TimeSpan.FromSeconds(0.7))
                .AddRule(e => e.FirstName, new StringRequiredRule(), new MinLengthRule(2))
                .AddRule(e => e.LastName, new StringRequiredRule(), new MaxLengthRule(5))
                .AddRule(e => e.EmailAddress, new StringRequiredRule(), new EmailFormatRule(), new RangeLengthRule(10, 15))
                .AddRule(e => e.PhysicalAddress, new AddressRule());
        }

        public void NotifyErrorPropertyChanged() => RaisePropertyChanged(nameof(Errors));

        private void Submit()
        {
            try
            {
                Debug.Log("LastName: {0}", LastName);
                validationService.EnsurePropertiesAreValid();
            }
            catch (PropertyException ex)
            {
                var resultArgs = ex.ValidationResultArgs;
                var errors = string.Join(", ", resultArgs.ErrorMessages);
                toastService.ShowMessage("Errors: {0}", errors);
            }
        }
    }
}