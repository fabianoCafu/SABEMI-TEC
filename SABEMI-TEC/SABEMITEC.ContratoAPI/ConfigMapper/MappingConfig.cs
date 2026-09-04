using AutoMapper;
using SABEMITEC.ContratoAPI.Repository;
using SABEMITEC.ContratoAPI.Service;

namespace SABEMITEC.ContratoAPI.ConfigMapper
{
    public static class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            return new MapperConfiguration(config =>
            {
                config.CreateMap<IContratoService, ContratoService>().ReverseMap();
                config.CreateMap<IContratoRepository, ContratoRepository>().ReverseMap();
            });
        }
    }
}
