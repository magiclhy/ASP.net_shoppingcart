using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.DBs;
using ShoppingCart.Models;

namespace ShoppingCart.Controllers
{
    public class LogoutController : Controller
    {
        private readonly DbShoppingCart _db;
        public LogoutController(DbShoppingCart db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            string sessionId = HttpContext.Request.Cookies["sessionId"];

            _db.Sessions.Remove(new Session()
            {
                Id = sessionId
            });
            _db.SaveChanges();

            HttpContext.Response.Cookies.Delete("sessionId");

            TempData["Alert"] = "primary|Successfully logged out!";
            return Redirect("/Gallery/Index");
        }
    }
}
