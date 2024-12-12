using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsurl.test
{
    internal class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public double Price { get; set; }
        public bool Deleted { get; set; }
        public List<Category> Categories { get; set; }
    }
}
