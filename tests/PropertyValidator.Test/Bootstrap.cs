using Prism.Ioc;
using PropertyValidator.Services;
using PropertyValidator.Test.Pages;
using PropertyValidator.Test.ViewModels;
using PropertyValidator.ValidationPack;
using System;
using System.Threading.Tasks;

namespace PropertyValidator.Test
{
    public static class Bootstrap
    {
        public static async Task Initialize(IContainerRegistry registry, Action? preconfigure = null)
        {
            RegisterPages(registry);
            RegisterServices(registry);
            RegisterMappings();
            preconfigure?.Invoke();
            await ConfigureServices();
            SetupAnalytics(registry);
        }

        private static void RegisterPages(IContainerRegistry registry)
        {
            registry.RegisterForNavigation<MainPage, MainPageViewModel>();
            registry.RegisterForNavigation<AboutPage, AboutPageViewModel>();
            registry.RegisterForNavigation<ItemsPage, ItemsPageViewModel>();
        }

        private static void RegisterServices(IContainerRegistry registry)
        {
            registry.Register<IValidationService, ValidationService>();
        }

        private static void RegisterMappings()
        {

        }

        private static Task ConfigureServices()
        {
            ErrorMessageHelper.UpdateResource<ErrorMessages>();
            return Task.CompletedTask;
        }

        private static void SetupAnalytics(IContainerRegistry registry)
        {

        }
    }
}
