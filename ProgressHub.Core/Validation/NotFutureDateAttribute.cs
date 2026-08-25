using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Validation
{
    public class NotFutureDateAttribute : ValidationAttribute
    {
        public NotFutureDateAttribute()
        {
            ErrorMessage = "Date cannot be in the future.";
        }

        public override bool IsValid(object? value)
        {
            if (value is not DateOnly date)
            {
                return true; // let [Required] handle missing/wrong-type values
            }

            return date <= DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
