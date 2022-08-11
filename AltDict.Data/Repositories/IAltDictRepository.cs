using AltDict.Data.Dtos;

namespace AltDict.Data.Repositories
{
    public interface IAltDictRepository
    {
        List<ConnectionDto> GetAllConnections();
        ConnectionDto CreateUpdateConnection(ConnectionDto connectionDto);
        void DeleteConnection(Guid connectionId);
        List<List<SearchResultDto>> SearchRoutes(SearchDto searchDto);
    }
}