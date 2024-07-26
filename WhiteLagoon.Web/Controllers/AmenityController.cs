using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Infrastructure.Repository;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class AmenityController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;


        }

        public IActionResult Index()
        {

            var amenity = _unitOfWork.Amenity.GetAll(includeProperties:"Villa");
            return View(amenity);
        }

        [HttpGet]
        public IActionResult Create()
        {

            AmenityVM amenityVM = new AmenityVM()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };



            //IEnumerable<SelectListItem> list = _db.Villas.ToList().Select(u => new SelectListItem
            //{
            //    Text = u.Name,
            //    Value = u.Id.ToString()
            //}).ToList();


            // ViewData["VillaList"] = list;
            // ViewBag.VillaList = list;




            return View(amenityVM);
        }

        [HttpPost]
        public IActionResult Create(AmenityVM obj)
        {
           // bool roomNumberExists = _unitOfWork.Amenity.Any(u => u.Villa_Number == obj.Amenity.Villa_Number);

            // bool isNumberUnique = _db.Amenitys.Where(u => u.Villa_Number == obj.Villa_Number).Count()==0;



            if (ModelState.IsValid )
            {

                _unitOfWork.Amenity.Add(obj.Amenity);
                _unitOfWork.Amenity.Save();
                TempData["success"] = "Amenity Added Successfully";

                return RedirectToAction(nameof(Index));
            }



            obj.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

            return View(obj);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
           // var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            //    var villaList = _db.Villas.Where(u => u.Price > 50 && u.Occupancy > 2).ToList();

            AmenityVM amenityVM = new AmenityVM()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(u => u.Id == id)

            };


            if (amenityVM.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }


        [HttpPost]
        public IActionResult Edit(AmenityVM amenityVM)
        {


            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Update(amenityVM.Amenity);
                _unitOfWork.Amenity.Save();
                TempData["success"] = "Amenity Updated Succesfully";
                return RedirectToAction(nameof(Index));
            }


            amenityVM.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });



            return View(amenityVM);
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
          //  var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            //    var villaList = _db.Villas.Where(u => u.Price > 50 && u.Occupancy > 2).ToList();

            AmenityVM amenityVM = new AmenityVM()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(u => u.Id == id)

            };


            if (amenityVM.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }


        [HttpPost]
        public IActionResult Delete(AmenityVM obj)
        {
            Amenity villainDb = _unitOfWork.Amenity.Get(u => u.Id == obj.Amenity.Id);
            if (villainDb is not null)
            {
                _unitOfWork.Amenity.Remove(villainDb);
                _unitOfWork.Amenity.Save();
                TempData["success"] = "Amenity Deleted Successfully";
                return RedirectToAction(nameof(Index));

            }
            TempData["error"] = "Amenity Could Not be Deleted";
            return View();
        }
    }
}
