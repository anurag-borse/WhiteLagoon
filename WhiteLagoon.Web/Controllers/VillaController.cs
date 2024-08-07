using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;

        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villas = _villaService.GetAll();
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
                _villaService.CreateVilla(villa);
                TempData["success"] = "Villa Added Successfully";

                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa Could Not be Added";
            return View(villa);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var villa = _villaService.GetVillaById(id);

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
                _villaService.UpdateVilla(villa);
                TempData["success"] = "Villa Updated Succesfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa Could Not Be Updated";
            return View(villa);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var villa = _villaService.GetVillaById(id);

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
            bool deleted = _villaService.DeleteVilla(villa.Id);
            if (deleted)
            {
                TempData["success"] = "Villaa Deleted Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Villa Could Not be Deleted";
            }
            return View();
        }
    }
}