using System;
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

        private const string INSERT_USER_QUERY = "INSERT INTO Users (FirstName, LastName, Birthday) VALUES (@firstName, @lastName, @birthday)";
        private const string INSERT_PHONE_NUMBER_QUERY = "INSERT INTO PhoneNumbers (UserId, Number) VALUES (@userId, @number)";
        private const string CHANGE_USER_LAST_NAME_QUERY = "UPDATE Users SET LastName=@lastName WHERE FirstName=@firstName";
        private const string DELETE_USER_QUERY = "DELETE FROM Users WHERE FirstName=@firstName";
        private const string SELECT_ALL_FROM_USERS_QUERY = "SELECT * FROM Users";
        private const string SELECT_FROM_PHONE_NUMBERS_QUERY = "SELECT Id, Number FROM PhoneNumbers WHERE UserId = @userId";

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

        public int AddUser(string firstName, string lastName, string birthday)
        {
            SqlCommand insertUserCommand = new SqlCommand(INSERT_USER_QUERY, DatabaseConnection);
            insertUserCommand.Parameters.AddWithValue("@firstName", firstName);
            insertUserCommand.Parameters.AddWithValue("@lastName", lastName);
            insertUserCommand.Parameters.AddWithValue("@birthday", birthday);
            return insertUserCommand.ExecuteNonQuery();
        }

        public int AddPhoneNumber(string userId, string number)
        {
            SqlCommand insertPhoneNumberCommand = new SqlCommand(INSERT_PHONE_NUMBER_QUERY, DatabaseConnection);
            insertPhoneNumberCommand.Parameters.AddWithValue("@userId", userId);
            insertPhoneNumberCommand.Parameters.AddWithValue("@number", number);
            return insertPhoneNumberCommand.ExecuteNonQuery();
        }

        public int ChangeUserLastName(string userFirstName, string userLastName)
        {
            SqlCommand changeUserLastNameCommand = new SqlCommand(CHANGE_USER_LAST_NAME_QUERY, DatabaseConnection);
            changeUserLastNameCommand.Parameters.AddWithValue("@firstName", userFirstName);
            changeUserLastNameCommand.Parameters.AddWithValue("@lastName", userLastName);
            return changeUserLastNameCommand.ExecuteNonQuery();
        }

        public int DeleteUser(string userFirstName)
        {
            SqlCommand deleteUserCommand = new SqlCommand(DELETE_USER_QUERY, DatabaseConnection);
            deleteUserCommand.Parameters.AddWithValue("@firstName", userFirstName);
            return deleteUserCommand.ExecuteNonQuery();
        }

        public int ExecuteQuery(string sqlExpression)
        {
            SqlCommand command = new SqlCommand(sqlExpression, DatabaseConnection);
            return command.ExecuteNonQuery();
        }

        public void OutputDB()
        {
            SqlCommand selectUsersCommand = new SqlCommand(SELECT_ALL_FROM_USERS_QUERY, DatabaseConnection);
            SqlDataReader reader = selectUsersCommand.ExecuteReader();

            if (reader.HasRows)
            {
                Console.WriteLine("{0, 5} {1, 20} {2, 20} {3, 20}", reader.GetName(0), reader.GetName(1), reader.GetName(2), reader.GetName(3));

                while (reader.Read())
                {
                    object id = reader.GetValue(0);
                    object firstName = reader.GetValue(1);
                    object lastName = reader.GetValue(2);
                    DateTime birthday = (DateTime)reader.GetValue(3);
                    Console.WriteLine("{0, 5} {1, 20} {2, 20} {3, 20}", id, firstName, lastName, birthday.ToShortDateString());

                    SqlCommand phonesCommand = new SqlCommand(SELECT_FROM_PHONE_NUMBERS_QUERY, DatabaseConnection);
                    phonesCommand.Parameters.Add(new SqlParameter("@userId", id));
                    SqlDataReader phonesReader = phonesCommand.ExecuteReader();
                    if (phonesReader.HasRows)
                    {
                        Console.WriteLine("{0, 5} {1, 20}", phonesReader.GetName(0), phonesReader.GetName(1));
                        while (phonesReader.Read())
                        {
                            Console.WriteLine("{0, 5} {1, 20}", phonesReader.GetValue(0), phonesReader.GetValue(1));
                        }
                    }
                    else
                    {
                        Console.WriteLine("  Нет номера телефона для этого пользователя.");
                    }
                    phonesReader.Close();
                    Console.WriteLine();
                }
            }
            reader.Close();
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
