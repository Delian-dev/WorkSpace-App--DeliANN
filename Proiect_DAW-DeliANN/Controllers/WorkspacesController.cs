using Proiect_DAW_DeliANN.Data;
using Proiect_DAW_DeliANN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Proiect_DAW_DeliANN.Models.ApplicationUserWorkspaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ganss.Xss;

namespace Proiect_DAW_DeliANN.Controllers
{
    [Authorize]
    public class WorkspacesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public WorkspacesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
            {
                db = context;
                _userManager = userManager;
                _roleManager = roleManager;
            }

        //Afisarea tuturor Workspace-urilor impreuna cu categoria din care fac parte
        //Fiecare workspace va avea un buton de Request daca userul nu a incercat inca sa se alature
        //Daca se afla in asteptare (exista inregistrarea user_workspace in tabelul intermediar cu False), se va afisa un mesaj (Waiting for approval...)
        //Daca face parte din workspace, user-ul va putea intra sa vizualizeze continutul (Open workspace)
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            var workspaces = db.Workspaces.Include("Category")
                             .OrderByDescending(w => w.Date);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            //Mototul de cautare
            var search = "";
            
            //Se vor realiza cautari dupa: Numele workspace-ului, Categoria acestuia, dar si dupa descriere
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                //Cautare in workspace (nume,categorie,descriere)
                List<int> workspaceIds = db.Workspaces.Where(
                                         w => w.Name.Contains(search)
                                         || w.Description.Contains(search)
                                         || w.Category.CategoryName.Contains(search)
                                         ).Select(a => a.WorkspaceId).ToList();

                //lista workspace-urilor care contin cuvantul cautat (intr-unul din campurile vizate)
                workspaces = db.Workspaces.Where(workspace => workspaceIds.Contains(workspace.WorkspaceId))
                                          .Include("Category")
                                          .OrderByDescending(w => w.Date);
            }

            ViewBag.SearchString = search;

            //AFISARE PAGINATA

            //Sa zicem 4 workspace-uri pe pagina momentan
            int _perPage = 4;
            int totalItems = workspaces.Count(); //toate workspace-urile

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Workspaces/Index?page=valoare

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 4 =>  Asadar offsetul este egal cu numarul de workspace-uri care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            //Preluarea workspace-uri corespunzatoare pentru fiecare pagina la care ne aflam in functie de offset
            var paginatedWorkspaces = workspaces.Skip(offset).Take(_perPage);

            //Numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            //Trimitere workspace-uri catre View-ul corespunzator
            ViewBag.Workspaces = paginatedWorkspaces;

            //DACA AVEM AFISAREA PAGINATA IMPREUNA CU SEARCH
            if(search != "")
            {
                ViewBag.PaginationBaseUrl = "/Workspaces/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Workspaces/Index/?page";
            }

            var userId = _userManager.GetUserId(User); //trimitem in viewbag id-ul userului curent si toate campurile WorkSpaceId si status din tabelul asociativ
            var userWorkspaceRelations = db.ApplicationUserWorkspaces //daca trimiteam tot tabelul nu mergea in view sa accesez campurile for some reason
                                         .Where(uw => uw.UserId == userId)
                                         .Select (uw => new {uw.WorkspaceId, uw.status})
                                         .ToList();

            ViewBag.UserWorkspaceRelations = userWorkspaceRelations;

            SetUserWorkspaceStatus(); //functie care identifica toate perspectivele posibile pe care un user le poatea avea fata de un workspace
                                      //pe baza acestei perspective se vor afisa butoane/mesaje corespunzatoare pentru user

            return View();
        }


        [Authorize(Roles = "User")] //Afisarea tuturor workspace-urilor din care userul face parte sau pentru care este in asteptare
        public IActionResult Index_personalizat_user()
        {
            var userId = _userManager.GetUserId(User); //id-ul userului curent

            var filteredWorkspaces = db.Workspaces.Include("Category") //selectam doar workspace-urile in care exista o legatura in tabelul asociativ intr user si acesta
                                                  .Where(w => db.ApplicationUserWorkspaces
                                                            .Any(uw => uw.WorkspaceId == w.WorkspaceId && uw.UserId == userId))
                                                  .OrderByDescending(w => w.Date);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            //Mototul de cautare
            var search = "";

            //Se vor realiza cautari dupa: Numele workspace-ului, Categoria acestuia, dar si dupa descriere
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                //Cautare in workspace (nume,categorie,descriere)
                List<int> workspaceIds = db.Workspaces.Where(
                                         w => w.Name.Contains(search)
                                         || w.Description.Contains(search)
                                         || w.Category.CategoryName.Contains(search)
                                         ).Select(a => a.WorkspaceId).ToList();

                //lista workspace-urilor care contin cuvantul cautat (intr-unul din campurile vizate)
                filteredWorkspaces = db.Workspaces.Where(workspace => workspaceIds.Contains(workspace.WorkspaceId))
                                          .Include("Category")
                                          .Where(w => db.ApplicationUserWorkspaces
                                                            .Any(uw => uw.WorkspaceId == w.WorkspaceId && uw.UserId == userId))
                                          .OrderByDescending(w => w.Date);
            }

            ViewBag.SearchString = search;

            //AFISARE PAGINATA

            //Sa zicem 4 workspace-uri pe pagina momentan
            int _perPage = 4;
            int totalItems = filteredWorkspaces.Count(); //toate workspace-urile

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Workspaces/Index?page=valoare

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 4 =>  Asadar offsetul este egal cu numarul de workspace-uri care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            //Preluarea workspace-uri corespunzatoare pentru fiecare pagina la care ne aflam in functie de offset
            var paginatedWorkspaces = filteredWorkspaces.Skip(offset).Take(_perPage);

            //Numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            //Trimitere workspace-uri catre View-ul corespunzator
            ViewBag.Workspaces = paginatedWorkspaces;

            //DACA AVEM AFISAREA PAGINATA IMPREUNA CU SEARCH
            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Workspaces/Index_personalizat_user/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Workspaces/Index_personalizat_user/?page";
            }

            //trimitem in viewbag id-ul userului curent si toate campurile WorkSpaceId si status din tabelul asociativ
            var userWorkspaceRelations = db.ApplicationUserWorkspaces //daca trimiteam tot tabelul nu mergea in view sa accesez campurile for some reason
                                         .Where(uw => uw.UserId == userId)
                                         .Select(uw => new { uw.WorkspaceId, uw.status })
                                         .ToList();

            ViewBag.UserWorkspaceRelations = userWorkspaceRelations;

            SetUserWorkspaceStatus(); //functie care identifica toate perspectivele posibile pe care un user le poatea avea fata de un workspace
                                      //pe baza acestei perspective se vor afisa butoane/mesaje corespunzatoare pentru user

            return View();
        }

        //anyone can create a new workspace
        [Authorize(Roles = "Admin, User, Editor")]
        public IActionResult New()
        {
            ///the GET part => we take the data for the new workspace
            Workspace workspace = new Workspace();
            //get all the categories for the dropdown
            workspace.Categ = GetAllCategories();
            return View(workspace);
        }
        ///Adding the new workspace in the db with the data we input 
        [HttpPost]
        [Authorize(Roles = "Admin, User, Editor")]
        public IActionResult New(Workspace workspace)
        {
            //probabil aici va trebui sa si cream un channel default atunci cand da new, dar asta o sa fie deja la gandire de channel
            var sanitizer = new HtmlSanitizer();
            workspace.Date = DateTime.Now;
            ///preluam Id-ul utilizatorului care creeaza workspace-ul
            workspace.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                //sanitizing both the name and the description
                //even though the name is much shorter, you never know what these people write
                //better safe than sorry
                workspace.Description = sanitizer.Sanitize(workspace.Description);
                workspace.Name = sanitizer.Sanitize(workspace.Name);
                ///adding to the db
                db.Workspaces.Add(workspace);
                db.SaveChanges();

                //sa nu uit aici sa pun chestia cu status ca e automata!!!!!!!!!!
                //cream si inserarea in tabelul asociativ - status=true, moderator=true
                var userWorkspace = new ApplicationUserWorkspace
                {
                    UserId = workspace.UserId,
                    WorkspaceId = workspace.WorkspaceId,
                    status = true,
                    moderator = true
                };

                //adaugare in tabelul asociativ
                db.ApplicationUserWorkspaces.Add(userWorkspace);
                db.SaveChanges();

                //message to show all went smoothly
                TempData["message"] = "The new workspace has been added!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                //we sent something that didn't fit our regulations
                //it will send you back to the begining, addding the dropdown once again
                workspace.Categ = GetAllCategories();
                return View(workspace);
            }
        }
        //editing the workspace
        ///category is in a dropdown
        ///anita da-o in mama ei de engleza macar comentariile sa fie in romana:)))
        ///se poate edita un workspace daca esti: Admin/Editor sau User moderator
        ///automatically GET
        [Authorize(Roles = "Admin, User, Editor")]
        public IActionResult Edit(int id)
        {
            //same here, probabil va putea sa se joace si cu channels tinand cont ca tin de workspace
            //gen ordinea lor, all that stuff
            Workspace workspace = db.Workspaces.Include("Channels")
                                      .Where(wrk => wrk.WorkspaceId == id)
                                      .First();
            workspace.Categ = GetAllCategories();

            var userId = _userManager.GetUserId(User); //id-ul userului curent
            var isAdminorEditor = User.IsInRole("Admin") || User.IsInRole("Editor");

            //verificam daca user-ul care vrea sa editeze este moderator in workspace
            var isModeratorInWorkspace = db.ApplicationUserWorkspaces
                                           .Where(u => u.WorkspaceId == id && u.UserId == userId && u.moderator == true)
                                           .Any();

            if(isAdminorEditor || isModeratorInWorkspace) //nu are rost sa verificam daca userId==workspace.UserId pt ca acesta oricum e bagat by default cu moderator=true cand se creeaza workspace-ul (sper)
            {
                return View(workspace); //userul curent are voie sa editeze
            }

            else
            {

                TempData["message"] = "You are not allowed to edit a workspace that does not belong to you!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        ///addding the edits to the database
        [HttpPost]
        [Authorize(Roles = "Admin, User, Editor")]
        public IActionResult Edit(int id, Workspace requestWorkspace)
        {
            ///same as the new
            var sanitizer = new HtmlSanitizer();
            Workspace workspace = db.Workspaces.Find(id);
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User); //id-ul userului curent
                var isAdminorEditor = User.IsInRole("Admin") || User.IsInRole("Editor");

                //verificam daca user-ul care vrea sa editeze este moderator in workspace
                var isModeratorInWorkspace = db.ApplicationUserWorkspaces
                                               .Where(u => u.WorkspaceId == id && u.UserId == userId && u.moderator == true)
                                               .Any();

                if (isAdminorEditor || isModeratorInWorkspace)
                {
                    workspace.Date = DateTime.Now;
                    workspace.CategoryId = requestWorkspace.CategoryId;
                    workspace.Name = requestWorkspace.Name;
                    workspace.Description = requestWorkspace.Description;
                    TempData["message"] = "Workspace has been edited.";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You are not allowed to edit a workspace that does not belong to you!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestWorkspace.Categ = GetAllCategories();
                return View(requestWorkspace);
            }
        }
        ///delete the workspace
        [HttpPost]
        [Authorize(Roles = "Admin, User, Editor")]
        public IActionResult Delete(int id)
        {
            ///deleting the channels that are associated to the workspace as well
            Workspace workspace = db.Workspaces.Include("Channels")
                                                .Where(wrk => wrk.WorkspaceId == id)
                                                .First();

            var userId = _userManager.GetUserId(User); //id-ul userului curent
            var isAdminorEditor = User.IsInRole("Admin") || User.IsInRole("Editor");

            //verificam daca user-ul care vrea sa stearga este moderator in workspace
            var isModeratorInWorkspace = db.ApplicationUserWorkspaces
                                           .Where(u => u.WorkspaceId == id && u.UserId == userId && u.moderator == true)
                                           .Any();

            if (isAdminorEditor || isModeratorInWorkspace)
            {
                db.Workspaces.Remove(workspace);
                db.SaveChanges();
                TempData["message"] = "The Workspace has been deleted.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You are not allowed to delete a workspace that does not belong to you!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        //this is for the dropdown => it makes a list of all categories
        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName
                });
            }

            return selectList;
        }
        //[NonAction]
        //public IEnumerable<SelectListItem> GetAllUsers()
        //{
        //    // generam o lista de tipul SelectListItem fara elemente
        //    var selectList = new List<SelectListItem>();

        //    // extragem toate categoriile din baza de date
        //    var users = from cat in db.Users
        //                     select cat;

        //    // iteram prin categorii
        //    foreach (var user in users)
        //    {
        //        // adaugam in lista elementele necesare pentru dropdown
        //        // id-ul categoriei si denumirea acesteia
        //        selectList.Add(new SelectListItem
        //        {
        //            Value = user.UserId.ToString(),
        //            Text = user.CategoryName
        //        });
        //    }

        //    return selectList;
        //}
        //Afisarea unui Workpace => se alege dupa id din Index
        //Ce vedem aici? => Butonul de edit si delete, numele si descrierea 
        //Lista tuturor user-ilor care sunt in workspace si lista tuturor channel-urilor
        //si cam atat, ca nu planuiesc momentan sa fac cu 
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            //includem tot ce se poate afisa
            //momentan, postarile nu, cu toate ca probabil vom edita sa intre pe prima default dupa ce le si avem
            Workspace workspace = db.Workspaces.Include("Category")
                                                .Include("ApplicationUserWorkspaces")
                                                .Include("Channels")
                                                .Include("User")
                                                .Where(wrk => wrk.WorkspaceId == id)
                                                .First();
            SetAccessRights(id); //setam privilegiile pt workspace-ul pe care vrem sa intram

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(workspace);
        }

        //Functia cu ajutorul careia cream view-ul pt moderator unde poate vedea toti userii care au dat request join pentru workspace
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ShowRequests(int id) {
            
            var userId = _userManager.GetUserId(User); //id-ul userului curent
            var isAdminorEditor = User.IsInRole("Admin") || User.IsInRole("Editor");

            //verificam daca user-ul care vrea sa stearga este moderator in workspace
            var isModeratorInWorkspace = db.ApplicationUserWorkspaces
                                           .Where(u => u.WorkspaceId == id && u.UserId == userId && u.moderator == true)
                                           .Any();

            if(isAdminorEditor || isModeratorInWorkspace)
            {
                //selectam userii care au status = false raportat la acest workspace (=> asteapta sa fie aprobati)
                var pendingUsers = db.ApplicationUserWorkspaces
                                   .Where(u => u.WorkspaceId == id && u.status == false)
                                   .Select(u => new {  //selectam doar campurile pe care le vrem (ca sa nu selectam absolut toate prostiile din User)
                                       u.WorkspaceId, u.UserId, //aici ar mai avea logic sa selectam si poza user-ului de profil pt cand vom avea acel camp
                                       u.status, u.User.UserName
                                       }
                                   ).ToList().Cast<object>(); // Trebuie castate la ceva de tip object ca sa mearga (altfel am fi facut o clasa separat cu tipul de campuri pe care le selectam)
                                                              //Ori ar fi trebuit sa selectam tot ce are User in spate si its kinda overkill
                return View(pendingUsers);
            }

            else
            {
                return Forbid();
            }
        }

        // Conditiile de afisare pentru butoanele de editare si stergere
        // butoanele aflate in view-uri
        //adica editor + admin 100% le vad
        //dar si user-ul care a creat workspace-ul le vede (restul nu)
        private void SetAccessRights(int workspaceId)
        {
            ViewBag.AfisareButoane = false;

            string currentUserId = _userManager.GetUserId(User); //afisam butoanele daca userul e Admin/Editor/Moderator pe workspace
            bool isModerator = db.ApplicationUserWorkspaces
                              .Any(auw => auw.UserId == currentUserId && auw.WorkspaceId == workspaceId && auw.moderator == true);

            ViewBag.EsteModerator = isModerator;
            if (User.IsInRole("Editor") || User.IsInRole("Admin") || isModerator)
            {
                ViewBag.AfisareButoane = true;
            }

            //de astea nush daca mai avem nevoie
            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }


        [HttpPost]
        [Authorize(Roles ="User")]
        public IActionResult RequestToJoin(int workspaceId) //functie apelata cand se da request de join la un workspace
        {
            var userId = _userManager.GetUserId(User); //id-ul userului curent

            var newRelation = new ApplicationUserWorkspace //construim noua legatura
            {
                UserId = userId,
                WorkspaceId = workspaceId,
                status = false,
                moderator=null
            };

            db.ApplicationUserWorkspaces.Add(newRelation); //updatam tabelul asociativ
            db.SaveChanges();
            TempData["message"] = "Request to join submitted!"; //mesaj pt confirmare
            TempData["messageType"] = "alert-succes";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult LeaveWorkspace(int workspaceId) //functie apelata cand se da cancel la request 
        {
            var userId = _userManager.GetUserId(User);//id-ul userului curent

            //gasim inregistrarea din tabel
            var relation = db.ApplicationUserWorkspaces
                           .FirstOrDefault(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId);

            if (relation != null && relation.status)
            {
                //stergem inregistrarea
                db.ApplicationUserWorkspaces.Remove(relation);
                db.SaveChanges();
                TempData["message"] = "You Left the Workspace!"; //mesaj pt confirmare
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult CancelRequest(int workspaceId) //functie apelata cand se da cancel la request 
        {
            var userId = _userManager.GetUserId(User);//id-ul userului curent

            //gasim inregistrarea din tabel
            var relation = db.ApplicationUserWorkspaces
                           .FirstOrDefault(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId);

            if (relation != null && !relation.status)
            {
                //stergem inregistrarea
                db.ApplicationUserWorkspaces.Remove(relation);
                db.SaveChanges();
                TempData["message"] = "Request to join cancelled!"; //mesaj pt confirmare
                TempData["messageType"] = "alert-warning";
            }

            return RedirectToAction("Index");
        }

        private void SetUserWorkspaceStatus() 
        {
            ViewBag.AfisButonRequest = true; //n-am avut nevoie de astea 3 butoane pt ca le am rezolvat direct in view da cine stie poate o sa fie util mai lasa-le aici
            ViewBag.AfisButonCancelRequest = false;
            ViewBag.AfisButonEnter = false;

            if (User.IsInRole("Admin") || User.IsInRole("Editor"))
            {
                ViewBag.AfisButonRequest = false;
                ViewBag.AfisButonEnter = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User); 
            ViewBag.EsteAdmin = User.IsInRole("Admin"); //salvam si in variabile daca este admin/este editor -> o sa ne trebuiasca sa stim la functii gen show/new/edit/delete cine are drepturi sa vada anumite functionalitati/butoane
            ViewBag.EsteEditor = User.IsInRole("Editor"); //editor adica moderatorul suprem, nu un user care e moderator pe un anumit workspace
            //astea 2 variabile le-am folosit in view-ul index
        }
    }
}
