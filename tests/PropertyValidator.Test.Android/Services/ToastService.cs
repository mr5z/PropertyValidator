using Android.Content;
using Android.Widget;
using PropertyValidator.Test.Services;

namespace PropertyValidator.Test.Droid.Services
{
    class ToastService : IToastService
    {
        private readonly Context context;
        private Toast toast;

        public ToastService(Context context)
        {
            this.context = context;
        }

        public void ShowMessage(string message, params string[] args)
        {
            toast?.Cancel();
            toast = Toast.MakeText(context, string.Format(message, args), ToastLength.Long);
            toast.Show();
        }
    }
}