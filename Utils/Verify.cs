using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ShoppingCart.DBs
{
    public class Verify
    {
        public bool VerifySession(string SessionId, DbShoppingCart _db) {
            if (string.IsNullOrEmpty(SessionId))
            {
                return false;
            } else
            {                
                return _db.Sessions.Where(x => x.Id == SessionId).ToList().Count != 0;
            }
        }
    }
}
