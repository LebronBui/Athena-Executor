using AutoMapper;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;

namespace RealEstateAuction.AutoMapperProfile
{
    public class ModelToDataModel : Profile
    {
        public ModelToDataModel()
        {
            CreateMap<User, UserDatalModel>();
        }
    }
}