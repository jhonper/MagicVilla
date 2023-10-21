using AutoMapper;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.DTO;

namespace MagicVilla_API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            // forma para colocar en línea separadas
            CreateMap<Villa, VillaDTO>(); // fuente y destino
            CreateMap<VillaDTO, Villa>();// destino y fuente

            // Forma en una sola linea
            CreateMap<Villa, VillaCreateDTO>().ReverseMap();

            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();


            // Mapeos de NumeroVilla
            CreateMap<NumeroVilla, NumeroVillaDTO>().ReverseMap();
            CreateMap<NumeroVilla, NumeroVillaCreateDTO>().ReverseMap();
            CreateMap<NumeroVilla, NumeroVillaUpdateDTO>().ReverseMap();
            CreateMap<NumeroVilla, VillaCreateDTO>().ReverseMap();
        }
    }
}
