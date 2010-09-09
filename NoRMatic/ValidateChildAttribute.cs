using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NoRMatic {

    public class ValidateChildAttribute : ValidationAttribute {

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
