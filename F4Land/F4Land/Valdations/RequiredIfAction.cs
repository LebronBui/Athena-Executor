using RealEstateAuction.DataModel;
using RealEstateAuction.Enums;
using System;
using System.ComponentModel.DataAnnotations;


namespace RealEstateAuction.Valdations
{
    public class RequiredIfActionAttribute : ValidationAttribute
    {
        private readonly PaymentType _requiredAction;

        public RequiredIfActionAttribute(PaymentType requiredAction)
        {
            _requiredAction = requiredAction;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (PaymentDataModel)validationContext.ObjectInstance;

            if (model.Action == PaymentType.TopUp && value == null)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}


