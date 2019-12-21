using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW.Domain
{
    public class Product : Entity
    {
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
