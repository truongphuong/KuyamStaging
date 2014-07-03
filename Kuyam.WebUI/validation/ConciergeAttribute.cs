using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kuyam.WebUI.validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ConciergeAttribute : ValidationAttribute, IClientValidatable
    {
        string dependentProperty;       

        public ConciergeAttribute(string dependentProperty, string errorMessage)
            : base(errorMessage)
        {
            this.dependentProperty = dependentProperty;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var role = GetValue<string>(validationContext.ObjectInstance, dependentProperty);
            if (role == "Concierge" && value == null)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }

        private static T GetValue<T>(object objectInstance, string propertyName)
        {
            if (objectInstance == null) throw new ArgumentNullException("objectInstance");
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException("propertyName");

            var propertyInfo = objectInstance.GetType().GetProperty(propertyName);

            return (T)propertyInfo.GetValue(objectInstance);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var modelClientValidationRule = new ModelClientValidationRule
            {
                ValidationType = "concierrole",
                ErrorMessage = FormatErrorMessage(metadata.DisplayName)
            };

            modelClientValidationRule.ValidationParameters.Add("dependentproperty", dependentProperty);
            yield return modelClientValidationRule;

        }

    }
}