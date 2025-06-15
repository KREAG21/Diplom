using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Model
{
    public class EquipmentModel
    {
        public int EquipmentID { get; set; }
        public string Name { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Cost { get; set; }
        public string Condition { get; set; }
    }
}
