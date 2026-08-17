using AutoMapper;
using SABEMITEC.PagamentoAPI.Repository;
using SABEMITEC.PagamentoAPI.Service;

namespace SABEMITEC.PagamentoAPI.ConfigMapper
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            return new MapperConfiguration(config =>
            {
                config.CreateMap<IEventoBrutoRepository, EventoBrutoRepository>().ReverseMap();
                config.CreateMap<IEventoBrutoService, EventoBrutoService>().ReverseMap();
            });
        }
    }
}
