using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using PropertyValidator.Test.Droid.Services;
using PropertyValidator.Test.Services;
using Prism.Ioc;
using Prism;

namespace PropertyValidator.Test.Droid
{
    [Activity(
        Label = "PropertyValidator.Test",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.Keyboard)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(new AndroidInitializer(this)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public class AndroidInitializer : IPlatformInitializer
        {
            private readonly Activity activity;

            public AndroidInitializer(Activity activity)
            {
                this.activity = activity;
            }

            public void RegisterTypes(IContainerRegistry registry)
            {
                //Register services with platform-specific implementations here.
                registry.Register<IToastService>(() => new ToastService(activity));
            }
        }
    }
}