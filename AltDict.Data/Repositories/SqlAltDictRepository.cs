using AltDict.Data.DbContexts;
using AltDict.Data.Dtos;
using AltDict.Data.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace AltDict.Data.Repositories
{
    public class SqlAltDictRepository : IAltDictRepository
    {
        private readonly AltDictDbContext _altDictDbContext;
        private readonly IMapper _mapper;

        public SqlAltDictRepository(
            AltDictDbContext altDictDbContext,
            IMapper mapper)
        {
            _altDictDbContext = altDictDbContext;
            _mapper = mapper;
        }

        private string TransformVendorCode(string vendorCode)
        {
            if (string.IsNullOrWhiteSpace(vendorCode))
            {
                throw new Exception("Vendor code can't be empty.");
            }
            var result = string.Concat(vendorCode.Where(c => char.IsDigit(c) || char.IsLetter(c)));
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception("Vendor code can't be empty.");
            }
            return result;
        }

        private string TransformManufacturer(string manufacturer)
        {
            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                throw new Exception("Manufacturer can't be empty.");
            }
            var result = manufacturer.ToLower();
            return result;
        }

        private ConnectionDto TransformConnection(ConnectionDto connectionDto)
        {
            connectionDto.VendorCode1 = TransformVendorCode(connectionDto.VendorCode1);
            connectionDto.Manufacturer1 = TransformManufacturer(connectionDto.Manufacturer1);
            connectionDto.VendorCode2 = TransformVendorCode(connectionDto.VendorCode2);
            connectionDto.Manufacturer2 = TransformManufacturer(connectionDto.Manufacturer2);
            return connectionDto;
        }

        private SearchDto TransformSearch(SearchDto searchDto)
        {
            searchDto.VendorCode1 = TransformVendorCode(searchDto.VendorCode1);
            searchDto.Manufacturer1 = TransformManufacturer(searchDto.Manufacturer1);
            searchDto.VendorCode2 = TransformVendorCode(searchDto.VendorCode2);
            searchDto.Manufacturer2 = TransformManufacturer(searchDto.Manufacturer2);
            return searchDto;
        }

        public List<ConnectionDto> GetAllConnections()
        {
            var connections = _altDictDbContext.Connections
                .ProjectTo<ConnectionDto>(_mapper.ConfigurationProvider)
                .ToList();
            return connections;
        }

        public ConnectionDto CreateUpdateConnection(ConnectionDto connectionDto)
        {
            connectionDto = TransformConnection(connectionDto);

            if (connectionDto.VendorCode1 == connectionDto.VendorCode2 &&
                connectionDto.Manufacturer1 == connectionDto.Manufacturer2)
            {
                throw new Exception("Product cannot be connected to itself.");
            }

            using (var transaction = _altDictDbContext.Database.BeginTransaction())
            {
                try
                {
                    var connectionDuplicate = _altDictDbContext.Connections
                        .SingleOrDefault(c =>
                            (c.ConnectionId != connectionDto.ConnectionId) &&
                            ((c.VendorCode1 == connectionDto.VendorCode1 &&
                            c.VendorCode2 == connectionDto.VendorCode2 &&
                            c.Manufacturer1 == connectionDto.Manufacturer1 &&
                            c.Manufacturer2 == connectionDto.Manufacturer2) ||
                            (c.VendorCode1 == connectionDto.VendorCode2 &&
                            c.VendorCode2 == connectionDto.VendorCode1 &&
                            c.Manufacturer1 == connectionDto.Manufacturer2 &&
                            c.Manufacturer2 == connectionDto.Manufacturer1)));
                    if (connectionDuplicate is not null)
                    {
                        throw new Exception("Connection already exists.");
                    }

                    if (connectionDto.ConnectionId is not null)
                    {
                        var connectionExist = _altDictDbContext.Connections
                            .Find(connectionDto.ConnectionId);
                        if (connectionExist is null)
                        {
                            throw new Exception("Connection not found.");
                        }
                        _mapper.Map(connectionDto, connectionExist);

                        _altDictDbContext.SaveChanges();
                        transaction.Commit();

                        return _mapper.Map<ConnectionDto>(connectionExist);
                    }
                    else
                    {
                        var connection = _mapper.Map<Connection>(connectionDto);
                        _altDictDbContext.Connections.Add(connection);
                        _altDictDbContext.SaveChanges();
                        transaction.Commit();

                        return _mapper.Map<ConnectionDto>(connection);
                    }


                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        public void DeleteConnection(Guid connectionId)
        {
            using (var transaction = _altDictDbContext.Database.BeginTransaction())
            {
                try
                {
                    var connectionExist = _altDictDbContext.Connections
                        .Find(connectionId);
                    if (connectionExist is null)
                    {
                        throw new Exception("Connection not found.");
                    }
                    _altDictDbContext.Connections.Remove(connectionExist);
                    _altDictDbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        public List<List<SearchResultDto>> SearchRoutes(SearchDto searchDto)
        {
            if (searchDto.SearchDepth < 1)
            {
                throw new Exception("Search depth must be positive number.");
            }
            searchDto = TransformSearch(searchDto);
            var connections = _altDictDbContext.Connections.ToList();
            var visited = new Dictionary<(string, string), bool>();
            var routes = new List<List<SearchResultDto>>();
            var route = new List<SearchResultDto>();
            FindRoutes(connections,
                searchDto.VendorCode1,
                searchDto.Manufacturer1,
                searchDto.VendorCode2,
                searchDto.Manufacturer2,
                searchDto.SearchDepth,
                visited,
                routes,
                route);
            return routes;
        }

        private void FindRoutes(
            List<Connection> connections,
            string vendorCode1,
            string manufacturer1,
            string vendorCode2,
            string manufacturer2,
            byte searchDepth,
            Dictionary<(string, string), bool> visited,
            List<List<SearchResultDto>> routes,
            List<SearchResultDto> route)
        {
            if (vendorCode1 == vendorCode2 &&
                manufacturer1 == manufacturer2)
            {
                routes.Add(route);
            }
            var localVisited = new Dictionary<(string, string), bool>(visited);
            localVisited[(vendorCode1, manufacturer1)] = true;

            var cons = connections.Where(c =>
                (c.VendorCode1 == vendorCode1 && c.Manufacturer1 == manufacturer1) ||
                (c.VendorCode2 == vendorCode1 && c.Manufacturer2 == manufacturer1))
                .Select(c => new {
                    Connection = c,
                    IsTargetFirst = c.VendorCode2 == vendorCode1 && c.Manufacturer2 == manufacturer1
                })
                .ToList();

            foreach (var con in cons)
            {
                var targetKey = con.IsTargetFirst ?
                    (con.Connection.VendorCode1, con.Connection.Manufacturer1) :
                    (con.Connection.VendorCode2, con.Connection.Manufacturer2);
                if ((!localVisited.TryGetValue(targetKey, out bool isVisited) || !isVisited) && con.Connection.TrustLevel > 0)
                {
                    if (searchDepth - 1 >= 0)
                    {
                        vendorCode1 = targetKey.Item1;
                        manufacturer1 = targetKey.Item2;
                        var sourceKey = con.IsTargetFirst ?
                            (con.Connection.VendorCode2, con.Connection.Manufacturer2) :
                            (con.Connection.VendorCode1, con.Connection.Manufacturer1);
                        var localRoute = new List<SearchResultDto>(route);
                        localRoute.Add(new SearchResultDto
                        {
                            VendorCode1 = sourceKey.Item1,
                            Manufacturer1 = sourceKey.Item2,
                            VendorCode2 = targetKey.Item1,
                            Manufacturer2 = targetKey.Item2
                        });
                        FindRoutes(
                            connections,
                            vendorCode1,
                            manufacturer1,
                            vendorCode2,
                            manufacturer2,
                            (byte)(searchDepth - 1),
                            localVisited,
                            routes,
                            localRoute);
                    }
                }
            }

            localVisited[(vendorCode1, manufacturer1)] = false;
        }
    }
}
