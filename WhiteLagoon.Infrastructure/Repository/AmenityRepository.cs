using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {

        private readonly ApplicationDbContext _db;
         
        public AmenityRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _db = dbContext;

        }


        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Amenity entity)
        {
            _db.Amenities.Update(entity);
        }



    }
}
