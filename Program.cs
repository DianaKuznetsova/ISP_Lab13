using System;
using System.Data.SqlClient;
using System.IO;

namespace ISP_Lab13
{
    public class Program
    {
        static void Main(string[] args)
        {
            string pathToDbFile = Path.GetFullPath("PhoneBookDb.mdf");
            PhoneBookDatebase db = new PhoneBookDatebase(pathToDbFile);
            db.InitializeDatabase();

            try
            {
                db.OpenConnection();
                Console.WriteLine("Подключение открыто");
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            OutputDB(db.DatabaseConnection);
            string sqlExpressionInsert = "INSERT INTO Users (FirstName, LastName, Birthday) VALUES ('Tom', 'B', '09.03.1999')";
            string sqlExpressionInsertPhone = "INSERT INTO PhoneNumbers (UserId, Number) VALUES ('1', '+375257001105')";
            string sqlExpressionUpdate = "UPDATE Users SET LastName='Bin' WHERE FirstName='Tom'";
            string sqlExpressionDelete = "DELETE  FROM Users WHERE FirstName='Tom'";

            Console.WriteLine("Добавлено объектов: {0}", db.ExecuteQuery(sqlExpressionInsert));
            OutputDB(db.DatabaseConnection);
            Console.WriteLine("Добавлено объектов: {0}", db.ExecuteQuery(sqlExpressionInsertPhone));
            OutputDB(db.DatabaseConnection);
            Console.WriteLine("Обновлено объектов: {0}", db.ExecuteQuery(sqlExpressionUpdate));
            OutputDB(db.DatabaseConnection);
            Console.WriteLine("Удалено объектов: {0}", db.ExecuteQuery(sqlExpressionDelete));
            OutputDB(db.DatabaseConnection);

            db.CloseConnection();
            Console.WriteLine("Подключение закрыто...");
            Console.Read();

            db.DeleteDatabase();
        }

        public static void OutputDB(SqlConnection connection)
        {

            string getUsersExpression = "SELECT * FROM Users";
            string getUserPhoneNumbersExpression = "SELECT Id, Number FROM PhoneNumbers WHERE UserId = @userId";
            SqlCommand command = new SqlCommand(getUsersExpression, connection);
            SqlDataReader reader = command.ExecuteReader();

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

                    SqlCommand phonesCommand = new SqlCommand(getUserPhoneNumbersExpression, connection);
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
    }
}
