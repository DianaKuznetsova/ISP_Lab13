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

            db.OutputDB();

            Console.WriteLine("Добавлено объектов: {0}", db.AddUser("Tom", "B", "09.03.1999"));
            db.OutputDB();
            Console.WriteLine("Добавлено объектов: {0}", db.AddPhoneNumber("1", "+375257001105"));
            db.OutputDB();
            Console.WriteLine("Обновлено объектов: {0}", db.ChangeUserLastName("Tom", "Bin"));
            db.OutputDB();
            Console.WriteLine("Удалено объектов: {0}", db.DeleteUser("Tom"));
            db.OutputDB();

            db.CloseConnection();
            Console.WriteLine("Подключение закрыто...");
            Console.Read();

            db.DeleteDatabase();
        }
    }
}
