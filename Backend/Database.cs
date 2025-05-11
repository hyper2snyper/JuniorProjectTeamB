using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuniorProject.Backend
{
    public class Database
    {
        private readonly string _connectionString;
        public string ConnectionString => _connectionString;

        public Database(string dbFilePath)
        {
            _connectionString = $"Data Source={dbFilePath};Version=3;";
        }

        public object GetValue(string tableName, string idColumnName, int rowId, string columnName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = $"SELECT {columnName} FROM {tableName} WHERE {idColumnName} = @Id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", rowId);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public void UpdateValue(string tableName, int rowId, string columnName, object newValue)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = $"UPDATE {tableName} SET {columnName} = @Value WHERE Id = @Id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Value", newValue);
                    cmd.Parameters.AddWithValue("@Id", rowId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
