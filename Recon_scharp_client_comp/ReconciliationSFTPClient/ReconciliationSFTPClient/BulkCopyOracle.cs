using System;
using System.Collections.Generic;
using ReconSCHARPClient.Models;
using Oracle.DataAccess.Client;
using System.Linq;
using System.Data;
using ReconSCHARPClient.Helpers;

namespace ReconSCHARPClient
{
    public static class BulkCopyOracle
    {
        internal static void InsertSCHARPdataUsingArrayBinding(List<EAE> Summaries, OracleConnection connection, string batchId)
        {
            using (OracleTransaction transaction = connection.BeginTransaction())
            {
                string[] _BatchID = new string[Summaries.Count];
                _BatchID = Populate(_BatchID, batchId);
                string[] _EAENumber = Summaries.Select(s => s.EAENumber).ToArray();
                string[] _ProtocolNumber = Summaries.Select(s => s.ProtocolNumber).ToArray();
                string[] _ParticipantIdentifier = Summaries.Select(s => s.ParticipantIdentifier).ToArray();
                string[] _PrimaryAE = Summaries.Select(s => s.PrimaryAE).ToArray();
                string[] _MEDDRACodePT = Summaries.Select(s => s.MedDRACodePT).ToArray();
                string[] _OnsetDate = Summaries.Select(s => s.OnsetDate).ToArray();
                string[] _RelationToPrimaryAE = Summaries.Select(s => s.RelationshipToPrimaryAE).ToArray();
                string[] _SeverityGrade = Summaries.Select(s => s.SeverityGrade).ToArray();
                string[] _DateOfDeath = Summaries.Select(s => s.DateOfDeath).ToArray();

                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        //string query = "INSERT INTO SCHARP_EAESUMMARY(BATCHID,CREATEDDATE,DOCUMENTID, EAENUMBER, PROTOCOLNUMBER, PARTICIPANTIDENTIFIER, PRIMARYAE, MEDDRACODEPT, ONSETDATE, RELATIONTOPRIMARYAE, SEVERITYGRADE, DATEOFDEATH)VALUES(:BatchID, sysdate,:DocumentID,:EAENumber,:ProtocolNumber,:ParticipantIdentifier,:PrimaryAE,:MEDDRAcodePT,:OnsetDate,:RelationToPrimaryAE,:SeverityGrade,:DateOfDeath)";
                        command.CommandText = Constant.SP_InsertScharp_EAESummary;
                        command.CommandType = CommandType.StoredProcedure;
                        command.BindByName = true;
                        command.ArrayBindCount = Summaries.Count();

                        command.Parameters.Add("BatchID", OracleDbType.Varchar2, _BatchID, ParameterDirection.Input);
                        command.Parameters.Add("EAENumber", OracleDbType.Varchar2, _EAENumber, ParameterDirection.Input);
                        command.Parameters.Add("ProtocolNumber", OracleDbType.Varchar2, _ProtocolNumber, ParameterDirection.Input);
                        command.Parameters.Add("ParticipantIdentifier", OracleDbType.Varchar2, _ParticipantIdentifier, ParameterDirection.Input);
                        command.Parameters.Add("PrimaryAE", OracleDbType.Varchar2, _PrimaryAE, ParameterDirection.Input);
                        command.Parameters.Add("MEDDRACodePT", OracleDbType.Varchar2, _MEDDRACodePT, ParameterDirection.Input);
                        command.Parameters.Add("OnsetDate", OracleDbType.Varchar2, _OnsetDate, ParameterDirection.Input);
                        command.Parameters.Add("RelationToPrimaryAE", OracleDbType.Varchar2, _RelationToPrimaryAE, ParameterDirection.Input);
                        command.Parameters.Add("SeverityGrade", OracleDbType.Varchar2, _SeverityGrade, ParameterDirection.Input);
                        command.Parameters.Add("DateOfDeath", OracleDbType.Varchar2, _DateOfDeath, ParameterDirection.Input);

                        command.ExecuteNonQuery();
                    }

                    //Insert ICHSeriousnessCriteria
                    var seriousnessCriteria = Summaries.SelectMany(i => i.SeriousnessCriteria);
                    if (seriousnessCriteria.Count() != 0)
                    {
                        string[] _sBatchID = new string[seriousnessCriteria.Count()];
                        _sBatchID = Populate(_sBatchID, batchId);
                        string[] _sParticipantIdentifier = seriousnessCriteria.Select(s => s.ParticipantIdentifier).ToArray();
                        string[] _sSeriousnessDescription = seriousnessCriteria.Select(s => s.ICHSeriousnessCriterionDescription).ToArray();

                        using (var command = connection.CreateCommand())
                        {
                            //string query = "INSERT INTO SCHARP_ICHSERIOUSNESSCRITERION (BATCHID,CREATEDDATE,PARTICIPANTIDENTIFIER,SERIOUSNESSDESCRIPTION)VALUES(:sBatchID, sysdate,:sParticipantIdentifier,:sSeriousnessDescription)";
                            command.CommandText = Constant.SP_InsertScharp_IchCriterion;
                            command.CommandType = CommandType.StoredProcedure;
                            command.BindByName = true;
                            command.ArrayBindCount = seriousnessCriteria.Count();

                            command.Parameters.Add("sBatchID", OracleDbType.Varchar2, _sBatchID, ParameterDirection.Input);
                            command.Parameters.Add("sParticipantIdentifier", OracleDbType.Varchar2, _sParticipantIdentifier, ParameterDirection.Input);
                            command.Parameters.Add("sSeriousnessDescription", OracleDbType.Varchar2, _sSeriousnessDescription, ParameterDirection.Input);

                            command.ExecuteNonQuery();

                        }
                    }

                    BatchLog log = new BatchLog
                    {
                        BatchID = batchId,
                        BatchTransactionStatus = "Success",
                        BatchException = ""
                    };
                    Utilities.UpdateScharpBatchLog(connection, log);
                    transaction.Commit();

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    connection.Close();
                    connection.Dispose();

                    BatchLog log = new BatchLog
                    {
                        BatchID = batchId,
                        BatchTransactionStatus = "Failure",
                        BatchException = e.Message
                    };
                    Utilities.UpdateScharpBatchLog(log);
                    throw;
                }               
            }
        }

        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
            return arr;
        }
    }
}