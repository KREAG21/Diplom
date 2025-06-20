using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Helpers
{
    public class SupplyModel
    {
        public int SupplyID { get; set; }
        public string SupplierName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal TotalCost { get; set; }
        public int MaterialID { get; set; }

        // Необязательно: для отображения
        public string MaterialName { get; set; }
    }
}
