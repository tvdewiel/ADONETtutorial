using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Transactions;

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
        public void VoegKlasToe(Klas k)
        {
            string query = "INSERT INTO dbo.klas (klasnaam) VALUES(@klasnaam)";
            using (DbCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    DbParameter parNaam = sqlFactory.CreateParameter();
                    parNaam.ParameterName = "@klasnaam";
                    parNaam.DbType = DbType.String;
                    command.Parameters.Add(parNaam);
                    command.CommandText = query;
                    command.Parameters["@klasnaam"].Value = k.klasnaam;
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
        public void VoegKlassenToe(List<Klas> klist)
        {
            string query1 = "SELECT * FROM dbo.klas";
            try
            {
                connection.Open();
                using (DbDataAdapter adapter = sqlFactory.CreateDataAdapter())
                {
                    DbCommand command = sqlFactory.CreateCommand();
                    command.CommandText = query1;
                    command.Connection = connection;

                    adapter.SelectCommand = command;

                    DbCommandBuilder builder = sqlFactory.CreateCommandBuilder();
                    builder.DataAdapter = adapter;
                    adapter.InsertCommand = builder.GetInsertCommand();

                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    foreach (Klas k in klist)
                    {
                        DataRow row = table.NewRow();
                        row["klasnaam"] = k.klasnaam;
                        table.Rows.Add(row);
                    }
                    adapter.Update(table);
                }
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
        public void VoegStudentToe(Student s)
        {            
            string queryS = "INSERT INTO dbo.student(naam,klasId) VALUES(@naam,@klasId)";

            using (DbCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    DbParameter parNaam = sqlFactory.CreateParameter();
                    parNaam.ParameterName = "@naam";
                    parNaam.DbType = DbType.String;
                    command.Parameters.Add(parNaam);
                    DbParameter parKlas = sqlFactory.CreateParameter();
                    parKlas.ParameterName = "@klasId";
                    parKlas.DbType = DbType.Int32;
                    command.Parameters.Add(parKlas);

                    command.CommandText = queryS;
                    command.Parameters["@naam"].Value = s.naam;
                    command.Parameters["@klasId"].Value = s.klas.id;
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
        public void VoegStudentMetCursussenToe(Student s)
        {
            string queryS = "INSERT INTO dbo.student(naam,klasId) output INSERTED.ID VALUES(@naam,@klasId)";
            string querySC = "INSERT INTO dbo.student_cursus(cursusId,studentId) VALUES(@cursusId,@studentId)";

            using (DbCommand command1 = connection.CreateCommand())
            using (DbCommand command2 = connection.CreateCommand())
            {                
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                command1.Transaction = transaction;
                command2.Transaction = transaction;
                try
                {
                    //student toevoegen
                    DbParameter parNaam = sqlFactory.CreateParameter();
                    parNaam.ParameterName = "@naam";
                    parNaam.DbType = DbType.String;
                    command1.Parameters.Add(parNaam);
                    DbParameter parKlas = sqlFactory.CreateParameter();
                    parKlas.ParameterName = "@klasId";
                    parKlas.DbType = DbType.Int32;
                    command1.Parameters.Add(parKlas);
                    command1.CommandText = queryS;
                    command1.Parameters["@naam"].Value = s.naam;
                    command1.Parameters["@klasId"].Value = s.klas.id;
                    //command1.ExecuteNonQuery();
                    int newID = (int)command1.ExecuteScalar();
                    //Cursussen toevoegen
                    DbParameter parCID = sqlFactory.CreateParameter();
                    parCID.ParameterName = "@cursusId";
                    parCID.DbType = DbType.Int32;
                    command2.Parameters.Add(parCID);
                    DbParameter parSID = sqlFactory.CreateParameter();
                    parSID.ParameterName = "@studentId";
                    parSID.DbType = DbType.Int32;
                    command2.Parameters.Add(parSID);

                    command2.CommandText = querySC;
                    command2.Parameters["@studentId"].Value = newID;

                    foreach (var cursus in s.cursussen)
                    {
                        command2.Parameters["@cursusId"].Value = cursus.id;
                        command2.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        //public void VoegStudentMetCursussenToe(Student s)
        //{
        //    string queryS = "INSERT INTO dbo.student(naam,klasId) output INSERTED.ID VALUES(@naam,@klasId)";
        //    string querySC = "INSERT INTO dbo.student_cursus(cursusId,studentId) VALUES(@cursusId,@studentId)";

        //    using (var txscope = new TransactionScope(TransactionScopeOption.RequiresNew))
        //    using (DbCommand command1 = connection.CreateCommand())
        //    using (DbCommand command2 = connection.CreateCommand())
        //    {
        //        connection.Open();
        //        //DbTransaction transaction = connection.BeginTransaction();
        //        //command1.Transaction = transaction;
        //        //command2.Transaction = transaction;
        //        try
        //        {
        //            //student toevoegen
        //            DbParameter parNaam = sqlFactory.CreateParameter();
        //            parNaam.ParameterName = "@naam";
        //            parNaam.DbType = DbType.String;
        //            command1.Parameters.Add(parNaam);
        //            DbParameter parKlas = sqlFactory.CreateParameter();
        //            parKlas.ParameterName = "@klasId";
        //            parKlas.DbType = DbType.Int32;
        //            command1.Parameters.Add(parKlas);
        //            command1.CommandText = queryS;
        //            command1.Parameters["@naam"].Value = s.naam;
        //            command1.Parameters["@klasId"].Value = s.klas.id;
        //            //command1.ExecuteNonQuery();
        //            int newID = (int)command1.ExecuteScalar();
        //            //Cursussen toevoegen
        //            DbParameter parCID = sqlFactory.CreateParameter();
        //            parCID.ParameterName = "@cursusId";
        //            parCID.DbType = DbType.Int32;
        //            command2.Parameters.Add(parCID);
        //            DbParameter parSID = sqlFactory.CreateParameter();
        //            parSID.ParameterName = "@studentId";
        //            parSID.DbType = DbType.Int32;
        //            command2.Parameters.Add(parSID);

        //            command2.CommandText = querySC;
        //            command2.Parameters["@studentId"].Value = newID;

        //            foreach (var cursus in s.cursussen)
        //            {
        //                command2.Parameters["@cursusId"].Value = cursus.id;
        //                command2.ExecuteNonQuery();
        //            }
        //            //transaction.Commit();
        //            txscope.Complete();
        //        }
        //        catch (Exception ex)
        //        {
        //            //transaction.Rollback();
        //            txscope.Dispose();
        //            Console.WriteLine(ex);
        //        }
        //        finally
        //        {
        //            connection.Close();
        //        }
        //    }
        //}
        public void KoppelCursusAanStudent(int studentId,List<int> cursusIds)
        {
            string queryS = "INSERT INTO dbo.student_cursus(cursusId,studentId) VALUES(@cursusId,@studentId)";

            using (DbCommand command = connection.CreateCommand())
            {
                connection.Open();
                try
                {
                    DbParameter parCID = sqlFactory.CreateParameter();
                    parCID.ParameterName = "@cursusId";
                    parCID.DbType = DbType.Int32;
                    command.Parameters.Add(parCID);
                    DbParameter parSID = sqlFactory.CreateParameter();
                    parSID.ParameterName = "@studentId";
                    parSID.DbType = DbType.Int32;
                    command.Parameters.Add(parSID);

                    command.CommandText = queryS;
                    command.Parameters["@studentId"].Value = studentId;

                    foreach (int cursusId in cursusIds)
                    {
                        command.Parameters["@cursusId"].Value = cursusId;
                        command.ExecuteNonQuery();
                    }
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
        public IEnumerable<Cursus> GeefCursussen()
        {
            IList<Cursus> lg = new List<Cursus>();
            string query = "SELECT * FROM dbo.cursus";
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                connection.Open();
                try
                {
                    IDataReader dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        int id = (int)dataReader["id"];
                        string cursusnaam = dataReader.GetString(1); //verschillende methodes om data op te vragen !
                        lg.Add(new Cursus(id, cursusnaam));
                    }
                    dataReader.Close();
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
            return lg;
        }
        public Klas GeefKlas(int id)
        {
            string query = "SELECT * FROM dbo.klas WHERE id=@id";
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = query;
                DbParameter paramId = sqlFactory.CreateParameter();
                paramId.ParameterName = "@Id";
                paramId.DbType = DbType.Int32;
                paramId.Value = id;
                command.Parameters.Add(paramId);
                connection.Open();
                try
                {
                    DbDataReader reader=command.ExecuteReader();
                    reader.Read();
                    Klas klas = new Klas((int)reader["Id"],(string)reader["klasnaam"]);
                    return klas;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
