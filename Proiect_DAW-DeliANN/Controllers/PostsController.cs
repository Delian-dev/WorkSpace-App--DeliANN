using Microsoft.AspNetCore.Mvc;

namespace Proiect_DAW_DeliANN.Controllers
{
    public class PostsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
