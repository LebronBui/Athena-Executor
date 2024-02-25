using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class TicketCommentDataModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int TicketId { get; set; }
        [Required(ErrorMessage = "Nội dung trả lời là bắt buộc")]
        public string Comment { get; set; }
        public bool IsClosed { get; set; }
    }
}
