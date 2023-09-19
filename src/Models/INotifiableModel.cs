using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyValidator.Models
{
    public interface INotifiableModel
    {
        void NotifyPropertyChanged();
    }
}
