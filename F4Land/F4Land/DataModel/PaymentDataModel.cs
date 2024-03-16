using RealEstateAuction.Enums;
using RealEstateAuction.Valdations;
using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class PaymentDataModel
    {
        [RequiredIfPaymentId(ErrorMessage = "Vui lòng nhập dữ liệu")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0.")]
        public decimal Amount { get; set; }

        [RequiredIfPaymentId(ErrorMessage = "Tài khoản nhận là bắt buộc")]
        public int BankId { get; set; }

        [RequiredIfPaymentId(ErrorMessage = "Tài khoản giao dịch là bắt buộc")]
        public string? UserAccountNumber { get; set; }

        [RequiredIfPaymentId(ErrorMessage = "Ngân hàng là bắt buộc")]
        public string? UserBankName { get; set; }

        [Required(ErrorMessage = "Loại giao dịch là bắt buộc")]
        public PaymentType Action { get; set; }

        public int PaymentId;
    }
}
