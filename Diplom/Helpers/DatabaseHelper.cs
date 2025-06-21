using Diplom.Helpers;
using Diplom.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Diplom
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["BeautySalon"].ConnectionString;

        public static string HashPassword(string password)
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

        public static bool RegisterUser(string username, string email, string password, string role, out string error)
        {
            error = null;
            string hashedPassword = HashPassword(password);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Users (Username, PasswordHash, Email, Role) VALUES (@Username, @PasswordHash, @Email, @Role)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Role", role);
                        

                        int result = command.ExecuteNonQuery();
                        return result == 1;
                    }
                }
                catch (SqlException ex)
                {
                    error = ex.Number == 2627 ? "Пользователь с таким именем или email уже существует" : "Ошибка базы данных";
                    return false;
                }
            }
        }

        public static (bool Success, int UserID, string Role) AuthenticateUser(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT UserID, Role FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (true, reader.GetInt32(0), reader.GetString(1));
                            }
                            return (false, 0, null);
                        }
                    }
                }
                catch
                {
                    return (false, 0, null);
                }
            }
        }

        internal static bool RegisterUsers(string text1, string text2, string password, string text3, out string error)
        {
            throw new NotImplementedException();
        }

        public static List<EquipmentModel> GetEquipmentList()
        {
            var result = new List<EquipmentModel>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Equipment", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new EquipmentModel
                        {
                            EquipmentID = (int)reader["EquipmentID"],
                            Name = reader["Name"].ToString(),
                            PurchaseDate = (DateTime)reader["PurchaseDate"],
                            Cost = (decimal)reader["Cost"],
                            Condition = reader["Condition"].ToString()
                        });
                    }
                }
            }
            return result;
        }

        public static void DeleteEquipment(int equipmentId)
        {
            using (var conn = new SqlConnection(connectionString)) // connectionString уже должен быть у тебя
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Equipment WHERE EquipmentID = @id", conn);
                cmd.Parameters.AddWithValue("@id", equipmentId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertEquipment(EquipmentModel eq)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Equipment (Name, PurchaseDate, Cost, Condition) VALUES (@name, @date, @cost, @cond)", conn);
                cmd.Parameters.AddWithValue("@name", eq.Name);
                cmd.Parameters.AddWithValue("@date", eq.PurchaseDate);
                cmd.Parameters.AddWithValue("@cost", eq.Cost);
                cmd.Parameters.AddWithValue("@cond", eq.Condition);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateEquipment(EquipmentModel eq)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Equipment SET Name = @name, PurchaseDate = @date, Cost = @cost, Condition = @cond WHERE EquipmentID = @id", conn);
                cmd.Parameters.AddWithValue("@id", eq.EquipmentID);
                cmd.Parameters.AddWithValue("@name", eq.Name);
                cmd.Parameters.AddWithValue("@date", eq.PurchaseDate);
                cmd.Parameters.AddWithValue("@cost", eq.Cost);
                cmd.Parameters.AddWithValue("@cond", eq.Condition);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<SupplyModel> GetSupplies()
        {
            var list = new List<SupplyModel>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT SupplyID, SupplierName, DeliveryDate, TotalCost, MaterialID FROM Supplies", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SupplyModel
                        {
                            SupplyID = (int)reader["SupplyID"],
                            SupplierName = reader["SupplierName"].ToString(),
                            DeliveryDate = (DateTime)reader["DeliveryDate"],
                            TotalCost = (decimal)reader["TotalCost"],
                            MaterialID = (int)reader["MaterialID"]
                        });
                    }
                }
            }
            return list;
        }


        public static bool InsertSupply(string supplierName, DateTime deliveryDate, decimal totalCost, int materialId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Supplies (SupplierName, DeliveryDate, TotalCost, MaterialID) VALUES (@name, @date, @cost, @material)", conn);
                cmd.Parameters.AddWithValue("@name", supplierName);
                cmd.Parameters.AddWithValue("@date", deliveryDate);
                cmd.Parameters.AddWithValue("@cost", totalCost);
                cmd.Parameters.AddWithValue("@material", materialId);
                int result = cmd.ExecuteNonQuery();
                return result == 1;
            }
        }

        public static bool UpdateSupply(SupplyModel supply)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"UPDATE Supplies 
                                   SET SupplierName = @name, DeliveryDate = @date, 
                                       TotalCost = @cost, MaterialID = @material 
                                   WHERE SupplyID = @id", conn);
                cmd.Parameters.AddWithValue("@name", supply.SupplierName);
                cmd.Parameters.AddWithValue("@date", supply.DeliveryDate);
                cmd.Parameters.AddWithValue("@cost", supply.TotalCost);
                cmd.Parameters.AddWithValue("@material", supply.MaterialID);
                cmd.Parameters.AddWithValue("@id", supply.SupplyID);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public static bool DeleteSupply(int supplyId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Supplies WHERE SupplyID = @id", conn);
                cmd.Parameters.AddWithValue("@id", supplyId);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public static List<MaterialModel> GetMaterials()
        {
            var list = new List<MaterialModel>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT MaterialID, Name, Quantity, UnitPrice FROM Materials", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MaterialModel
                        {
                            MaterialID = (int)reader["MaterialID"],
                            Name = reader["Name"].ToString(),
                            Quantity = (int)reader["Quantity"],
                            UnitPrice = (decimal)reader["UnitPrice"]
                        });
                    }
                }
            }
            return list;
        }

        public static bool DeleteMaterial(int materialId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Materials WHERE MaterialID = @id", conn);
                cmd.Parameters.AddWithValue("@id", materialId);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public static bool AddMaterial(MaterialModel material)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Materials (Name, Quantity, UnitPrice) VALUES (@name, @qty, @price)", conn);
                cmd.Parameters.AddWithValue("@name", material.Name);
                cmd.Parameters.AddWithValue("@qty", material.Quantity);
                cmd.Parameters.AddWithValue("@price", material.UnitPrice);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public static bool UpdateMaterial(MaterialModel material)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Materials SET Name = @name, Quantity = @qty, UnitPrice = @price WHERE MaterialID = @id", conn);
                cmd.Parameters.AddWithValue("@id", material.MaterialID);
                cmd.Parameters.AddWithValue("@name", material.Name);
                cmd.Parameters.AddWithValue("@qty", material.Quantity);
                cmd.Parameters.AddWithValue("@price", material.UnitPrice);
                return cmd.ExecuteNonQuery() == 1;
            }
        }


    }
}