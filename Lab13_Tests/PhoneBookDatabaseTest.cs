using System;
using System.IO;
using System.Data.SqlClient;
using Xunit;
using ISP_Lab13;
using System.Data;
using System.Collections.Generic;

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
                string tableName = (string) column["TABLE_NAME"];
                string columnName = (string) column["COLUMN_NAME"];
                if (tableName.Equals("Users"))
                {
                    usersColumns.Add(columnName);
                } else if (tableName.Equals("PhoneNumbers"))
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
    }
}
