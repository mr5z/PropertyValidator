using PropertyValidator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyValidator.Test.Validation
{
    public class MaxLengthRule : ValidationRule<string>
    {
        private readonly int maxLength;

        public MaxLengthRule(int maxLength)
        {
            this.maxLength = maxLength;
        }

        public override string ErrorMessage => $"Characters must not exceed to {maxLength}";

        public override bool IsValid(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            return value.Length <= maxLength;
        }
    }
}
