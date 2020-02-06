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

        public DataBeheer(DbProviderFactory sqlFactory, string connectionString)
        {
            this.sqlFactory = sqlFactory;
            this.connectionString = connectionString;           
        }
        private DbConnection getConnection()
        {
            DbConnection connection = sqlFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }
        public void VoegCursusToe(Cursus c)
        {
            DbConnection connection = getConnection();
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
            DbConnection connection = getConnection();
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
            DbConnection connection = getConnection();
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
            DbConnection connection = getConnection();
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
            DbConnection connection = getConnection();
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
        //    DbConnection connection = getConnection();
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
            DbConnection connection = getConnection();
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
            DbConnection connection = getConnection();
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
            DbConnection connection = getConnection();
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
                    reader.Close();
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
        public Student GeefStudent(int id)
        {
            DbConnection connection = getConnection();
            string queryS = "SELECT * FROM dbo.student WHERE id=@id";
            string querySC = "SELECT * FROM [adresBeheer].[dbo].[cursus] t1,[adresBeheer].[dbo].[student_cursus] t2 "
                +"where t1.Id = t2.cursusid and t2.studentid = @id";
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = queryS;
                DbParameter paramId = sqlFactory.CreateParameter();
                paramId.ParameterName = "@Id";
                paramId.DbType = DbType.Int32;
                paramId.Value = id;
                command.Parameters.Add(paramId);
                connection.Open();
                try
                {
                    DbDataReader reader = command.ExecuteReader();
                    reader.Read();
                    int studentId = (int)reader["Id"];
                    string studentnaam = (string)reader["naam"];
                    int klasId = (int)reader["klasId"];
                    reader.Close();
                    Klas klas = GeefKlas(klasId);
                    Student student = new Student(studentId,studentnaam,klas);

                    command.CommandText = querySC;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Cursus cursus = new Cursus(reader.GetInt32(0), reader.GetString(1));
                        student.voegCursusToe(cursus);
                    }
                    reader.Close();
                    return student;
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
        public Cursus GeefCursus(int id)
        {
            DbConnection connection = getConnection();
            string query = "SELECT * FROM dbo.cursus WHERE id=@id";
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
                    DbDataReader reader = command.ExecuteReader();
                    reader.Read();
                    Cursus cursus = new Cursus((int)reader["Id"], (string)reader["cursusnaam"]);
                    reader.Close();
                    return cursus;
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
        public void UpdateCursus(Cursus c)
        {
            DbConnection connection = getConnection();
            Cursus cursusDB = GeefCursus(c.id);
            string query = "SELECT * FROM dbo.cursus WHERE Id=@Id";
           
            using (DbDataAdapter adapter = sqlFactory.CreateDataAdapter())
            {
                try
                {
                    DbParameter paramId = sqlFactory.CreateParameter();
                    paramId.ParameterName = "@Id";
                    paramId.DbType = DbType.Int32;
                    paramId.Value = c.id;
                    DbCommandBuilder builder = sqlFactory.CreateCommandBuilder();
                    builder.DataAdapter = adapter;
                    adapter.SelectCommand = sqlFactory.CreateCommand();
                    adapter.SelectCommand.CommandText = query;
                    adapter.SelectCommand.Connection = connection;
                    adapter.SelectCommand.Parameters.Add(paramId);
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    table.Rows[0]["cursusnaam"] = c.cursusnaam;
                    
                    adapter.Update(table);
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
        public void VerwijderCursussen(List<int> ids)
        {
            string query = "SELECT * FROM dbo.cursus";
            DataSet ds = new DataSet();
            DbConnection connection = getConnection();
            using(DbDataAdapter adapter=sqlFactory.CreateDataAdapter())
            {
                try
                {
                    DbCommandBuilder builder = sqlFactory.CreateCommandBuilder();
                    builder.DataAdapter = adapter;
                    adapter.SelectCommand = sqlFactory.CreateCommand();
                    adapter.SelectCommand.CommandText = query;
                    adapter.SelectCommand.Connection = connection;
                    adapter.DeleteCommand = builder.GetDeleteCommand();
                    adapter.FillSchema(ds, SchemaType.Source, "cursus");
                    adapter.Fill(ds, "cursus");
                    
                    foreach (int id in ids)
                    {
                        DataRow r = ds.Tables["cursus"].Rows.Find(id);
                        r.Delete();
                    }
                    adapter.Update(ds, "cursus");
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
