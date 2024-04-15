//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.AspNetCore.Mvc;
//using ShoppingCart.DBs;
//using ShoppingCart.Models;

//namespace ShoppingCart.Controllers
//{
//    public class PurchaseController : Controller
//    {
//        private readonly DbShoppingCart _db;

//        public PurchaseController(DbShoppingCart db)
//        {
//            _db = db;
//        }

//        public IActionResult Index(Verify v)
//        {
//            var sessionId = Request.Cookies["sessionId"];
//            if (!v.VerifySession(sessionId, _db))
//            {
//                TempData["Alert"] = "primary|Please log in to view your purchases.";
//                TempData["ReturnUrl"] = "/Purchase/Index";
//                return RedirectToAction("Index", "Login");
//            }

//            ViewData["Logged"] = true;
//            var user = _db.Sessions.FirstOrDefault(x => x.Id == sessionId)?.User;
//            ViewData["Username"] = user.Username;

//            var purchases = _db.Orders
//                .Where(o => o.UserId == user.Id)
//                .SelectMany(o => o.OrderDetails)
//                .Select(od => new
//                {
//                    od.Order.DateTime,
//                    od.Id,
//                    od.Product.ImageLink,
//                    od.Product.Name,
//                    od.Product.Description,
//                    od.ProductId
//                })
//                .AsEnumerable()
//                .GroupBy(x => new { x.ImageLink, x.Name, x.Description, x.ProductId })
//                .Select(g => new PurchasesViewModel
//                {
//                    DateTime = g.Select(x => x.DateTime).ToList(),
//                    ImageLink = g.Key.ImageLink,
//                    Name = g.Key.Name,
//                    Quantity = g.Count(),
//                    Description = g.Key.Description,
//                    ActivationCode = g.Select(x => x.Id).ToList(),
//                    ProductId = g.Key.ProductId
//                })
//                .ToList();

//            ViewData["HavePastOrders"] = purchases.Any();

//            return View(purchases);
//        }

//        public IActionResult UpdatePurchaseDate([FromBody] string selectedCode)
//        {
//            var orderDetail = _db.OrderDetails.FirstOrDefault(x => x.Id == selectedCode);

//            var purchaseDate = orderDetail?.Order.DateTime.ToString("d MMM yyyy");
//            var productId = orderDetail?.ProductId ?? 0;

//            return Json(new
//            {
//                PurchaseDate = purchaseDate,
//                ProductId = productId
//            });
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.DBs;
using ShoppingCart.Models;

namespace ShoppingCart.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly DbShoppingCart _db;

        public PurchaseController(DbShoppingCart db)
        {
            _db = db;
        }

        public IActionResult Index(Verify v)
        {
            // session
            string sessionId = Request.Cookies["sessionId"];

            if (v.VerifySession(sessionId, _db)) 
            {
                // Logout 按钮
                ViewData["Logged"] = true;

                User user = _db.Sessions.FirstOrDefault(x => x.Id == sessionId).User;

                ViewData["Username"] = user.Username;

                // 联合查询
                List<OrderDetail> orderDetail = _db.OrderDetails.ToList();
                List<Order> order = _db.Orders.ToList();
                List<Product> product = _db.Products.ToList();

                IEnumerable<PurchasesViewModel> purchases =
                    from o in order
                    join od in orderDetail on o.Id equals od.OrderId
                    join p in product on od.ProductId equals p.Id
                    where o.UserId == user.Id
                    orderby o.DateTime descending
                    select new { o.DateTime, od.Id, p.ImageLink, p.Name, p.Description, od.ProductId} into y
                    group y by new { y.ImageLink, y.Name, y.Description, y.ProductId } into grp
                    select new PurchasesViewModel
                    {
                        DateTime = grp.Select(x => x.DateTime).ToList(),
                        ImageLink = grp.Key.ImageLink,
                        Name = grp.Key.Name,
                        Quantity = grp.Count(),
                        Description = grp.Key.Description,
                        ActivationCode = grp.Select(x => x.Id).ToList(),
                        ProductId = grp.Key.ProductId
                    };

                if (purchases.ToList().Count == 0) 
                {
                    ViewData["HavePastOrders"] = false;

                    return View();
                }

                ViewData["HavePastOrders"] = true;

                return View(purchases.ToList());
            }
            else 
            {
                TempData["Alert"] = "primary|Please log in to view your purchases.";
                TempData["ReturnUrl"] = "/Purchase/Index";

                return RedirectToAction("Index", "Login");
            }
        }

        //public IActionResult UpdatePurchaseDate([FromBody] string selectedCode)
        //{
        //    OrderDetail orderDetail = _db.OrderDetails.FirstOrDefault(x => x.Id == selectedCode);

        //    string purchaseDate = orderDetail.Order.DateTime.ToString("d MMM yyyy");
        //    int productId = orderDetail.ProductId;

        //    return Json(new
        //    {
        //        PurchaseDate = purchaseDate,
        //        ProductId = productId
        //    });
        //}
    }
}
