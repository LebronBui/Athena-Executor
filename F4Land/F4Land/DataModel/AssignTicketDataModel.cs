
using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class AssignTicketDataModel
    {
        [Required]
        public int StaffId { get; set; }
        [Required]
        public int TicketId { get; set; }
    }
}
