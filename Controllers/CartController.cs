//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.Json;
//using Microsoft.AspNetCore.Mvc;
//using ShoppingCart.DBs;
//using ShoppingCart.Models;

//namespace ShoppingCart.Controllers
//{
//    public class CartController : Controller
//    {
//        private readonly DbShoppingCart _db;
//        private readonly Verify _v;

//        public CartController(DbShoppingCart db, Verify v)
//        {
//            _db = db;
//            _v = v;
//        }

//        public IActionResult Index()
//        {
//            var sessionId = HttpContext.Request.Cookies["sessionId"];
//            var carts = GetCarts(sessionId);

//            SetViewDataForCarts(carts);

//            return View(carts);
//        }

//        private List<Cart> GetCarts(string sessionId)
//        {
//            if (_v.VerifySession(sessionId, _db))
//            {
//                ViewData["Logged"] = true;
//                var user = _db.Sessions.FirstOrDefault(s => s.Id == sessionId)?.User;
//                ViewData["Username"] = user?.Username;

//                return _db.Carts.Where(x => x.UserId == user.Id).ToList();
//            }
//            else
//            {
//                var cartCookie = HttpContext.Request.Cookies["guestCart"];
//                if (cartCookie != null)
//                {
//                    var guestCart = JsonSerializer.Deserialize<GuestCart>(cartCookie);
//                    return guestCart.LoadProducts(_db);
//                }
//            }
//            return new List<Cart>();
//        }

//        private void SetViewDataForCarts(List<Cart> carts)
//        {
//            if (carts.Count == 0)
//            {
//                ViewData["CartQuantity"] = 0;
//                ViewData["Total"] = 0;
//            }
//            else
//            {
//                ViewData["CartQuantity"] = carts.Sum(cart => cart.Quantity);
//                ViewData["ItemsInCart"] = carts;
//                ViewData["Total"] = carts.Sum(cart => cart.Quantity * cart.Product.Price);
//            }
//        }

//        public IActionResult Checkout()
//        {
//            var sessionId = HttpContext.Request.Cookies["sessionId"];
//            if (!_v.VerifySession(sessionId, _db))
//            {
//                TempData["Alert"] = "danger|Please login to check out";
//                return Redirect("/Login/Index");
//            }

//            var carts = _db.Carts.Include(c => c.Product).Where(c => c.UserId == _db.Sessions.FirstOrDefault(s => s.Id == sessionId).UserId).ToList();
//            if (carts.Any(c => c.Quantity > c.Product.Quantity))
//            {
//                var outOfStockProduct = carts.First(c => c.Quantity > c.Product.Quantity);
//                TempData["Alert"] = $"danger|{outOfStockProduct.Product.Name} out of stock";
//                return Redirect("/Cart/Index");
//            }

//            var order = CreateOrder(carts, sessionId);
//            _db.SaveChanges();
//            TempData["Alert"] = "primary|Successful checkout";

//            return Redirect("/Purchase");
//        }

//        private Order CreateOrder(List<Cart> carts, string sessionId)
//        {
//            var order = new Order { UserId = _db.Sessions.FirstOrDefault(s => s.Id == sessionId).UserId, DateTime = DateTime.Now };
//            foreach (var cart in carts)
//            {
//                cart.Product.Quantity -= cart.Quantity;
//                for (int i = 0; i < cart.Quantity; i++)
//                {
//                    _db.OrderDetails.Add(new OrderDetail { Order = order, ProductId = cart.ProductId });
//                }
//                _db.Carts.Remove(cart);
//            }
//            return order;
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.DBs;
using ShoppingCart.Models;

namespace ShoppingCart.Controllers
{
    public class CartController : Controller
    {
        //private readonly DbShoppingCart _db;
        private DbShoppingCart _db;
        private Verify _v;

        public CartController(DbShoppingCart _db, Verify v)
        {
            this._db = _db;
            _v = v;
        }
        public IActionResult Index()
        {
            string sessionId = HttpContext.Request.Cookies["sessionId"];
            List<Cart> carts = new List<Cart>();
            //无效session 
            if (_v.VerifySession(sessionId, _db))
            {
                ViewData["Logged"] = true;
                User user = _db.Sessions.FirstOrDefault(s => s.Id == sessionId).User;

                ViewData["Username"] = user.Username;
                //展示购物车
                carts = _db.Carts.Where(x => x.UserId == user.Id).ToList();
            }
            else
            {
                //未登陆，cookie获取购物车信息
                string cartCookie = HttpContext.Request.Cookies["guestCart"];
                if (cartCookie != null)
                {
                    var guestCart = JsonSerializer.Deserialize<GuestCart>(HttpContext.Request.Cookies["guestCart"]);
                    carts = guestCart.LoadProducts(_db);
                }
            }

            if (carts.Count == 0)
            {
                ViewData["CartQuantity"] = 0;
                ViewData["Total"] = 0;
                carts = null;
            }
            else
            {
                ViewData["CartQuantity"] = carts.Sum(cart => cart.Quantity);
                ViewData["ItemsInCart"] = carts;
                ViewData["Total"] = carts.Sum(cart => cart.Quantity * cart.Product.Price);
            }

            return View(carts);

        }

        //public IActionResult Index()
        //{
        //    string sessionId = HttpContext.Request.Cookies["sessionId"];
        //    List<Cart> carts = new List<Cart>();

        //    if (carts.Count == 0)
        //    {
        //        ViewData["CartQuantity"] = 0;
        //        ViewData["Total"] = 0;
        //        carts = null;
        //    }
        //    else
        //    {
        //        ViewData["CartQuantity"] = carts.Sum(cart => cart.Quantity);
        //        ViewData["ItemsInCart"] = carts;
        //        ViewData["Total"] = carts.Sum(cart => cart.Quantity * cart.Product.Price);
        //    }

        //    return View(carts);

        //}

        public IActionResult Checkout(Hasher h)
        {
            string sessionId = HttpContext.Request.Cookies["sessionId"];

            if (_v.VerifySession(sessionId, _db))
            {
                User user = _db.Sessions.FirstOrDefault(session => session.Id == sessionId).User;
                List<Cart> carts = _db.Carts.Where(cart => cart.UserId == user.Id).ToList();
                bool lackStack = false;
                string productName = string.Empty;

                var order = new Order
                {
                    UserId = user.Id,
                    DateTime = DateTime.Now
                };

                foreach (Cart c in carts)
                {
                    //遍历库存
                    Product product = _db.Products.FirstOrDefault(p => p.Id == c.ProductId);
                    if (product.Quantity < c.Quantity) {
                        lackStack = true;
                        productName = product.Name;
                        break;
                    }

                    int quntity = product.Quantity - c.Quantity;
                    product.Quantity = quntity < 0 ? 0 : quntity;
                    _db.SaveChanges();

                    for (int i = 0; i < c.Quantity; i++)
                    {
                        var orderDetail = new OrderDetail
                        {
                            Id = h.GenerateActivationKey(_db),
                            Order = order,
                            ProductId = c.ProductId
                        };
                        _db.OrderDetails.Add(orderDetail);
                    }                   

                    _db.Carts.Remove(c);
                }

                if (lackStack) {
                    //TempData是ASP.NET MVC框架中用于在连续的请求之间传递数据的一种机制
                    //"Alert"是存储在TempData字典中的键名
                    TempData["Alert"] = "danger|"+ productName + " out of stock";
                    return Redirect("/Cart/Index");                    
                }

                _db.SaveChanges();
                TempData["Alert"] = "primary|Successful checkout";

            }
            //登陆结账
            else
            {
                TempData["ReturnUrl"] = "/Cart/Index";
                TempData["Alert"] = "danger|please login to check out";
                return Redirect("/Login/Index");
            }
            return Redirect("/Purchase");
        }

        [HttpPost]
        //post请求更新购物车商品数量
        public JsonResult Update(int productId, int quantity)
        {
            string sessionId = HttpContext.Request.Cookies["sessionId"];
            string newPrice = string.Empty;
            string totalPrice = string.Empty;
            string stockError = string.Empty;
            bool lackStack = false;

            if (_v.VerifySession(sessionId, _db))
            {
                Product product = _db.Products.FirstOrDefault(p => p.Id == productId);
                if (product.Quantity >= quantity)
                {
                    User user = _db.Sessions.FirstOrDefault(session => session.Id == sessionId).User;
                    Cart cart = _db.Carts.FirstOrDefault(cart => cart.UserId == user.Id && cart.ProductId == productId);
                    cart.Quantity = quantity;
                    _db.SaveChanges();

                    newPrice = (cart.Quantity * cart.Product.Price).ToString();
                    totalPrice = (_db.Carts.Where(cart => cart.UserId == user.Id).Sum(cart => cart.Quantity * cart.Product.Price)).ToString();
                }
                else
                {
                    lackStack = true;
                    TempData["Alert"] = "warning|Out of stock";
                    return Json(new
                    {
                        success = false,
                    });
                }

            }
            else
            {
                Product prod = _db.Products.FirstOrDefault(p => p.Id == productId);
                if (prod.Quantity >= quantity)
                {
                    var guestCart = JsonSerializer.Deserialize<GuestCart>(HttpContext.Request.Cookies["guestCart"]);
                    double priceSum = 0;

                    foreach (var product in guestCart.Products)
                    {
                        if (product.ProductId == productId)
                        {
                            product.Quantity = quantity;
                        }
                        priceSum += _db.Products.FirstOrDefault(p => p.Id == product.ProductId).Price * product.Quantity;
                    }
                    HttpContext.Response.Cookies.Append("guestCart", JsonSerializer.Serialize<GuestCart>(guestCart));

                    newPrice = (_db.Products.FirstOrDefault(product => product.Id == productId).Price * quantity).ToString();
                    totalPrice = priceSum.ToString();
                }
                else {
                    lackStack = true;
                    TempData["Alert"] = "warning|Out of stock!";
                    return Json(new
                    {
                        success = false,
                    });
                }
            }

            return Json(new
            {
                success = true,
                newPrice,
                totalPrice
            });
        }

        [HttpPost]
        public JsonResult Remove(int productId, int row)
        {
            string sessionId = HttpContext.Request.Cookies["sessionId"];
            string totalPrice;

            if (_v.VerifySession(sessionId, _db))
            {
                User user = _db.Sessions.FirstOrDefault(session => session.Id == sessionId).User;
                Cart cart = _db.Carts.FirstOrDefault(cart => cart.UserId == user.Id && cart.ProductId == productId);

                _db.Carts.Remove(cart);

                _db.SaveChanges();
                totalPrice = (_db.Carts.Where(cart => cart.UserId == user.Id).Sum(cart => cart.Quantity * cart.Product.Price)).ToString();
            }
            else
            {
                var guestCart = JsonSerializer.Deserialize<GuestCart>(HttpContext.Request.Cookies["guestCart"]);
                double priceSum = 0;
                Cart productToRemove = null;

                foreach (var product in guestCart.Products)
                {
                    if (product.ProductId == productId)
                    {
                        productToRemove = product;
                        continue;
                    }
                    priceSum += _db.Products.FirstOrDefault(p => p.Id == product.ProductId).Price * product.Quantity;
                }

                if (productToRemove != null)
                {
                    guestCart.Products.Remove(productToRemove);
                }

                HttpContext.Response.Cookies.Append("guestCart", JsonSerializer.Serialize<GuestCart>(guestCart));
                totalPrice = priceSum.ToString();
            }

            return Json(new
            {
                success = true,
                totalPrice,
            });
        }

    }
}
