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
    public class VillaNumberController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;


        }

        public IActionResult Index()
        {

            var villasnumber = _unitOfWork.VillaNumber.GetAll(includeProperties:"Villa");
            return View(villasnumber);
        }

        [HttpGet]
        public IActionResult Create()
        {

            VillaNumberVM villaNumberVM = new VillaNumberVM()
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




            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberVM obj)
        {
            bool roomNumberExists = _unitOfWork.VillaNumber.Any(u => u.Villa_Number == obj.VillaNumber.Villa_Number);

            // bool isNumberUnique = _db.VillaNumbers.Where(u => u.Villa_Number == obj.Villa_Number).Count()==0;



            if (ModelState.IsValid & !roomNumberExists)
            {

                _unitOfWork.VillaNumber.Add(obj.VillaNumber);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "Villa Number Added Successfully";

                return RedirectToAction(nameof(Index));
            }


            if (roomNumberExists)
            {
                TempData["error"] = "Villa Number already Exists !!!";

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

            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id)

            };


            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }


        [HttpPost]
        public IActionResult Edit(VillaNumberVM villaNumberVM)
        {


            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Update(villaNumberVM.VillaNumber);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "Villa Number Updated Succesfully";
                return RedirectToAction(nameof(Index));
            }


            villaNumberVM.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });



            return View(villaNumberVM);
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
          //  var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            //    var villaList = _db.Villas.Where(u => u.Price > 50 && u.Occupancy > 2).ToList();

            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == id)

            };


            if (villaNumberVM.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }


        [HttpPost]
        public IActionResult Delete(VillaNumberVM obj)
        {
            VillaNumber villainDb = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == obj.VillaNumber.Villa_Number);
            if (villainDb is not null)
            {
                _unitOfWork.VillaNumber.Remove(villainDb);
                _unitOfWork.VillaNumber.Save();
                TempData["success"] = "Villaa Number Deleted Successfully";
                return RedirectToAction(nameof(Index));

            }
            TempData["error"] = "Villa Number Could Not be Deleted";
            return View();
        }
    }
}
