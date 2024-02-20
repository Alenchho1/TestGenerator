using Microsoft.AspNetCore.Mvc;
using TestGenerator.Data;

namespace TestGenerator.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected void SetAlert(string message, string type = "info")
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = type;
        }
    }
} 