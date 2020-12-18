using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ISP_Lab13
{
    public class PhoneBookDatebase
    {

        private const string SERVER_CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;Trusted_Connection=Yes;";

        private const string DATABASE_CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Trusted_Connection=Yes;MultipleActiveResultSets=True";

        private const string CREATE_DATABASE_QUERY_STRING = "CREATE DATABASE PhoneBookDb ON PRIMARY (NAME = PhoneBookDb, FILENAME = '{0}')";
        private const string DELETE_DATABASE_QUERY_STRING = "USE master " +
            "ALTER DATABASE PhoneBookDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE " +
            "DROP DATABASE PhoneBookDb;";

        private const string CREATE_USERS_TABLE_QUERY_STRING = "CREATE TABLE Users ( " +
            "Id INT IDENTITY(1, 1) NOT NULL, " +
            "FirstName VARCHAR(50) NOT NULL, " +
            "LastName VARCHAR(50) NOT NULL, " +
            "Birthday DATETIME NOT NULL, " +
            "PRIMARY KEY CLUSTERED(Id ASC) " +
        ");";

        private const string CREATE_PHONE_NUMBERS_TABLE_QUERY_STRING = "CREATE TABLE PhoneNumbers (" +
            "Id INT IDENTITY(1, 1) NOT NULL, " +
            "UserId INT NOT NULL, " +
            "Number VARCHAR(25) NOT NULL, " +
            "PRIMARY KEY CLUSTERED(Id ASC), " +
             "FOREIGN KEY(UserId) REFERENCES Users (Id) ON DELETE CASCADE" +
        ");";

        public string DatabaseFilePath { get; }

        public SqlConnection DatabaseConnection { get; }

        public PhoneBookDatebase(string databaseFilePath)
        {
            DatabaseFilePath = databaseFilePath;
            DatabaseConnection = new SqlConnection(string.Format(DATABASE_CONNECTION_STRING, DatabaseFilePath));
        }

        public void InitializeDatabase()
        {
            if (File.Exists(DatabaseFilePath))
            {
                return;
            }
            SqlConnection serverConnection = new SqlConnection(SERVER_CONNECTION_STRING);
            SqlCommand createDatabaseCommand = new SqlCommand(string.Format(CREATE_DATABASE_QUERY_STRING, DatabaseFilePath), serverConnection);
            try
            {
                serverConnection.Open();
                createDatabaseCommand.ExecuteNonQuery();
                CreateDatabaseTables();
            }
            finally
            {
                if (serverConnection.State == ConnectionState.Open)
                {
                    serverConnection.Close();
                }
            }
        }

        private void CreateDatabaseTables()
        {
            try
            {
                OpenConnection();
                SqlCommand createUsersTableCommand = new SqlCommand(CREATE_USERS_TABLE_QUERY_STRING, DatabaseConnection);
                createUsersTableCommand.ExecuteNonQuery();
                SqlCommand cretePhoneNumbersTableCommand = new SqlCommand(CREATE_PHONE_NUMBERS_TABLE_QUERY_STRING, DatabaseConnection);
                cretePhoneNumbersTableCommand.ExecuteNonQuery();
            }
            finally
            {
                CloseConnection();
            }
        }

        public void OpenConnection()
        {
            if (DatabaseConnection.State != ConnectionState.Open)
            {
                DatabaseConnection.Open();
            }
        }

        public int ExecuteQuery(string sqlExpression)
        {
            SqlCommand command = new SqlCommand(sqlExpression, DatabaseConnection);
            return command.ExecuteNonQuery();
        }

        public void CloseConnection()
        {
            if (DatabaseConnection.State == ConnectionState.Open)
            {
                DatabaseConnection.Close();
            }
        }

        public void DeleteDatabase()
        {
            SqlConnection serverConnection = new SqlConnection(SERVER_CONNECTION_STRING);
            SqlCommand deleteDatabaseCommand = new SqlCommand(DELETE_DATABASE_QUERY_STRING, serverConnection);
            try
            {
                CloseConnection();
                serverConnection.Open();
                deleteDatabaseCommand.ExecuteNonQuery();
            }
            finally
            {
                if (serverConnection.State == ConnectionState.Open)
                {
                    serverConnection.Close();
                }
            }
        }
    }
}
