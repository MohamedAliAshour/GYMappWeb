using AutoMapper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblMemberShipType;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.ViewModels.TblUserMemberShip;


namespace GYMappWeb.Helpers
{
    public class ObjectMapper
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                // This line ensures that internal properties are also mapped over.
                cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
                cfg.AddProfile<mapperProfile>();
            });
            var mapper = config.CreateMapper();
            return mapper;
        });

        public static IMapper Mapper => Lazy.Value;
    }

    public class mapperProfile : Profile
    {

        public mapperProfile()
        {

            #region TblUser
            CreateMap<TblUser, TblUserViewModel>()
                 .ForMember(dest => dest.TblUserMemberShips, opt => opt.MapFrom(src => src.TblUserMemberShips))
                .ReverseMap();
            CreateMap<TblUser, GetWithPaginationTblUserViewModel>().ReverseMap();
            CreateMap<TblUser, SaveTblUserViewModel>().ReverseMap();
            #endregion

            #region TblUserMemberShip
            CreateMap<TblUserMemberShip, TblUserMemberShipViewModel>()
                 .ForMember(dest => dest.TblMemberShipFreezes, opt => opt.MapFrom(src => src.TblMemberShipFreezes))
                 .ForMember(dest => dest.Off, opt => opt.MapFrom(src => src.Off))
                 .ForMember(dest => dest.MemberShipTypes, opt => opt.MapFrom(src => src.MemberShipTypes))
                 .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ReverseMap();
            CreateMap<TblUserMemberShip, GetWithPaginationTblUserMemberShipViewModel>().ReverseMap();
            CreateMap<TblUserMemberShip, SaveTblUserMemberShipViewModel>().ReverseMap();
            #endregion


            #region TblMemberShipFreeze
            CreateMap<TblMemberShipFreeze, TblMemberShipFreezeViewModel>()
                 .ForMember(dest => dest.UserMemberShip, opt => opt.MapFrom(src => src.UserMemberShip))
                .ReverseMap();
            CreateMap<TblMemberShipFreeze, GetWithPaginationTblMemberShipFreezeViewModel>().ReverseMap();
            CreateMap<TblMemberShipFreeze, SaveTblMemberShipFreezeViewModel>().ReverseMap();
            #endregion

            #region TblMembershipType
            CreateMap<TblMembershipType, TblMemberShipTypeViewModel>()
                 .ForMember(dest => dest.TblUserMemberShips, opt => opt.MapFrom(src => src.TblUserMemberShips))
                 .ForMember(dest => dest.TblOffers, opt => opt.MapFrom(src => src.TblOffers))
                .ReverseMap();
            CreateMap<TblMembershipType, GetWithPaginationTblMemberShipTypeViewModel>().ReverseMap();
            CreateMap<TblMembershipType, SaveTblMemberShipTypeViewModel>().ReverseMap();
            #endregion

            #region TblOffer
            CreateMap<TblOffer, TblOfferViewModel>()
                 .ForMember(dest => dest.TblUserMemberShips, opt => opt.MapFrom(src => src.TblUserMemberShips))
                 .ForMember(dest => dest.MemberShipTypes, opt => opt.MapFrom(src => src.MemberShipTypes))
                .ReverseMap();
            CreateMap<TblOffer, GetWithPaginationTblOfferViewModel>().ReverseMap();
            CreateMap<TblOffer, SaveTblOfferViewModel>().ReverseMap();
            #endregion

        }
    }
}
