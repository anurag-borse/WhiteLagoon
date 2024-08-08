using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public void CreateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(villa.Image.FileName);
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, @"Image\VillaImage");

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    villa.Image.CopyTo(fileStream);
                }
                villa.ImageUrl = @"\Image\VillaImage\" + fileName;
            }
            else
            {
                villa.ImageUrl = "https://placehold.co/600*400";
            }

            _unitOfWork.Villa.Add(villa);
            _unitOfWork.Villa.Save();
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                var villainDb = _unitOfWork.Villa.Get(u => u.Id == id);

                if (villainDb is not null)
                {
                    if (!string.IsNullOrEmpty(villainDb.ImageUrl))
                    {
                        string oldfilePath = Path.Combine(_webHostEnvironment.WebRootPath, villainDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldfilePath))
                        {
                            System.IO.File.Delete(oldfilePath);
                        }
                    }

                    _unitOfWork.Villa.Remove(villainDb);
                    _unitOfWork.Villa.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Villa GetVillaById(int id)
        {
            return _unitOfWork.Villa.Get(u => u.Id == id);
        }

        public void UpdateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + "_" + Path.GetExtension(villa.Image.FileName);
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, @"Image\VillaImage");

                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    string oldfilePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldfilePath))
                    {
                        System.IO.File.Delete(oldfilePath);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    villa.Image.CopyTo(fileStream);
                }
                villa.ImageUrl = @"\Image\VillaImage\" + fileName;
            }

            _unitOfWork.Villa.Update(villa);
            _unitOfWork.Villa.Save();
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
        }

        public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomsAvailable_Count
                    (villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);

                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }

            return villaList;
        }

        public bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
        {
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            int roomAvailable = SD.VillaRoomsAvailable_Count
                (villaId, villaNumbersList, checkInDate, nights, bookedVillas);

            return roomAvailable > 0;
        }
    }
}