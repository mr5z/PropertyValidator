using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

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

        public string FirstNameError { get; set; }
        public string LastNameError { get; set; }
        public string EmailAddressError { get; set; }

        public void Initialize(INavigationParameters parameters)
        {
            validationService.For(this)
                .AddRule(e => e.FirstName, new RequiredRule())
                .AddRule(e => e.EmailAddress, new EmailFormatRule());

            validationService.PropertyInvalid += ValidationService_PropertyInvalid;
        }

        private void ValidationService_PropertyInvalid(object sender, ValidationResultArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FirstName):
                    FirstNameError = e.FirstError;
                    break;
                case nameof(EmailAddress):
                    EmailAddressError = e.FirstError;
                    break;
            }
        }
    }
}