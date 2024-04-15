//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using ShoppingCart.DBs;
//using ShoppingCart.Models;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;

//namespace ShoppingCart.Middlewares
//{
//    public class SessionKeeper
//    {
//        private readonly RequestDelegate next;

//        public SessionKeeper(RequestDelegate next)
//        {
//            this.next = next;
//        }

//        public async Task Invoke(HttpContext context, ITempDataProvider tdp, DbShoppingCart db)
//        {
//            string lastAccess = context.Request.Cookies["lastAccessTime"];
//            var now = DateTime.Now;

//            if (lastAccess == null)
//            {
//                SetLastAccessTime(context, now);
//            }
//            else
//            {
//                if (IsSessionExpired(lastAccess, now))
//                {
//                    await HandleSessionExpired(context, db, tdp);
//                    return;
//                }
//                else
//                {
//                    SetLastAccessTime(context, now);
//                }
//            }

//            await next(context);
//        }

//        private void SetLastAccessTime(HttpContext context, DateTime currentTime)
//        {
//            context.Response.Cookies.Append("lastAccessTime", currentTime.ToString(), new CookieOptions
//            {
//                HttpOnly = true,
//                SameSite = SameSiteMode.Lax
//            });
//        }

//        private bool IsSessionExpired(string lastAccess, DateTime currentTime)
//        {
//            DateTime lastAccessDateTime = Convert.ToDateTime(lastAccess);
//            return currentTime > lastAccessDateTime.AddMinutes(30);
//        }

//        private async Task HandleSessionExpired(HttpContext context, DbShoppingCart db, ITempDataProvider tdp)
//        {
//            context.Response.Cookies.Delete("lastAccessTime");

//            string sessionId = context.Request.Cookies["sessionId"];
//            if (sessionId != null)
//            {
//                var session = db.Sessions.FirstOrDefault(session => session.Id == sessionId);
//                if (session != null)
//                {
//                    db.Sessions.Remove(session);
//                    await db.SaveChangesAsync();
//                }
//            }

//            context.Response.Cookies.Delete("sessionId");
//            tdp.SaveTempData(context, new Dictionary<string, object> { ["Alert"] = "warning|Your session has timed-out!" });
//            context.Response.Redirect("/Gallery/Index");
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using ShoppingCart.DBs;
using ShoppingCart.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;


namespace ShoppingCart.Middlewares
{
    public class SessionKeeper
    {
        private readonly RequestDelegate next;

        public SessionKeeper (RequestDelegate next)
        {
            this.next = next;
            
        }

        public async Task Invoke(HttpContext context, ITempDataProvider tdp, DbShoppingCart db)
        {
            // 存在用户最后登陆时间
            string lastAccess = context.Request.Cookies["lastAccessTime"];

            if (lastAccess == null)
            {
                //新session
                context.Response.Cookies.Append("lastAccessTime", DateTime.Now.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });

            }
            else
            {
                // lastAccessTime存在时间
                DateTime lastAccessDateTime = Convert.ToDateTime(lastAccess);

                if (DateTime.Now.CompareTo(lastAccessDateTime.AddMinutes(30)) == 1)
                {
                    context.Response.Cookies.Delete("lastAccessTime");

                    string sessionId = context.Request.Cookies["sessionId"];

                    if (sessionId != null)
                    {
                        var session = db.Sessions.FirstOrDefault(session => session.Id == sessionId);

                        if (session != null)
                        {
                            db.Sessions.Remove(session);
                            db.SaveChanges();
                        }
                    }

                    context.Response.Cookies.Delete("sessionId");

                    tdp.SaveTempData(context, new Dictionary<string, object> { ["Alert"] = "warning|Your session is timed-out" });

                    context.Response.Redirect("/Gallery/Index");

                    return;
                } 
                else
                {
                    // 更新时间戳
                    context.Response.Cookies.Append("lastAccessTime", DateTime.Now.ToString(), new CookieOptions
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax
                    });
                }
            }
              
            await next(context);
        }
        
    }
}
