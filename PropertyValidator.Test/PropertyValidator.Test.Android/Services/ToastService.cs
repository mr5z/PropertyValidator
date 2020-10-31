using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
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
            toast = Toast.MakeText(context, string.Format(message, args), ToastLength.Short);
            toast.Show();
        }
    }
}