using Diplom.Helpers;
using Diplom.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

        public static string ConnectionString => ConfigurationManager.ConnectionStrings["BeautySalon"].ConnectionString;

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
            string hashedPassword = HashPassword(password);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Users (Username, PasswordHash, Email, Role) VALUES (@Username, @PasswordHash, @Email, @Role)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = hashedPassword;
                        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;
                        command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = role;

                        return command.ExecuteNonQuery() == 1;
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
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    const string query = "SELECT UserID, Role, PasswordHash FROM Users WHERE Username = @Username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return (false, 0, null);
                            }

                            int userId = reader.GetInt32(0);
                            string role = reader.GetString(1);
                            string storedHash = reader[2].ToString();

                            bool requiresUpgrade;
                            if (!VerifyPassword(password, storedHash, out requiresUpgrade))
                            {
                                return (false, 0, null);
                            }

                            if (requiresUpgrade)
                            {
                                reader.Close();
                                UpgradePasswordHash(connection, userId, password);
                            }

                            return (true, userId, role);
                        }
                    }
                }
                catch
                {
                    return (false, 0, null);
                }
            }
        }

        private static void UpgradePasswordHash(SqlConnection connection, int userId, string password)
        {
            const string updateQuery = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID";
            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
            {
                updateCommand.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = HashPassword(password);
                updateCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                updateCommand.ExecuteNonQuery();
            }
        }

        internal static bool RegisterUsers(string username, string email, string password, string role, out string error)
        {
            return RegisterUser(username, email, password, role, out error);
        }

        public static List<EquipmentModel> GetEquipmentList()
        {
            var result = new List<EquipmentModel>();
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Equipment WHERE EquipmentID = @id", conn);
                cmd.Parameters.AddWithValue("@id", equipmentId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertEquipment(EquipmentModel eq)
        {
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Materials WHERE MaterialID = @id", conn);
                cmd.Parameters.AddWithValue("@id", materialId);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public static bool AddMaterial(MaterialModel material)
        {
            using (var conn = new SqlConnection(ConnectionString))
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
            using (var conn = new SqlConnection(ConnectionString))
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
