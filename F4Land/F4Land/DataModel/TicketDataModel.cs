using RealEstateAuction.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class TicketDataModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public ICollection<IFormFile> ImageFiles { get; set; }
        public ICollection<TicketImage>? Images { get; set; }

    }
}
