using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Infrastructure.Repository
{
    public interface IVillaNumberRepository : IRepository<VillaNumber>
    {

        void Update(VillaNumber entity);

        void Save();


    }
}
