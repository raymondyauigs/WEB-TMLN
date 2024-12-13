using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HYDtmn.Framework.Tools
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredForAnyAttribute : ValidationAttribute
    {
        /// <summary>
        /// Values of the <see cref="PropertyName"/> that will trigger the validation
        /// </summary>
        public object[] Values { get; set; }

        /// <summary>
        /// Independent property name
        /// </summary>
        public string PropertyName { get; set; }

        public bool NotNull { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;
            if (model == null || (!NotNull && Values == null))
            {
                return ValidationResult.Success;
            }

            var currentValue = model.GetType().GetProperty(PropertyName)?.GetValue(model, null);
            if(NotNull && currentValue!=null && value==null)
            {
                var propertyInfo = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                return new ValidationResult($"{propertyInfo.Name} is required for the current {PropertyName} value is not null");

            }


            if (Values!=null && currentValue!=null && Values.Contains(currentValue) && value == null)
            {
                var propertyInfo = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                return new ValidationResult($"{propertyInfo.Name} is required for the current {PropertyName} value {currentValue}");
            }
            return ValidationResult.Success;
        }
    }
}