using Proiect_DAW_DeliANN.Data;
using Proiect_DAW_DeliANN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Proiect_DAW_DeliANN.Models.ApplicationUserWorkspaces;

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

                foreach (var id in workspaceIds)
                {
                    Console.WriteLine(id);
                }

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

            return View();
        }
    }
}
