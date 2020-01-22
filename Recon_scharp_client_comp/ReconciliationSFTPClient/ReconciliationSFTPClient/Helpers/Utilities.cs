using Oracle.DataAccess.Client;
using ReconSCHARPClient.Models;
using System;
using System.Configuration;
using System.Data;
using System.Text;

namespace ReconSCHARPClient.Helpers
{
    public class Utilities
    {
        internal static OracleConnection GetOracleConnection()
        {
            return new OracleConnection(ConfigurationManager.AppSettings["OracleConnectionString"]);
        }

        public static string GenerateBatchID()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            StringBuilder code = new StringBuilder();
            code.Append(DateTime.Now.ToString("yyyy-MM-dd"));
            code.Append(string.Format("{0:x}", i - DateTime.Now.Ticks));

            return code.ToString();
        }

        public static void InsertScharpBatchLog(BatchLog log)
        {
            using (var connection = GetOracleConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = Constant.SP_InsertSFTP_BatchLog;
                    command.CommandType = CommandType.StoredProcedure;
                    command.BindByName = true;

                    command.Parameters.Add("batchID", OracleDbType.Varchar2, log.BatchID, ParameterDirection.Input);
                    command.Parameters.Add("fileSource", OracleDbType.Varchar2, log.FileSource, ParameterDirection.Input);
                    command.Parameters.Add("fileName", OracleDbType.Varchar2, log.FileName, ParameterDirection.Input);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateScharpBatchLog(BatchLog log)
        {
            using (var connection = GetOracleConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = Constant.SP_UpdateSFTP_BatchLog;
                    command.CommandType = CommandType.StoredProcedure;
                    command.BindByName = true;

                    command.Parameters.Add("p_batchTransactionStatus", OracleDbType.Varchar2, log.BatchTransactionStatus, ParameterDirection.Input);
                    command.Parameters.Add("p_batchException", OracleDbType.Varchar2, log.BatchException, ParameterDirection.Input);
                    command.Parameters.Add("p_batchID", OracleDbType.Varchar2, log.BatchID, ParameterDirection.Input);

                    command.ExecuteNonQuery();
                }
            }

        }

        public static void UpdateScharpBatchLog(OracleConnection connection, BatchLog log)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = Constant.SP_UpdateSFTP_BatchLog;
                command.CommandType = CommandType.StoredProcedure;
                command.BindByName = true;

                command.Parameters.Add("p_batchTransactionStatus", OracleDbType.Varchar2, log.BatchTransactionStatus, ParameterDirection.Input);
                command.Parameters.Add("p_batchException", OracleDbType.Varchar2, log.BatchException, ParameterDirection.Input);
                command.Parameters.Add("p_batchID", OracleDbType.Varchar2, log.BatchID, ParameterDirection.Input);

                command.ExecuteNonQuery();
            }
        }

    }
}
