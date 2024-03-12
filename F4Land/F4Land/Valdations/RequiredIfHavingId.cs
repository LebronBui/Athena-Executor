using RealEstateAuction.DataModel;
using System.ComponentModel.DataAnnotations;

public class RequiredIfHavingIdAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;

    public RequiredIfHavingIdAttribute(string dependentProperty)
    {
        _dependentProperty = dependentProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (PaymentDataModel)validationContext.ObjectInstance;

        var dependentPropertyValue = validationContext.ObjectType.GetProperty(_dependentProperty)?.GetValue(model);

        if ((int)dependentPropertyValue != null)
        {
            if (!string.IsNullOrEmpty(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}