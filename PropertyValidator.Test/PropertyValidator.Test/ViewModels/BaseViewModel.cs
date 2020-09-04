using Prism.Mvvm;
using Prism.Navigation;

namespace PropertyValidator.Test.ViewModels
{
    public class BaseViewModel : BindableBase
    {
        public BaseViewModel(INavigationService navigationService)
        {

        }

        public string Title { get; set; }
    }
}
