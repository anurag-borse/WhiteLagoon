using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Infrastructure.Repository
{
    public interface IAmenityRepository : IRepository<Amenity>
    {
        void Update(Amenity entity);

        void Save();
    }
}