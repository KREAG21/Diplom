using Diplom.Helpers;
using Diplom.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Diplom
{
    public static class DatabaseHelper
    {
        private const string PasswordHashPrefix = "PBKDF2";
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int Pbkdf2Iterations = 100000;

        public static string ConnectionString => "MockDataSource";

        private static readonly object SyncRoot = new object();

        private static readonly List<UserRecord> Users = new List<UserRecord>();
        private static readonly List<EquipmentModel> Equipments = new List<EquipmentModel>();
        private static readonly List<SupplyModel> Supplies = new List<SupplyModel>();
        private static readonly List<MaterialModel> Materials = new List<MaterialModel>();
        private static readonly List<OrderRecord> Orders = new List<OrderRecord>();

        private static int _nextUserId = 1;
        private static int _nextEquipmentId = 1;
        private static int _nextSupplyId = 1;
        private static int _nextMaterialId = 1;
        private static int _nextOrderId = 1;

        static DatabaseHelper()
        {
            SeedMockData();
        }

        private static void SeedMockData()
        {
            AddUserInternal("manager", "manager@beauty.local", "manager123", "менеджер");
            AddUserInternal("employee", "employee@beauty.local", "employee123", "сотрудник");

            var shampoo = AddMaterialInternal("Шампунь", 30, 450m);
            var mask = AddMaterialInternal("Маска для волос", 15, 780m);

            Supplies.Add(new SupplyModel
            {
                SupplyID = _nextSupplyId++,
                SupplierName = "ООО БьютиТрейд",
                DeliveryDate = DateTime.Today.AddDays(-10),
                TotalCost = 12500m,
                MaterialID = shampoo.MaterialID
            });

            Supplies.Add(new SupplyModel
            {
                SupplyID = _nextSupplyId++,
                SupplierName = "ЗАО ПрофКосметика",
                DeliveryDate = DateTime.Today.AddDays(-4),
                TotalCost = 8400m,
                MaterialID = mask.MaterialID
            });

            Equipments.Add(new EquipmentModel
            {
                EquipmentID = _nextEquipmentId++,
                Name = "Фен профессиональный",
                PurchaseDate = DateTime.Today.AddMonths(-8),
                Cost = 15900m,
                Condition = "Исправен"
            });

            Equipments.Add(new EquipmentModel
            {
                EquipmentID = _nextEquipmentId++,
                Name = "Стерилизатор",
                PurchaseDate = DateTime.Today.AddMonths(-5),
                Cost = 9400m,
                Condition = "Исправен"
            });

            Orders.Add(new OrderRecord
            {
                OrderID = _nextOrderId++,
                CustomerName = "Анна Петрова",
                ServiceDescription = "Стрижка + укладка",
                OrderDate = DateTime.Today.AddDays(-2),
                TotalAmount = 3500m,
                EquipmentID = Equipments[0].EquipmentID,
                UserID = 2
            });

            Orders.Add(new OrderRecord
            {
                OrderID = _nextOrderId++,
                CustomerName = "Мария Иванова",
                ServiceDescription = "Окрашивание",
                OrderDate = DateTime.Today.AddDays(-1),
                TotalAmount = 5200m,
                EquipmentID = Equipments[1].EquipmentID,
                UserID = 2
            });
        }

        private static MaterialModel AddMaterialInternal(string name, int quantity, decimal unitPrice)
        {
            var material = new MaterialModel
            {
                MaterialID = _nextMaterialId++,
                Name = name,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            Materials.Add(material);
            return material;
        }

        private static void AddUserInternal(string username, string email, string password, string role)
        {
            Users.Add(new UserRecord
            {
                UserID = _nextUserId++,
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = role
            });
        }

        private static string HashLegacyPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSizeBytes];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HashSizeBytes);
            }

            return string.Format(
                "{0}${1}${2}${3}",
                PasswordHashPrefix,
                Pbkdf2Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }

        private static bool VerifyPassword(string password, string storedHash, out bool requiresUpgrade)
        {
            requiresUpgrade = false;

            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var hashParts = storedHash.Split('$');
            if (hashParts.Length == 4 && hashParts[0] == PasswordHashPrefix)
            {
                int iterations;
                if (!int.TryParse(hashParts[1], out iterations))
                {
                    return false;
                }

                byte[] salt;
                byte[] expectedHash;
                try
                {
                    salt = Convert.FromBase64String(hashParts[2]);
                    expectedHash = Convert.FromBase64String(hashParts[3]);
                }
                catch (FormatException)
                {
                    return false;
                }

                byte[] actualHash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    actualHash = pbkdf2.GetBytes(expectedHash.Length);
                }

                if (!FixedTimeEquals(actualHash, expectedHash))
                {
                    return false;
                }

                requiresUpgrade = iterations < Pbkdf2Iterations;
                return true;
            }

            bool legacyMatch = HashLegacyPassword(password) == storedHash;
            requiresUpgrade = legacyMatch;
            return legacyMatch;
        }

        public static bool RegisterUser(string username, string email, string password, string role, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                error = "Все поля обязательны";
                return false;
            }

            lock (SyncRoot)
            {
                bool exists = Users.Any(u =>
                    string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    error = "Пользователь с таким именем или email уже существует";
                    return false;
                }

                Users.Add(new UserRecord
                {
                    UserID = _nextUserId++,
                    Username = username.Trim(),
                    Email = email.Trim(),
                    PasswordHash = HashPassword(password),
                    Role = role.Trim()
                });

                return true;
            }
        }

        public static (bool Success, int UserID, string Role) AuthenticateUser(string username, string password)
        {
            lock (SyncRoot)
            {
                var user = Users.FirstOrDefault(u =>
                    string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    return (false, 0, null);
                }

                bool requiresUpgrade;
                if (!VerifyPassword(password, user.PasswordHash, out requiresUpgrade))
                {
                    return (false, 0, null);
                }

                if (requiresUpgrade)
                {
                    user.PasswordHash = HashPassword(password);
                }

                return (true, user.UserID, user.Role);
            }
        }

        internal static bool RegisterUsers(string username, string email, string password, string role, out string error)
        {
            return RegisterUser(username, email, password, role, out error);
        }

        public static DataTable GetOrdersTable()
        {
            lock (SyncRoot)
            {
                var table = new DataTable();
                table.Columns.Add("OrderID", typeof(int));
                table.Columns.Add("CustomerName", typeof(string));
                table.Columns.Add("ServiceDescription", typeof(string));
                table.Columns.Add("OrderDate", typeof(DateTime));
                table.Columns.Add("TotalAmount", typeof(decimal));
                table.Columns.Add("EquipmentID", typeof(int));

                foreach (var order in Orders.OrderByDescending(o => o.OrderDate))
                {
                    table.Rows.Add(order.OrderID, order.CustomerName, order.ServiceDescription, order.OrderDate, order.TotalAmount, order.EquipmentID);
                }

                return table;
            }
        }

        public static bool SaveOrder(int orderId, string customerName, string serviceDescription, decimal totalAmount, DateTime orderDate, int userId)
        {
            lock (SyncRoot)
            {
                if (orderId == 0)
                {
                    Orders.Add(new OrderRecord
                    {
                        OrderID = _nextOrderId++,
                        CustomerName = customerName,
                        ServiceDescription = serviceDescription,
                        TotalAmount = totalAmount,
                        OrderDate = orderDate,
                        UserID = userId,
                        EquipmentID = Equipments.Count > 0 ? Equipments[0].EquipmentID : 0
                    });

                    return true;
                }

                var existing = Orders.FirstOrDefault(o => o.OrderID == orderId);
                if (existing == null)
                {
                    return false;
                }

                existing.CustomerName = customerName;
                existing.ServiceDescription = serviceDescription;
                existing.TotalAmount = totalAmount;
                existing.OrderDate = orderDate;
                return true;
            }
        }

        public static bool DeleteOrder(int orderId)
        {
            lock (SyncRoot)
            {
                var order = Orders.FirstOrDefault(o => o.OrderID == orderId);
                if (order == null)
                {
                    return false;
                }

                Orders.Remove(order);
                return true;
            }
        }

        public static List<EquipmentModel> GetEquipmentList()
        {
            lock (SyncRoot)
            {
                return Equipments.Select(CloneEquipment).ToList();
            }
        }

        public static void DeleteEquipment(int equipmentId)
        {
            lock (SyncRoot)
            {
                var equipment = Equipments.FirstOrDefault(e => e.EquipmentID == equipmentId);
                if (equipment != null)
                {
                    Equipments.Remove(equipment);
                }
            }
        }

        public static void InsertEquipment(EquipmentModel eq)
        {
            lock (SyncRoot)
            {
                Equipments.Add(new EquipmentModel
                {
                    EquipmentID = _nextEquipmentId++,
                    Name = eq.Name,
                    PurchaseDate = eq.PurchaseDate,
                    Cost = eq.Cost,
                    Condition = eq.Condition
                });
            }
        }

        public static void UpdateEquipment(EquipmentModel eq)
        {
            lock (SyncRoot)
            {
                var existing = Equipments.FirstOrDefault(e => e.EquipmentID == eq.EquipmentID);
                if (existing == null)
                {
                    return;
                }

                existing.Name = eq.Name;
                existing.PurchaseDate = eq.PurchaseDate;
                existing.Cost = eq.Cost;
                existing.Condition = eq.Condition;
            }
        }

        public static List<SupplyModel> GetSupplies()
        {
            lock (SyncRoot)
            {
                return Supplies.Select(s => new SupplyModel
                {
                    SupplyID = s.SupplyID,
                    SupplierName = s.SupplierName,
                    DeliveryDate = s.DeliveryDate,
                    TotalCost = s.TotalCost,
                    MaterialID = s.MaterialID
                }).ToList();
            }
        }

        public static bool InsertSupply(string supplierName, DateTime deliveryDate, decimal totalCost, int materialId)
        {
            lock (SyncRoot)
            {
                Supplies.Add(new SupplyModel
                {
                    SupplyID = _nextSupplyId++,
                    SupplierName = supplierName,
                    DeliveryDate = deliveryDate,
                    TotalCost = totalCost,
                    MaterialID = materialId
                });

                return true;
            }
        }

        public static bool UpdateSupply(SupplyModel supply)
        {
            lock (SyncRoot)
            {
                var existing = Supplies.FirstOrDefault(s => s.SupplyID == supply.SupplyID);
                if (existing == null)
                {
                    return false;
                }

                existing.SupplierName = supply.SupplierName;
                existing.DeliveryDate = supply.DeliveryDate;
                existing.TotalCost = supply.TotalCost;
                existing.MaterialID = supply.MaterialID;
                return true;
            }
        }

        public static bool DeleteSupply(int supplyId)
        {
            lock (SyncRoot)
            {
                var existing = Supplies.FirstOrDefault(s => s.SupplyID == supplyId);
                if (existing == null)
                {
                    return false;
                }

                Supplies.Remove(existing);
                return true;
            }
        }

        public static List<MaterialModel> GetMaterials()
        {
            lock (SyncRoot)
            {
                return Materials.Select(m => new MaterialModel
                {
                    MaterialID = m.MaterialID,
                    Name = m.Name,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice
                }).ToList();
            }
        }

        public static bool DeleteMaterial(int materialId)
        {
            lock (SyncRoot)
            {
                var material = Materials.FirstOrDefault(m => m.MaterialID == materialId);
                if (material == null)
                {
                    return false;
                }

                Materials.Remove(material);
                return true;
            }
        }

        public static bool AddMaterial(MaterialModel material)
        {
            lock (SyncRoot)
            {
                Materials.Add(new MaterialModel
                {
                    MaterialID = _nextMaterialId++,
                    Name = material.Name,
                    Quantity = material.Quantity,
                    UnitPrice = material.UnitPrice
                });

                return true;
            }
        }

        public static bool UpdateMaterial(MaterialModel material)
        {
            lock (SyncRoot)
            {
                var existing = Materials.FirstOrDefault(m => m.MaterialID == material.MaterialID);
                if (existing == null)
                {
                    return false;
                }

                existing.Name = material.Name;
                existing.Quantity = material.Quantity;
                existing.UnitPrice = material.UnitPrice;
                return true;
            }
        }

        private static EquipmentModel CloneEquipment(EquipmentModel source)
        {
            return new EquipmentModel
            {
                EquipmentID = source.EquipmentID,
                Name = source.Name,
                PurchaseDate = source.PurchaseDate,
                Cost = source.Cost,
                Condition = source.Condition
            };
        }

        private class UserRecord
        {
            public int UserID { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
            public string Role { get; set; }
        }

        private class OrderRecord
        {
            public int OrderID { get; set; }
            public string CustomerName { get; set; }
            public string ServiceDescription { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public int EquipmentID { get; set; }
            public int UserID { get; set; }
        }
    }
}
