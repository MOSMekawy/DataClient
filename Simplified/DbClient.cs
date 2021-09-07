using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Simplified.Exceptions;

namespace Sql
{
    public class DbClient<TEntity> 
    {

        /* 
         
           EXAMPLE USE CASE OF API 
        
           DbClient<Person> client = new DbClient<Person>();
        
           Person p = client.ExecStoredProcedure("GetMinaisy").Apply({
            {"@job", "dev"}
            {"@age", 29}
           }).Map((data) => new Person() {  // Alternatively this lambda function can be saved and just refrenced instead of redefining it
            Name = data["@name"],
            Job = data["@job"],
            Age = data["@age"],
            Kpi = data["@assessment"]
           }).SingleOutput();
         
           -----------------------------------------------------------------------------------------------------------------------------------

           private Func<SqlDataReader, Person> mapFunctor = (data) => new Person() { 
            Job = data["@job"],
            Age = data["@age"],
            Kpi = data["@assessment"]
           };

           Person p = client.ExecStoredProcedure("GetDevsByAge").Apply({
            {"@age", 29}
           }).Map(mapFunctor).SingleOutput();
        
        */

        private string ConnectionString;
        private SqlConnection ContextConnection;
        private SqlCommand CurrentCommand;
        private SqlDataReader Reader;
        private TEntity Retrieved;
        private List<TEntity> MultipleRetrieved = new List<TEntity>();
        public DbClient(string connString)
        {
            ConnectionString = connString;
        }
        public DbClient<TEntity> ExecStoredProcedure(string procName)
        {
            ContextConnection = new SqlConnection(ConnectionString);
            CurrentCommand = new SqlCommand(procName, ContextConnection)
            {
                CommandType = CommandType.StoredProcedure
            };

            Reader = null;
            Retrieved = default;
            MultipleRetrieved = new List<TEntity>();

            return this;
        }

        public DbClient<TEntity> Apply(dynamic[,] parameters)
        {

            if (CurrentCommand == null) throw new InvalidChainOrderException();

            for(int i = 0; i < parameters.Length; i++)
            {

               CurrentCommand.Parameters.AddWithValue(parameters[i, 0], parameters[i, 1]);
               
            }

            return this;
        }

        public DbClient<TEntity> Map(Func<SqlDataReader, TEntity> map)
        {
            if (ContextConnection == null || CurrentCommand == null) throw new InvalidChainOrderException();

            ContextConnection.Open();
            Reader = CurrentCommand.ExecuteReader();

            if (Reader.Read())
            {
                Retrieved = map(Reader);
            }

            return this;
        }

        public DbClient<TEntity> MultiMap(Func<SqlDataReader, TEntity> map)
        {
            if (ContextConnection == null || CurrentCommand == null) throw new InvalidChainOrderException();

            ContextConnection.Open();
            Reader = CurrentCommand.ExecuteReader();

            while (Reader.Read())
            {
                MultipleRetrieved.Add(map(Reader));
            }

            return this;
        }
        public TEntity SingleOutput()
        {
            ContextConnection.Close();
            CurrentCommand.Dispose();

            return Retrieved;
        }
        public List<TEntity> MultiOutput()
        {
            ContextConnection.Close();
            CurrentCommand.Dispose();

            return MultipleRetrieved;
        }
    }
}

