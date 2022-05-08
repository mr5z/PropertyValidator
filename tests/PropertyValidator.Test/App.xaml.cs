using Prism;
using Prism.Ioc;
using PropertyValidator.Test.Extensions;
using System.Threading.Tasks;

namespace PropertyValidator.Test
{
    public partial class App
    {
        public App() : this(null) { }

        public App(IPlatformInitializer? initializer) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();
            GoToLandingPage().FireAndForget();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Bootstrap
                .Initialize(containerRegistry)
                .FireAndForget();
        }

        private Task GoToLandingPage()
        {
            return NavigationService.NavigateAsync("MainPage");
        }
    }
}
