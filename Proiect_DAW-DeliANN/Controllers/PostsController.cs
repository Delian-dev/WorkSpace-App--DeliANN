using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_DAW_DeliANN.Data;
using Proiect_DAW_DeliANN.Models;
using System.Net.NetworkInformation;
//using System.Threading.Channels;

namespace Proiect_DAW_DeliANN.Controllers
{
    public class PostsController : Controller
    {

        //presupun ca o sa am nevoie de new/edit/delete cu mentiunea sa am grija cine are voie sa faca asta
        //o sa am lista de useri in dreapta 
        //o sa am optiune de raspuns => o sa am o metoda (un buton ig) care o sa ia id-ul si il trimite in metoda
        //si o sa apara cumva sters idk still thinking
        //ce e de mentionat e ca o sa aiba poza de profil yucky
        //anyways si reaction o sa ai un buton, o lista initial hidden si dupa apare cu ce alegi si poti sa alegi doar unul si sa il schimbi
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public PostsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
         IWebHostEnvironment env
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }
        //Delete pe post => ai voie doar ca admin/editor/tu care l-a scris/mod
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]    //DE SETAT ACCESSRIGHTS PE BAZA ROLULUI IN WORKSPACE (LA FEL CA IN WORKSPACE SHOW) - PENTRU CA APOI IN VIEW SA STIM CAND SAU NU AFISAM BUTOANELE DE EDIT/DELETE
        public IActionResult Delete(int id)
        {
            Post post = db.Posts
               .Include(p => p.Channel)
               .FirstOrDefault(p => p.PostId == id);
            if (post == null)
            {
                return Forbid();
            }

            //facem rost de workspace-ul in care se afla postarea
            int? workspaceId = post.Channel.WorkspaceId;

            var userId = _userManager.GetUserId(User);

            var userWorkspace = db.ApplicationUserWorkspaces //verifica daca userul e moderator in workspace
                                  .FirstOrDefault(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId && uw.moderator == true);


            Console.WriteLine(userWorkspace);
            if (!User.IsInRole("Admin") && !User.IsInRole("Editor") && userWorkspace == null && post.UserId != userId) //aceeasi modificare ca la edit
            {
                //TempData["message"] = "You can't edit this post";
                //TempData["messageType"] = "alert-danger";
                return Redirect("/Channels/Show/" + post.ChannelId);
            } 
            db.Posts.Remove(post);
            db.SaveChanges();

            //TempData["message"] = "Post deleted successfully";
            //TempData["messageType"] = "alert-success";

            return Redirect("/Channels/Show/" + post.ChannelId);
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Console.WriteLine("Vreau sa editez!");
            Post post = db.Posts
               .Include(p => p.Channel)
               .FirstOrDefault(p => p.PostId == id);
            if (post == null)
            {
                return Forbid();
            }
            Console.WriteLine(post.ChannelId);
            //facem rost de workspace-ul in care se afla postarea
            int? workspaceId = post.Channel.WorkspaceId;
            Console.WriteLine(workspaceId);
            var userId = _userManager.GetUserId(User);

            var userWorkspace = db.ApplicationUserWorkspaces //verifica daca userul e moderator in workspace
                                  .FirstOrDefault(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId && uw.moderator == true);

            
            Console.WriteLine(userWorkspace);
            if (!User.IsInRole("Admin") && !User.IsInRole("Editor") && userWorkspace == null && post.UserId != userId) //aceeasi modificare ca la edit
            {
                    //TempData["message"] = "You can't edit this post";
                    //TempData["messageType"] = "alert-danger";
                    return Redirect("/Channels/Show/" + post.ChannelId);
            }
            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Post requestPost)
        {
            Post post = db.Posts
               .Include(p => p.Channel)
               .FirstOrDefault(p => p.PostId == id);
            if (post == null)
            {
                return Forbid();
            }
    
            //facem rost de workspace-ul in care se afla postarea
            int? workspaceId = post.Channel.WorkspaceId;

            var userId = _userManager.GetUserId(User);

            var userWorkspace = db.ApplicationUserWorkspaces //verifica daca userul e moderator in workspace
                                  .FirstOrDefault(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId && uw.moderator == true);


            Console.WriteLine(userWorkspace);
            if (!User.IsInRole("Admin") && !User.IsInRole("Editor") && userWorkspace == null && post.UserId != userId) //aceeasi modificare ca la edit
            {
                //TempData["message"] = "You can't edit this post";
                //TempData["messageType"] = "alert-danger";
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (requestPost.ChannelId != post.ChannelId)
                {
                    return View(requestPost); //ne intoarcem in formular
                }
                Console.WriteLine("Boom");
                //updatam campurile
                post.Content = requestPost.Content;
                post.Type = requestPost.Type;
                db.SaveChanges();
                Console.WriteLine("Boom");
                //TempData["message"] = "Channel updated successfully";
                //TempData["messageType"] = "alert-success";
                Console.WriteLine("Boom");
                return Redirect("/Channels/Show/" + post.ChannelId);
            }
            else
            {
                Console.WriteLine("Boom");
                return View(requestPost);
            }

        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(int? id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Editor"))
            {
                var userId = _userManager.GetUserId(User);
                Channel chan = db.Channels.Find(id);
                //Channel chn = db.Channels.Include("Workspaces").Where(c => c.ChannelId == id).First();
                int? wrkId = chan.WorkspaceId;
                var userWorkspace = db.ApplicationUserWorkspaces
                                      .FirstOrDefault(uw => uw.WorkspaceId == wrkId && uw.UserId == userId); //verificam daca e moderator in acest workspace


                if (userWorkspace == null)
                { //acest user nu are drept sa creeze canale pe acest worksop
                    TempData["message"] = "You can't add a new post in this channel.";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Workspaces/Index/");
                }
            }
            if (id != null) //s-a intrat legal pe aceasta metoda :)))))((*9
            {
                //creare canal nou
                Post post = new Post //MOTIV PENTRU CARE CRAPA NEW-UL (nu primea workspaceId desi noi il setam aici):
                {                             //chiar daca noi il completam de aici pt canalul ce va fi nou, este nevoie ca in form-ul din view sa avem si input hidden pt acest camp ca sa realizez model binding
                    ChannelId = id,
                    UserId = _userManager.GetUserId(User)
                };

                return View(post); //mergem in view pentru a completa restul campurilor
            }
            else //cazul de intrare prin Channels/New direct (fara workspaceId)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public async Task<IActionResult> New(Post post, IFormFile Media) //de reparat new-ul astfel incat sa putem incarca postare si fara continut media
        {
            var sanitizer = new HtmlSanitizer();


            if (!User.IsInRole("Admin") && !User.IsInRole("Editor"))
            {

                var userId = _userManager.GetUserId(User);

                var userWorkspace = db.ApplicationUserWorkspaces
                                      .FirstOrDefault(uw => uw.UserId == userId);

                if (userWorkspace == null)
                {
                    return Forbid();
                }


            }


            post.Date = DateTime.Now;
            post.UserId = _userManager.GetUserId(User);
            Console.WriteLine("test media2");
            if (Media != null && Media.Length > 0)
            {
                Console.WriteLine("test media3");
                //Verificam extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Media.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PostMedia", "Wrong format :jpg, jpeg, png, gif, mp4, mov only.");
                    return View(post);
                }
                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "images", Media.FileName);
                var databaseFileName = "/images/" + Media.FileName;
                // Salvare fișier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Media.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(post.Media));
                post.Media = databaseFileName;
            }
            else
            {
                Console.WriteLine("test media4");
                post.Media = null;
            }
            if(post.Content!=null)
            {
                Console.WriteLine("test media5");
                post.Content = sanitizer.Sanitize(post.Content);
                // Adăugare articol
                db.Posts.Add(post);
                await db.SaveChangesAsync();
                // Redirecționare după succes
                return Redirect("/Channels/Show/" + post.ChannelId);
            }

            return View(post);
        }

    }
}
