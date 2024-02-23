using AutoMapper;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;

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
