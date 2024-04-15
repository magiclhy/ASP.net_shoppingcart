using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(30)]
        public string Username { get; set; }
        [Required, MaxLength(100)]
        public string Password { get; set; }
        [Required, MaxLength(50)]
        public string Salt { get; set; }
    }
}
