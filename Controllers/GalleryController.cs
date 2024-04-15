using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShoppingCart.DBs;
using ShoppingCart.Models;

//namespace ShoppingCart.Controllers
//{
//    public class GalleryController : Controller
//    {
//        private readonly DbShoppingCart _db;
//        private readonly Verify _v;

//        public GalleryController(DbShoppingCart db, Verify v)
//        {
//            _db = db;
//            _v = v;
//        }

//        public IActionResult Index(string search, int page = 1, int pageSize = 10)
//        {
//            var sessionId = HttpContext.Request.Cookies["sessionId"];
//            User user = GetUserFromSession(sessionId);
//            ViewData["CartQuantity"] = CalculateCartQuantity(sessionId);

//            var products = GetProducts(search, page, pageSize);

//            var galleryView = new GalleryViewModel(user, page, products);
//            ViewData["Searchbar"] = search;

//            return View(galleryView);
//        }

//        private User GetUserFromSession(string sessionId)
//        {
//            if (string.IsNullOrEmpty(sessionId)) return null;

//            if (_v.VerifySession(sessionId, _db))
//            {
//                ViewData["Logged"] = true;
//                return _db.Sessions.Include(s => s.User)
//                          .FirstOrDefault(x => x.Id == sessionId)?.User;
//            }
//            return null;
//        }

//        private int CalculateCartQuantity(string sessionId)
//        {
//            if (string.IsNullOrEmpty(sessionId))
//            {
//                var cartCookie = HttpContext.Request.Cookies["guestCart"];
//                return string.IsNullOrEmpty(cartCookie) ? 0 : JsonSerializer.Deserialize<GuestCart>(cartCookie).Count();
//            }
//            else
//            {
//                var userId = _db.Sessions.FirstOrDefault(x => x.Id == sessionId)?.UserId;
//                return _db.Carts.Where(x => x.UserId == userId).Sum(cart => cart.Quantity);
//            }
//        }

//        private IEnumerable<Product> GetProducts(string search, int page, int pageSize)
//        {
//            var query = _db.Products.AsQueryable();
//            if (!string.IsNullOrEmpty(search))
//            {
//                query = query.Where(x => x.Name.Contains(search));
//            }
//            return query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).ToList();
//        }
//    }
//}

namespace ShoppingCart.Controllers
{
    public class GalleryController : Controller
    {
        private readonly DbShoppingCart _db;
        private readonly Verify _v;

        public GalleryController(DbShoppingCart db, Verify v)
        {
            _db = db;
            _v = v;
        }
        public IActionResult Index(string search, int page = 1)
        {
            string sessionId = HttpContext.Request.Cookies["sessionId"];
            User user = null;

            if (_v.VerifySession(sessionId, _db))
            {
                ViewData["Logged"] = true;
                user = _db.Sessions.FirstOrDefault(x => x.Id == sessionId).User;

                ViewData["Username"] = user.Username;

                List<Cart> carts = _db.Carts.Where(x => x.UserId == user.Id).ToList();
                ViewData["CartQuantity"] = carts.Sum(cart => cart.Quantity);
            }
            else
            {
                string cartCookie = HttpContext.Request.Cookies["guestCart"];
                if (cartCookie == null)
                {
                    ViewData["CartQuantity"] = 0;
                }
                else
                {
                    var guestCart = JsonSerializer.Deserialize<GuestCart>(HttpContext.Request.Cookies["guestCart"]);
                    ViewData["CartQuantity"] = guestCart.Count();
                }
            }

            List<Product> products = new List<Product>();

            //search字符串搜索
            if (string.IsNullOrEmpty(search))
            {
                products = _db.Products.OrderBy(x => x.Name).ToList();
            }
            else
            {
                products = _db.Products.Where(x => x.Name.Contains(search)).OrderBy(x => x.Name).ToList();
            }

            var galleryView = new GalleryViewModel(user, page, products);

            ViewData["Searchbar"] = search;

            return View(galleryView);
        }

        [HttpPost]
        public IActionResult AddCart(int productId)
        {
            string sessionId = HttpContext.Request.Cookies["sessionId"];

            if (_v.VerifySession(sessionId, _db))
            {
                int userid = _db.Sessions.FirstOrDefault(x => x.Id == sessionId).UserId;
                Cart cart = _db.Carts.FirstOrDefault(x => x.UserId == userid && x.ProductId == productId);
                Product prod = _db.Products.FirstOrDefault(p => p.Id == productId);
                //检查库存
                if ((cart != null && cart.Quantity+1 > prod.Quantity) || (cart == null && prod.Quantity < 1))
                {
                    TempData["Alert"] = "warning|Out of stock";
                    return Json(new
                    {
                        success = false,
                    });
                }

                if (cart == null)
                {
                    _db.Add(new Cart()
                    {
                        Quantity = 1,
                        UserId = userid,
                        ProductId = productId
                    });
                }
                else 
                {
                    cart.Quantity += 1;
                }                

                _db.SaveChanges();

                return Json(new
                {
                    success = true
                });
            }
            else
            {
                string cartCookie = HttpContext.Request.Cookies["guestCart"];
                GuestCart guestCart;
                if (cartCookie != null)
                {
                    guestCart = JsonSerializer.Deserialize<GuestCart>(cartCookie);
                }
                else
                {
                    guestCart = new GuestCart();
                    guestCart.Add(productId, _db.Products.FirstOrDefault(p => p.Id == productId));
                }

                Product product = _db.Products.FirstOrDefault(p => p.Id == productId);
                Cart inCart = guestCart.Find(productId);

                if ((inCart != null && inCart.Quantity > product.Quantity) || (inCart == null && product.Quantity < 1))
                {
                    TempData["Alert"] = "warning|Out of stock!";
                    return Json(new
                    {
                        success = false,
                    });
                }

                
                if (inCart == null)
                {
                    guestCart.Add(productId, product);
                }                

                HttpContext.Response.Cookies.Append("guestCart", JsonSerializer.Serialize<GuestCart>(guestCart), new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });

                return Json(new
                {
                    success = true
                });
            }
        }
        
    }
}
