using PropertyChanged;

namespace PropertyValidator.Test.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Address
    {
        public int PostalCode { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? CountryIsoCode { get; set; }
    }
}
