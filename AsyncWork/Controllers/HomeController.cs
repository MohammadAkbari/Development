using Microsoft.AspNetCore.Mvc;

namespace AsyncWork.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Test(TestDto dto)
        {
            return Ok();
        }

        public class TestDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
