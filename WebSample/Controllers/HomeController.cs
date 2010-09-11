using System.Web.Mvc;
using WebSample.Models;

namespace WebSample.Controllers {

    public class HomeController : Controller {

        public ActionResult Index() {
            var patients = Patient.All();
            return View(patients);
        }
    }
}
