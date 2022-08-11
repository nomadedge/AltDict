using AltDict.Data.Dtos;
using AltDict.Wpf.Models;
using AutoMapper;
using System.Windows.Documents;

namespace AltDict.Data.AutoMapper
{
    public class DtoModelProfile : Profile
    {
        public DtoModelProfile()
        {
            CreateMap<SearchResultDto, SearchResultModel>()
                ;

        }
    }
}
