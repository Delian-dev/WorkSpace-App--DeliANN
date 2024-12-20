using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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

            if (TempData.ContainsKey("message")) //verificam daca e vreun mesaj de afisat
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(profile);
        }

        [Authorize(Roles ="Admin,Editor,User")]
        public IActionResult Edit(int id)
        {
            Profile profile = db.Profiles.Find(id);

            if (profile == null)
            {
                return NotFound();
            }

            if((profile.UserId == _userManager.GetUserId(User)) || //ne asiguram ca este profilul userului curent pt a avea dreptul sa-l editeze
                    User.IsInRole("Admin") || User.IsInRole("Editor"))
            {
                return View(profile);
            }
            else
            {
                TempData["message"] = "You don't have the right to modify this profile!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = profile.UserId }); //nu ie voie
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Edit(int id, Profile requestProfile, IFormFile? Image)
        {
            Profile profile = db.Profiles.Find(id);
            if(ModelState.IsValid)
            {
                if ((profile.UserId == _userManager.GetUserId(User)) //verificam iar ca userul poate efectua editul
                    || User.IsInRole("Admin") || User.IsInRole("Editor"))
                {
                    profile.Bio = requestProfile.Bio;
                    profile.DisplayName = requestProfile.DisplayName;
                    profile.IsActive = requestProfile.IsActive;

                    //partea cu imaginea
                    if (Image != null && Image.Length > 0)
                    {
                        Console.WriteLine("Debug4");
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" }; //Pentru profil il lasam doar sa aiba poza!! nu punem si extentii de tip mp4 sau mov etc. Alea sunt pt postari

                        var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            Console.WriteLine("Debug5");
                            ModelState.AddModelError("ProfileImage", "The file needs to be a jpg, jpeg or png.");
                            return View(profile);
                        }

                        var storagePath = Path.Combine(_env.WebRootPath, "images", Image.FileName);
                        var databaseFileName = "/images/" + Image.FileName;
                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }

                        Console.WriteLine("Debug6");
                        ModelState.Remove(nameof(profile.ProfileImage));
                        profile.ProfileImage = databaseFileName;
                    }

                    //profilul a fost editat cu succes
                    TempData["message"] = "Profile updated successfully!";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Show", new {id=profile.UserId});
                }

                else
                {
                    Console.WriteLine("Debug8");
                    TempData["message"] = "You don't have the right to modify this profile!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", new { id = profile.UserId }); //nu ie voie
                }
            }
            else
            {
                Console.WriteLine("Debug9");
                return View(requestProfile); //inapoi la editare
            }
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
