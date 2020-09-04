using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyValidator.Test.Validation
{
    public class RequiredRule : ValidationRule<string>
    {
        public override string ErrorMessage => "Izz required!";

        public override bool IsValid(string value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}
