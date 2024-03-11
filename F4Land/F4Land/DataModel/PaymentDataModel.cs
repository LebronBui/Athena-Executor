using RealEstateAuction.Enums;
using RealEstateAuction.Valdations;
using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class PaymentDataModel
    {
        [Required(ErrorMessage = "Vui lòng nhập dữ liệu")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0.")]
        public int Amount { get; set; }
        [RequiredIfAction(PaymentType.TopUp, ErrorMessage = "Tài khoản nhận là bắt buộc")]
        public int BankId { get; set; }
        [Required(ErrorMessage = "Tài khoản giao dịch là bắt buộc")]
        public string UserAccountNumber { get; set; }

        [Required(ErrorMessage = "Ngân hàng là bắt buộc")]
        public string UserBankName { get; set; }

        [Required]
        public PaymentType Action { get; set; }
    }
}
