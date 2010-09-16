using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NoRMatic {

    /// <summary>
    /// When applied, this attribute will cause deep validation of complex child types.  The attribute itself does
    /// not add any validation rules, but it causes the validator to inspect the regular System.ComponentModel.DataAnnotations
    /// attributes applied to child types.  The attribute can also be applied to IEnumerable properties of complex types.
    /// </summary>
    public class ValidateChildAttribute : ValidationAttribute {

        /// <summary>
        /// Determines if the type is valid based on the System.ComponentModel.DataAnnotations attributes applied
        /// to the child types properties.
        /// </summary>
        public override bool IsValid(object value) {
            if (value == null) return true;
            var isValid = IsItValid(value);
            if (!isValid) return false;
            if (value is IEnumerable) return ((IEnumerable)value).Cast<object>().All(IsItValid);
            return true;
        }

        private static bool IsItValid(object value) {
            return Validator.TryValidateObject(value, new ValidationContext(value, null, null),
                new List<ValidationResult>(), true);
        }
    }
}
