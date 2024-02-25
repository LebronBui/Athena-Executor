using AutoMapper;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RealEstateAuction.AutoMapperProfile
{
    public class ModelToDataModel : Profile
    {
        public ModelToDataModel()
        {
            CreateMap<User, UserDatalModel>();
            CreateMap<Auction, AuctionDataModel>();
            CreateMap<Auction, AuctionEditDataModel>();
        }
    }
}
