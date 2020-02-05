using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace ADONETgeneric
{
    public class DataBeheer
    {
        private DbProviderFactory sqlFactory;
        private string connectionString;
        private DbConnection connection;

        public DataBeheer(DbProviderFactory sqlFactory, string connectionString)
        {
            this.sqlFactory = sqlFactory;
            this.connectionString = connectionString;
            connection = sqlFactory.CreateConnection();
            connection.ConnectionString = connectionString;
        }
        public void VoegCursusToe(Cursus c)
        {
            string query = "INSERT INTO dbo.cursus (cursusnaam) VALUES(@cursusnaam)";
            using (DbCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    DbParameter parNaam = sqlFactory.CreateParameter();
                    parNaam.ParameterName = "@cursusnaam";
                    parNaam.DbType = DbType.String;
                    command.Parameters.Add(parNaam);
                    command.CommandText = query;
                    command.Parameters["@cursusnaam"].Value = c.cursusnaam;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
