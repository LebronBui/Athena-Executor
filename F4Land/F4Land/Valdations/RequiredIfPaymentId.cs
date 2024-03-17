using RealEstateAuction.DataModel;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RequiredIfPaymentIdAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (PaymentDataModel)validationContext.ObjectInstance;

        // Kiểm tra xem paymentId có giá trị hay không
        if (model.PaymentId == null)
        {
            // Nếu paymentId có giá trị, kiểm tra xem value của thuộc tính có null hoặc trống không
            if (value == null || (value is string && string.IsNullOrWhiteSpace((string)value)))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        // Trường hợp paymentId không có giá trị, validation được coi là hợp lệ
        return ValidationResult.Success;
    }
}


