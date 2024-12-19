using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_DAW_DeliANN.Data;
using Proiect_DAW_DeliANN.Models;

namespace Proiect_DAW_DeliANN.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env; //ne trebuie pt poza de profil

        public ProfilesController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
              IWebHostEnvironment env) //se adauga si in constructor
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        public IActionResult Show(string id)
        {
            Profile profile = db.Profiles
                                .Include(p => p.User)
                                .FirstOrDefault(p => p.UserId == id); //nu cautam dupa cheia primara deci trebuie cu FirstOrDeafult. Altfel mergea si cu Find
            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.Status = profile.IsActive switch //switch pt a afisa statusul userului
            {
                true => "Active",
                false => "Inactive",
                null => "Hidden"
            };

            SetAccessRights(id); //stabilim privilegiile (pentru butonul de edit profile din Show)

            return View(profile);
        }

        [HttpPost]
        public IActionResult Edit()
        {
            return View();
        }

        private void SetAccessRights(string UserId)
        {
            ViewBag.EsteAdmin = User.IsInRole("Admin");
            ViewBag.EsteEditor = User.IsInRole("Editor");

            var currentUserId = _userManager.GetUserId(User);

            ViewBag.OwnProfile = currentUserId == UserId; //variabila in care retinem daca userul curent este si detinatorul profilului
        }
    }
}
