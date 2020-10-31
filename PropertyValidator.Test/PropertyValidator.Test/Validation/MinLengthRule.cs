using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyValidator.Test.Validation
{
    public class MinLengthRule : ValidationRule<string>
    {
        private readonly int minLength;

        public MinLengthRule(int minLength)
        {
            this.minLength = minLength;
        }

        public override string ErrorMessage => $"Characters must be at least {minLength} characters long";

        public override bool IsValid(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            return value.Length >= minLength;
        }
    }
}
