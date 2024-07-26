using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
