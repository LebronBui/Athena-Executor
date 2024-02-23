using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RealEstateAuction.DataModel
{
    public class UserDatalModel
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [RegularExpression(@"^[^\d]+$", ErrorMessage = "Tên không được chứa số")]
        public string FullName { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [CustomValidation(typeof(UserDatalModel), "ValidateDateOfBirth")]
        public DateTime Dob { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; }

        public string? Avatar { get; set; }

        public static ValidationResult ValidateDateOfBirth(DateTime dob, ValidationContext context)
        {
            if (dob > DateTime.Now)
            {
                return new ValidationResult("Ngày sinh không được lớn hơn ngày hiện tại");
            }

            return ValidationResult.Success;
        }
    }
}
