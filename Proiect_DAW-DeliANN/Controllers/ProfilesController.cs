using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Show()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Edit()
        {
            return View();
        }
    }
}
