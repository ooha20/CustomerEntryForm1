using Microsoft.AspNetCore.Mvc;

namespace DEMO.Controllers
{
    public class ContactController : Controller
    {
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Submit(string name, string email, string message)
        {
            // TODO: save to DB or send email
            TempData["ContactSuccess"] = "Thanks! We received your message.";
            return RedirectToAction("Contact");
        }
    }
}

