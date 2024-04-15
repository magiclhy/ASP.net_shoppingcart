using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ShoppingCart.Models
{
    public class GalleryViewModel
    {
        public int Columns { get; set; }
        public int Page { get; set; }
        public int TotalPage { get; set; }
        public int TotalProducts { get; set; }
        public User User { get; set; }
        public List<Product> DisplayedProducts { get; set; }

        // Row and column here refer to the number of products that should be visible in a row and column respectively
        // Example: row = 3, column = 4 means that there should be 3 rows and 4 columns total in a page (12 products total)
        public GalleryViewModel(int row, int column, User user, int page, List<Product> products)
        {
            Columns = column;
            Page = page;
            TotalProducts = products.Count;
            TotalPage = (int) Math.Ceiling((double)TotalProducts / (column * row));
            DisplayedProducts = new List<Product>();
            User = user;

            for (int i = (page - 1) * column * row; i < page * column * row && i < TotalProducts; i++)
            {
                DisplayedProducts.Add(products[i]);
            }
        }

        public GalleryViewModel(User user, int page, List<Product> products) : this(4, 3, user, page, products) { }

    }
}
