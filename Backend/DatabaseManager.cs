using System.Data.SQLite;
using System.Resources;

namespace JuniorProject.Backend
{
    class DatabaseManager
    {
        static DatabaseManager _instance;
        static DatabaseManager Instance
        {
            get
            {
                _instance ??= new DatabaseManager();
                return _instance;
            }
        }

        SQLiteConnection connection;

        private DatabaseManager() { }

        public static void LoadDB(string db_file)
        {
            string DBLocation = Properties.Resources.ProjectDir + db_file;
            Debug.Print($"Attempting to load: {DBLocation}");
            if (!System.IO.File.Exists(DBLocation))
            {
                throw new Exception($"No Database file found at: {DBLocation}");
            }


#if DEBUG  //Backups the DB in the debug output folder in case of catastrophic changes.                                                                               
            System.IO.Directory.CreateDirectory("./Backup");
            System.IO.File.Copy(DBLocation, $"./Backup/BackupDB.db", true);
#endif

            Instance.connection = new SQLiteConnection($"Data Source={DBLocation};Version=3;New=True;Compress=True;");
            Instance.connection.Open();
            Debug.Print("Loaded.");
        }
        /// <summary>
        /// Returns the SQLiteDataReader results of the command, this can be used to parse through the output.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static SQLiteDataReader ReadDB(string command)
        {
            SQLiteDataReader reader;
            SQLiteCommand cmd = Instance.connection.CreateCommand();
            cmd.CommandText = command;
            reader = cmd.ExecuteReader();
            return reader;
        }

        /// <summary>
        /// Executes 'command' on the opened database. Won't return any values. Used only for modification operations.
        /// </summary>
        /// <param name="command"></param>
        public static void WriteDB(string commandText, Dictionary<string, object> parameters)
        {
            using var cmd = Instance.connection.CreateCommand();
            cmd.CommandText = commandText;
            foreach (var pair in parameters)
            {
                cmd.Parameters.AddWithValue(pair.Key, pair.Value);
            }
            cmd.ExecuteNonQuery();
        }




    }
}
