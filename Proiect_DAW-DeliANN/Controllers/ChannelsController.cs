using Proiect_DAW_DeliANN.Data;
using Proiect_DAW_DeliANN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Proiect_DAW_DeliANN.Models.ApplicationUserWorkspaces;

namespace Proiect_DAW_DeliANN.Controllers
{   //ATENTIE: TREBUIE SA MODIFICAM PESTE TOT UNDE VERIFICAM CA UN USER FACE PARTE DINTR-UN WORKSPACE PENTRU A FACE MODIFICARI
    //TREBUIE VERIFICAT: 1. CA STATUS SA FIE TRUE (momentan poate face modificari si daca e false => este inca la nivel de request)
    //TREBUIE VERIFICAT: 2. CA MODERATOR SA FIE SET TO TRUE
    public class ChannelsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ChannelsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db=context;
            _userManager=userManager;
            _roleManager=roleManager;
        }
        //okay deci anita aici i will make a few logic changes so hear me out
        //tu in workspace show o sa vezi lista de canale => index de channel
        //daca esti admin/editor/mod o sa ai si butoanele de edit, new, delete langa fiecare channel si te redirectioneaza pe paginile acelea
        //daca nu, din click pe un channel => te trimite in show, unde vezi mesajele de pe el 
        //sus, o sa ai numele workspace-ului ca sa stii unde anume esti
        //in stanga o sa ai un index de channels, ca sa poti sa circuli usor intre ele
        //pe centru, o sa fie show-ul in sine, adica lista de mesaje, de la cel mai recent jos, spre cel mai vechi

        //[NonAction]
        ////cred ca trebuie nonaction, ca sa nu poata fi accesat prin url
        //public IActionResult Index(int? workspaceId) //in mod normal nu o sa avem Index pe channels pt ca le vizualizam in Workspace/Show
        //{
        //    var channel = db.Channels.Include("Workspaces")
        //                            .FirstOrDefault(wrk => wrk.WorkspaceId == workspaceId); 
        //    return View();
        //}
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            //canalul care este afisat => toate mesajele din el
            Channel channel = db.Channels.Include("Posts")
                                           //.Include("User")
                                           .Include("Posts.User")
                                           .Where(pst => pst.ChannelId == id)
                                           .First();


            //    var channel = db.Channels
            //    .Include(c => c.Posts) // Include posts
            // .ThenInclude(p => p.User) // Include the user for each post
            //.FirstOrDefault(c => c.ChannelId == id);

            channel.Posts = channel.Posts
                    .OrderByDescending(p => p.Date)
                    .ToList();
            int? wrkId = channel.WorkspaceId;
            Workspace w = db.Workspaces.Include("Channels")
                .Where(wId => wId.WorkspaceId == wrkId)
                .First();
            ViewBag.WorkspaceChannels = w.Channels;
            return View(channel);

        }
        //[Authorize(Roles = "User,Editor,Admin")]
        //public IActionResult Show_Channels(int id)
        //{


        //}

        [Authorize(Roles = "User,Editor,Admin")] //indiferent daca orice user sau doar moderatorii pot face new/edit/delete pe canale
                                                 //trebuie gandita o logica ce impiedica useri care nu fac parte din workspace sa modifice canalele (sa nu poata patrunde prin http)
        //da deci reminder sa trimited parametrii cu numele id:))
        public IActionResult New(int? id) //workspace-ul in care se va adauga canalul. AM FACUT PARAMETRUL NULLABLE IN CAZ CA UN HACKER CRAZY ADMIN/EDITOR intra direct pe calea Channels/New fara sa intre dintr-un workspace => nu am avea workspace id
        {                                         //Trebuie transmis id-ul workspace-ului cand este apasat butonul de add channel din workspaces/show !!! (anita reminder)
            //verificam daca user-ul care vrea sa creeze canalul are acces la acest workspace (trebuie modificat a.i. sa se verifice si daca e moderator)
            //daca e Admin sau Editor nu verificam legatura din tabelul intermediar
            if (!User.IsInRole("Admin") && !User.IsInRole("Editor"))
            {
                var userId = _userManager.GetUserId(User);

                var userWorkspace = db.ApplicationUserWorkspaces
                                      .FirstOrDefault(uw => uw.WorkspaceId == id && uw.UserId == userId && uw.moderator==true); //verificam daca e moderator in acest workspace

                if (userWorkspace == null)
                { //acest user nu are drept sa creeze canale pe acest worksop
                    TempData["message"] = "You can't add a new channel in this workspace";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Workspaces/Show/"+id);
                }
            }

            if (id != null) //s-a intrat legal pe aceasta metoda :)))))((*9
            {
                //creare canal nou
                Channel channel = new Channel //MOTIV PENTRU CARE CRAPA NEW-UL (nu primea workspaceId desi noi il setam aici):
                {                             //chiar daca noi il completam de aici pt canalul ce va fi nou, este nevoie ca in form-ul din view sa avem si input hidden pt acest camp ca sa realizez model binding
                    WorkspaceId = id
                };

                return View(channel); //mergem in view pentru a completa restul campurilor
            }
            else //cazul de intrare prin Channels/New direct (fara workspaceId)
            {
                return Forbid();
            }
        }


        //Adaugarea canalului in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(Channel channel) {
            if(ModelState.IsValid)
            {
                db.Channels.Add(channel);
                db.SaveChanges();
                TempData["message"] = "Canalul a fost creat!";
                TempData["messageType"] = "alert-success";
                return Redirect("/Workspaces/Show/" + channel.WorkspaceId); //ne intoarcem la Workspace Show unde avem toate canalele
            }
            else
            {
                return View(channel); //incercam sa il cream din nou
            }
        }

        [Authorize(Roles ="User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Channel channel = db.Channels.Find(id);

            if(channel == null)
            {
                return Forbid(); //daca somehow canalul nu este gasit sau se incearca patrunderea din root (adica fara sa intram in metoda cu un channelId)
            }

            //ne asiguram ca doar Adminul/Editorul/Userii care fac parte din acel workspace pot edita canalul
            if(!User.IsInRole("Admin") && !User.IsInRole("Editor"))
            {
                var userId = _userManager.GetUserId(User);

                //verificam ca userul este in workspace + ca userul e moderator dar inca nu avem campul
                var userWorkspace = db.ApplicationUserWorkspaces
                                      .FirstOrDefault(uw => uw.WorkspaceId == channel.WorkspaceId && uw.UserId == userId && uw.moderator==true);

                if (userWorkspace == null) {
                    TempData["message"] = "You can't edit this channel";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Workspaces/Show/" + channel.WorkspaceId);
                }
            }

            //mergem cu channel-ul in view-ul de editare
            return View(channel);
        }

        //Salvarea editarilor in baza de date
        [HttpPost]
        [Authorize(Roles="User,Editor,Admin")]
        public IActionResult Edit(int id, Channel requestChannel)
        {
            //verificam sa fie in continuare totul ok
            Channel channel = db.Channels.Find(id);

            if (channel == null)
            {
                return Forbid(); //daca somehow canalul nu este gasit sau se incearca patrunderea din root
            }

            //ne asiguram ca doar Adminul/Editorul/Userii care fac parte din acel workspace pot edita canalul
            if (!User.IsInRole("Admin") && !User.IsInRole("Editor"))
            {
                var userId = _userManager.GetUserId(User);

                //verificam ca userul este in workspace + ca userul e moderator dar inca nu avem campul
                var userWorkspace = db.ApplicationUserWorkspaces
                                      .FirstOrDefault(uw => uw.WorkspaceId == channel.WorkspaceId && uw.UserId == userId && uw.moderator==true);

                if (userWorkspace == null)
                {
                    return Forbid();
                }
            }

            //verificam ca s-a trimis corect din formularul de editare
            if (ModelState.IsValid)
            {
                if(requestChannel.WorkspaceId != channel.WorkspaceId)
                {
                    return View(channel); //ne intoarcem in formular
                }

                //updatam campurile
                channel.Name = requestChannel.Name;
                channel.Type = requestChannel.Type;
                db.SaveChanges();

                TempData["message"] = "Channel updated successfully";
                TempData["messageType"] = "alert-success";

                return Redirect("/Workspaces/Show/" + channel.WorkspaceId);
            }
            else
            {
                return View(channel);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Delete(int id) {
            Channel channel = db.Channels.Include("Posts")
                             .Where(ch => ch.ChannelId == id)
                             .FirstOrDefault();

            if (channel == null) {
                return Forbid(); 
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Editor"))
            {
                var userId = _userManager.GetUserId(User);

                var userWorkspace = db.ApplicationUserWorkspaces
                                      .FirstOrDefault(uw => uw.WorkspaceId == channel.WorkspaceId && uw.UserId == userId && uw.moderator == true);

                if (userWorkspace == null)
                {
                    TempData["message"] = "You can't delete this channel";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Workspaces/Show/" + channel.WorkspaceId);
                }
            }

            db.Channels.Remove(channel);
            db.SaveChanges();

            TempData["message"] = "Channel deleted successfully";
            TempData["messageType"] = "alert-success";

            return Redirect("/Workspaces/Show/" + channel.WorkspaceId);
        }
    }
}
