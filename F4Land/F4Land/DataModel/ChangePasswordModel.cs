using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới!")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự!")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới!")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp!")]
        public string ReNewPassword { get; set; }
    }
}
