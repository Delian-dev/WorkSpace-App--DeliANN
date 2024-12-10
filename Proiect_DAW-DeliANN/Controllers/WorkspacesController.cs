using Microsoft.AspNetCore.Mvc;

namespace Proiect_DAW_DeliANN.Controllers
{
    public class WorkspacesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
