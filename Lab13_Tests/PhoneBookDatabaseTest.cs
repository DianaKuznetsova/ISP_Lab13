using ISP_Lab13;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Xunit;

namespace Lab13_Tests
{
    public class PhoneBookDatabaseTest
    {
        [Fact]
        public void CheckDatabaseCreated()
        {
            string pathToDb = Path.GetFullPath("test_db.mdf");
            PhoneBookDatebase db = new PhoneBookDatebase(pathToDb);
            db.InitializeDatabase();

            db.OpenConnection();

            Assert.True(File.Exists(pathToDb));

            DataTable tables = db.DatabaseConnection.GetSchema("Tables");

            List<string> tablesNames = new List<string>();

            foreach (DataRow table in tables.Rows)
            {
                string tableName = (string)table["TABLE_NAME"];
                tablesNames.Add(tableName);
            }

            Assert.Contains("Users", tablesNames);
            Assert.Contains("PhoneNumbers", tablesNames);

            List<string> usersColumns = new List<string>();
            List<string> phoneNumbersColumns = new List<string>();

            DataTable columns = db.DatabaseConnection.GetSchema("Columns");
            foreach (DataRow column in columns.Rows)
            {
                string tableName = (string)column["TABLE_NAME"];
                string columnName = (string)column["COLUMN_NAME"];
                if (tableName.Equals("Users"))
                {
                    usersColumns.Add(columnName);
                }
                else if (tableName.Equals("PhoneNumbers"))
                {
                    phoneNumbersColumns.Add(columnName);
                }
            }

            Assert.Contains("Id", usersColumns);
            Assert.Contains("FirstName", usersColumns);
            Assert.Contains("LastName", usersColumns);
            Assert.Contains("Birthday", usersColumns);

            Assert.Contains("Id", phoneNumbersColumns);
            Assert.Contains("UserId", phoneNumbersColumns);
            Assert.Contains("Number", phoneNumbersColumns);

            db.CloseConnection();
            db.DeleteDatabase();
        }

        [Fact]
        public void CheckUserIdGenerated()
        {
            string pathToDb = Path.GetFullPath("test_db.mdf");
            PhoneBookDatebase db = new PhoneBookDatebase(pathToDb);
            db.InitializeDatabase();

            db.OpenConnection();

            Assert.Equal(1, db.AddUser("Vasia", "Ivanov", "03.12.2012"));
            Assert.Equal(1, db.AddUser("Petia", "Petrov", "08.07.2000"));

            SqlCommand getUserIdsCommand = new SqlCommand("SELECT Id FROM Users", db.DatabaseConnection);

            SqlDataReader reader = getUserIdsCommand.ExecuteReader();
            List<int> userIds = new List<int>();

            while (reader.Read())
            {
                userIds.Add(reader.GetInt32(0));
            }

            reader.Close();

            Assert.Equal(2, userIds.Count);

            Assert.NotEqual(userIds[0], userIds[1]);

            db.CloseConnection();
            db.DeleteDatabase();
        }

        [Fact]
        public void CheckUserDeleted()
        {
            string pathToDb = Path.GetFullPath("test_db.mdf");
            PhoneBookDatebase db = new PhoneBookDatebase(pathToDb);
            db.InitializeDatabase();

            db.OpenConnection();

            Assert.Equal(1, db.AddUser("Vasia", "Ivanov", "03.12.2012"));
            Assert.Equal(1, db.AddPhoneNumber("1", "+375259002315"));

            Assert.Equal(1, db.DeleteUser("Vasia"));

            SqlCommand getUsersCommand = new SqlCommand("SELECT * FROM Users", db.DatabaseConnection);
            SqlDataReader getUsersReader = getUsersCommand.ExecuteReader();

            Assert.False(getUsersReader.HasRows);

            getUsersReader.Close();

            SqlCommand getPhoneNumbersCommand = new SqlCommand("SELECT * FROM PhoneNumbers", db.DatabaseConnection);
            SqlDataReader getPhoneNumbersReader = getPhoneNumbersCommand.ExecuteReader();

            Assert.False(getPhoneNumbersReader.HasRows);

            getPhoneNumbersReader.Close();

            db.CloseConnection();
            db.DeleteDatabase();
        }
    }
}
