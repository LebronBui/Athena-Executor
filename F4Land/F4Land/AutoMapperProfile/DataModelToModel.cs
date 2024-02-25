using AutoMapper;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RealEstateAuction.AutoMapperProfile
{
    public class DataModelToModel : Profile
    {
        public DataModelToModel()
        {
            CreateMap<UserDatalModel, User>();
            CreateMap<AuctionDataModel, Auction>();
            CreateMap<AuctionEditDataModel, Auction>();
        }
    }
}
