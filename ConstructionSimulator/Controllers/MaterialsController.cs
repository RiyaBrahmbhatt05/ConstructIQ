using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Materials
        public IActionResult Index()
        {
            var materials = _context.Materials.ToList();
            return View(materials);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Materials.Add(material);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Material '{material.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(material);
        }

        // GET: Materials/Edit/5
        public IActionResult Edit(int id)
        {
            var material = _context.Materials.FirstOrDefault(m => m.MaterialId == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Material material)
        {
            if (id != material.MaterialId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingMaterial = _context.Materials.FirstOrDefault(m => m.MaterialId == id);
                if (existingMaterial == null)
                {
                    return NotFound();
                }

                existingMaterial.Name = material.Name;
                existingMaterial.Description = material.Description;
                existingMaterial.Quantity = material.Quantity;
                existingMaterial.UnitType = material.UnitType;
                existingMaterial.CostPerUnit = material.CostPerUnit;
                existingMaterial.LeadTimeDays = material.LeadTimeDays;
                existingMaterial.InStock = material.InStock;

                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Material '{material.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(material);
        }

        // GET: Materials/Delete/5
        public IActionResult Delete(int id)
        {
            var material = _context.Materials.FirstOrDefault(m => m.MaterialId == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var material = _context.Materials.FirstOrDefault(m => m.MaterialId == id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Material '{material.Name}' deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}