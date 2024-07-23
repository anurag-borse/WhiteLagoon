using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {

        private readonly ApplicationDbContext _db;

        public VillaController(ApplicationDbContext dbContext)
        {
            _db = dbContext;

        }

        public IActionResult Index()
        {

            var villas = _db.Villas.ToList();
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

                _db.Villas.Add(villa);
                _db.SaveChanges();
                TempData["success"] = "Villa Added Successfully";

                return RedirectToAction("Index", "Villa");
            }
            TempData["error"] = "Villa Could Not be Added";
            return View(villa);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            var villaList = _db.Villas.Where(u => u.Price > 50 && u.Occupancy > 2).ToList();

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
                _db.Villas.Update(villa);
                _db.SaveChanges();
                TempData["success"] = "Villa Updated Succesfully";
                return RedirectToAction("Index", "Villa");
            }
            TempData["error"] = "Villa Could Not Be Updated";
            return View(villa);
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

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
            var villainDb = _db.Villas.FirstOrDefault(u => u.Id == villa.Id);

            if (villainDb is not null)
            {
                _db.Villas.Remove(villainDb);
                _db.SaveChanges();
                TempData["success"] = "Villaa Deleted Successfully";
                return RedirectToAction("Index", "Villa");

            }
            TempData["error"] = "Villa Could Not be Deleted";
            return View();

        }
    }
}
