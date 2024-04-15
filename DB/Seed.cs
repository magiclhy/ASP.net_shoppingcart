using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingCart.DBs
{
    public class DbSeeder
    {
        private readonly DbShoppingCart _db;

        public DbSeeder(DbShoppingCart db)
        {
            _db = db;
        }
        public void Seed()
        {
            CreateUsers();
            CreateProducts();
            CreateCarts();
            CreateOrders(10);
            CreateCleanUser("smith");
        }

        //创建一组用户，生成salt和哈希值
        private void CreateUsers()
        {
            // Example Username: angelia  Password: angelia
            string[] users = { "Xena", "Liu", "Kei", "Andrew"};
            foreach (string user in users)
            {
                //加密
                byte[] salt = GenerateSalt();
                _db.Add(new User
                {
                    Username = user,
                    Salt = Convert.ToBase64String(salt),
                    Password = GenerateHashString(user, salt)
                });
            }
            _db.SaveChanges();
        }

        //创建一个特定的新用户
        private void CreateCleanUser(string user)
        {
            byte[] salt = GenerateSalt();
            _db.Add(new User
            {
                Username = user,
                Salt = Convert.ToBase64String(salt),
                Password = GenerateHashString(user, salt)
            });
            _db.SaveChanges();
        }

        //创建一系列产品名称、描述、价格、图像链接和数量。
        private void CreateProducts()
        {
            
            string[] names = { "a", "b"};
            string[] descriptions =
            {
                "aaa",
                "bbb"
            };
            double[] prices = { 99, 66 };
            int[] quantity = { 50, 50 };
            string[] imagelinks =
            {
                "a.jpeg",
                "b.jpeg"
            };

            for (int i = 0; i < names.Length; i++)
            {
                _db.Add(new Product
                {
                    Name = names[i],
                    Description = descriptions[i],
                    Price = prices[i],
                    ImageLink = "~/images/product/" + imagelinks[i],
                    Quantity = quantity[i]
                });
            }
            _db.SaveChanges();
        }

        private void CreateCarts()
        {
            // 随机生成购物车条目
            List<User> users = _db.Users.ToList();
            Random r = new Random();
            List<Product> products = _db.Products.ToList();

            foreach (User user in users)
            {
                List<int> used = new List<int>();
                for (int i = 0; i < r.Next(1, 5); i++)
                {
                    var randomNumber = r.Next(products.Count);
                    while (used.Contains(randomNumber))
                    {
                        randomNumber = r.Next(products.Count);
                    }
                    used.Add(randomNumber);
                    var randomProduct = products[randomNumber];

                    randomNumber = r.Next(1, 6);
                    var randomCart = new Cart { ProductId = randomProduct.Id, UserId = user.Id, Quantity = randomNumber };
                    _db.Carts.Add(randomCart);
                }
                _db.SaveChanges();
            }
        }

        private void CreateOrders(int orders)
        {
            // 随机生成指定数量订单
            List<User> users = _db.Users.ToList();
            List<Product> products = _db.Products.ToList();

            Random r = new Random();

            for (int i = 0; i < orders; i++)
            {
                List<int> used = new List<int>();
                List<string> usedKeys = new List<string>();
                var startDate = new DateTime(2010, 1, 1);
                var randomUser = users[r.Next(users.Count)];
                var randomDate = startDate.AddDays(r.Next((DateTime.Today - startDate).Days));
                var randomOrder = new Order { UserId = randomUser.Id, DateTime = randomDate };
                _db.Orders.Add(randomOrder);
                _db.SaveChanges();

                for (int j = 0; j < r.Next(1, 6); j++)
                {
                    var randomNumber = r.Next(products.Count);
                    while (used.Contains(randomNumber))
                    {
                        randomNumber = r.Next(products.Count);
                    }
                    used.Add(randomNumber);
                    var randomProduct = products[randomNumber];
                    var randomQuantity = r.Next(1, 6);
                    for (int k = 0; k <= randomQuantity; k++)
                    {
                        var randomActivation = GenerateActivationKey(r);
                        while (usedKeys.Contains(randomActivation))
                        {
                            randomActivation = GenerateActivationKey(r);
                        }
                        usedKeys.Add(randomActivation);
                        var randomItem = new OrderDetail { Id = randomActivation, OrderId = randomOrder.Id, ProductId = randomProduct.Id };
                        _db.OrderDetails.Add(randomItem);
                    }
                    
                    _db.SaveChanges();
                }
            }
        }

        //生成一个格式化的激活密钥
        private string GenerateActivationKey(Random r)
        {
            const string chars = "abcdefghiijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 14; i++)
            {
                if (i == 4 || i == 9) { sb.Append("-"); }
                else { sb.Append(chars[r.Next(chars.Length)]); }
            }
            return sb.ToString();
        }

        //哈希算法加密
        private string GenerateHashString(string password, byte[] salt)
        {
            byte[] encrypted = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA1, 10000, 32);
            return Convert.ToBase64String(encrypted);
        }

        //salt用于哈希密码
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }       
       
    }
}
