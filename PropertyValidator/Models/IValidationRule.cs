﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyValidator.Models
{
    public interface IValidationRule
    {
        string PropertyName { get; set; }
        bool HasError { get; }
        bool Validate(object value);
        string ErrorMessage { get; }
    }
}
