using Proiect_DAW_DeliANN.Data;
using Proiect_DAW_DeliANN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Proiect_DAW_DeliANN.Controllers
{
    [Authorize(Roles = "Admin")] //admin only
    public class CategoriesController : Controller
    {
        //PASUL 10: useri si roluri
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index() //la cezara e de tip ActionResult dar din ce inteleg e doar o forma default de IActionResult
        {
            //adaugarea in ViewBag al unui mesaj pentru afisare, daca avem ceva de transmis (adaugare/modificare/stergere categorie)
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories; //mergem cu toate categoriile in view-ul de afisare
            return View();
        }

        public IActionResult Show(int id)
        {
            Category category = db.Categories.Find(id); //mergem in view cu categoria specificata
            return View(category);
        }

        public IActionResult New() //HTTPGET implicit
        {
            return View();
        }

        [HttpPost]
        public IActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
            }
            else
            {
                return View(cat);
            }
        }

        public IActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Find(id);

            if (ModelState.IsValid)
            {
                category.CategoryName = requestCategory.CategoryName;
                db.SaveChanges();
                TempData["message"] = "Categoria a fost modificata!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id) {
            Category category = db.Categories.Include("Workspaces")
                                             .Include("Workspaces.Channels")
                                             .Include("Workspaces.Channels.Posts")
                                             .Include("Workspaces.Channels.Posts.Reactions")
                                             .Where(c => c.CategoryId == id)
                                             .First();
            db.Categories.Remove(category);

            TempData["message"] = "Categoria a fost stearsa";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
