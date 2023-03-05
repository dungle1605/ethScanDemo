using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanDemo
{
    public class DbIntegration
    {
        private readonly string _connectionString = Environment.GetEnvironmentVariable("ConnectionString");

        public bool ExecuteCommand(string sppName, MySqlParameter[] lstParam)
        {
            using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();
                using (MySqlCommand command = mySqlConnection.CreateCommand())
                {
                    var transaction = mySqlConnection.BeginTransaction();
                    try
                    { 
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = sppName;
                        command.Parameters.AddRange(lstParam);
                        
                        command.ExecuteNonQueryAsync();
                        transaction.Commit();

                        return true;
                    } 
                    catch(MySqlException)
                    {
                        transaction.Rollback();
                        return false;
                    }
                        
                }
            }

            return false;
            
        }
    }
}
