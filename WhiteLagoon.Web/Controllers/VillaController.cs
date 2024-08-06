using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
            return View(villas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("", "Name and Description can not be the same");
            }
            if (ModelState.IsValid)
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
                TempData["success"] = "Villa Added Successfully";

                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa Could Not be Added";
            return View(villa);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == id);

            //var villaList = _db.Villas.Where(u => u.Price > 50 && u.Occupancy > 2).ToList();

            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Edit(Villa villa)
        {
            if (ModelState.IsValid)
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
                TempData["success"] = "Villa Updated Succesfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa Could Not Be Updated";
            return View(villa);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == id);

            if (villa == null)
            {
                TempData["error"] = "Villa Not Found";
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var villainDb = _unitOfWork.Villa.Get(u => u.Id == villa.Id);

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
                TempData["success"] = "Villaa Deleted Successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa Could Not be Deleted";
            return View();
        }
    }
}