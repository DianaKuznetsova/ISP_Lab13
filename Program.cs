using System;
using System.Data.SqlClient;
using System.IO;

namespace ISP_Lab13
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathToDbFile = Path.GetFullPath("../../../PhoneBookDb.mdf");
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + pathToDbFile + ";Trusted_Connection=Yes;MultipleActiveResultSets=True";
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                Console.WriteLine("Подключение открыто");
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
           
            OutputDB(connection);
            string sqlExpressionInsert = "INSERT INTO Users (FirstName, LastName, Birthday) VALUES ('Tom', 'B', '09.03.1999')";        
            string sqlExpressionUpdate = "UPDATE Users SET LastName='Bin' WHERE FirstName='Tom'";
            string sqlExpressionDelete = "DELETE  FROM Users WHERE FirstName='Tom'";
            
            Console.WriteLine("Добавлено объектов: {0}", ExecuteQuery(sqlExpressionInsert, connection));
            OutputDB(connection);
            Console.WriteLine("Обновлено объектов: {0}", ExecuteQuery(sqlExpressionUpdate, connection));
            OutputDB(connection);
            Console.WriteLine("Удалено объектов: {0}", ExecuteQuery(sqlExpressionDelete, connection));
            OutputDB(connection);
            
            connection.Close();
            Console.WriteLine("Подключение закрыто...");
            Console.Read();
        }

       public static int ExecuteQuery(string sqlExpression, SqlConnection connection)
        {
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            return command.ExecuteNonQuery();
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
                    if (phonesReader.HasRows) {
                        Console.WriteLine("{0, 5} {1, 20}", phonesReader.GetName(0), phonesReader.GetName(1));
                        while (phonesReader.Read())
                        {
                            Console.WriteLine("{0, 5} {1, 20}", phonesReader.GetValue(0), phonesReader.GetValue(1));
                        }
                    } else
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
