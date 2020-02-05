using ADONETgeneric;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace ADONETtest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DbProviderFactories.RegisterFactory("sqlserver", SqlClientFactory.Instance);
            string connectionString = "Data Source=aocws947;Initial Catalog=adresBeheer;Integrated Security=True";
            DbProviderFactory sqlFactory = DbProviderFactories.GetFactory("sqlserver");

            DataBeheer db = new DataBeheer(sqlFactory,connectionString);
            Cursus c1 = new Cursus("Web1");
            db.VoegCursusToe(c1);
            //using(DbConnection connection = sqlFactory.CreateConnection())
            //{
            //    if (connection == null)
            //    {
            //        Console.WriteLine("no connection");
            //        return;
            //    }
            //    connection.ConnectionString = connectionString;
            //    connection.Open();

            //    connection.Close();
            //}
        }
    }
}
