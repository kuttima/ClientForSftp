using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Configuration;
using ReconSCHARPClient.Models;
using ReconSCHARPClient.Helpers;
using System.Xml.Linq;
using Serilog;

namespace ReconSCHARPClient
{
    class Program
    {
        private static string hostName = ConfigurationManager.AppSettings["HostName"];
        private static string userIdSCHARP = ConfigurationManager.AppSettings["UserIdSCHARP"];
        private static string pwdSCHARP = ConfigurationManager.AppSettings["PwdSCHARP"];
        private static string localDestinationFolder = ConfigurationManager.AppSettings["LocalDestinationFolder"];
        private static string archiveFolder = ConfigurationManager.AppSettings["ArchiveFolder"];
        private static string errorFolder = ConfigurationManager.AppSettings["ErrorFolder"];
        private static string sFTPlogFile = ConfigurationManager.AppSettings["SFTPlogFile"];

        static void Main(string[] args)
        {
            string path = sFTPlogFile;
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(path: path,
                  rollOnFileSizeLimit: true,
                  fileSizeLimitBytes: 20971520)
                .CreateLogger();

            using (var client = GetSftpClient(hostName, userIdSCHARP, pwdSCHARP))
            {
                //connect to SCHARP
                client.Connect();
                bool error = false;
                try
                {
                    var newFiles = DownloadFiles(client, localDestinationFolder);
                    foreach (var file in newFiles)
                    {
                        string fileName = localDestinationFolder + file.Name;
                        var summaries = DeserializeXmlToPOCO(fileName);
                        SaveSCHARPdata(summaries, file.Name);
                        MoveFileToArchiveFolder(client, file, archiveFolder);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("DateTime: {0}, Message: {1}", DateTime.Now, e.Message);
                    error = true;
                }
                finally
                {
                    DeleteDirectory(localDestinationFolder);
                    MoveFilesToErrorFolder(client, errorFolder);
                    if (client.IsConnected)
                        client.Disconnect();
                    Log.CloseAndFlush();
                    string status = "successfully";
                    if (error)
                    {
                        status = "with errors, check log file for details";
                    }
                    Console.WriteLine("Program execution completed {0}......", status);
                    Console.Read();
                }               
            }        
    }


    private static List<SftpFile> DownloadFiles(SftpClient sftpClient, string destLocalPath)
    {
        Directory.CreateDirectory(destLocalPath);
        IEnumerable<SftpFile> files = sftpClient.ListDirectory(".");
        List<SftpFile> NewFiles = new List<SftpFile>();

        foreach (SftpFile file in files)
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                string sourceFilePath = file.Name;
                string destFilePath = Path.Combine(destLocalPath, file.Name);
                if (file.IsRegularFile)
                {
                    using (Stream fileStream = File.Create(destFilePath))
                    {
                        sftpClient.DownloadFile(sourceFilePath, fileStream);
                        NewFiles.Add(file);
                    }
                }
            }
        }

        return NewFiles;
    }
    private static void DeleteDirectory(string dirToDelete)
    {
        string[] files = Directory.GetFiles(dirToDelete);
        string[] dirs = Directory.GetDirectories(dirToDelete);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(dirToDelete, false);
    }

    private static List<EAE> DeserializeXmlToPOCO(string filename)
    {
        Serializer ser = new Serializer();
        XDocument doc = XDocument.Load(filename);
        ListOfEaEs EAEs = ser.Deserialize<ListOfEaEs>(doc, "EAEs");
        //manually insert participant identifier, which is unique to each EAE, for the ICH seriousness criterion table.
        foreach (var eae in EAEs)
        {
            foreach (var criterion in eae.SeriousnessCriteria)
            {
                criterion.ParticipantIdentifier = eae.ParticipantIdentifier;
            }
        }
        return EAEs;
    }
   

    private static void SaveSCHARPdata(List<EAE> summaries, string filename)
    {
        string _batchId = Constant.SCHARP + Utilities.GenerateBatchID();

        BatchLog log = new BatchLog
        {
            BatchID = _batchId,
            FileSource = Constant.SCHARP,
            FileName = filename
        };
        Utilities.InsertScharpBatchLog(log);

        if (summaries != null && summaries.Count() != 0)
        {
            using (var connection = Utilities.GetOracleConnection())
            {
                connection.Open();
                BulkCopyOracle.InsertSCHARPdataUsingArrayBinding(summaries, connection, _batchId);
            }
        }
    }

    private static void MoveFileToArchiveFolder(SftpClient sftp, SftpFile remoteFile, string ftpPathDestFolder)
    {        
            if (remoteFile.IsRegularFile)
            {
                //MoveTo will result in error if filename alredy exists in the target folder. Prevent that error by cheking if File name exists
                string eachFileNameInArchive = remoteFile.Name;
                if (CheckIfRemoteFileExists(sftp, ftpPathDestFolder, remoteFile.Name))
                {
                    eachFileNameInArchive = eachFileNameInArchive + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");//Change file name if the file already exists
                }

                remoteFile.MoveTo(ftpPathDestFolder + eachFileNameInArchive);
            }        
    }

    private static void MoveFilesToErrorFolder(SftpClient sftp, string ftpPathDestFolder)
    {
        IEnumerable<SftpFile> files = sftp.ListDirectory(".");
        List<SftpFile> NewFiles = new List<SftpFile>();

        foreach (SftpFile file in files)
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                if (file.IsRegularFile)
                {
                        string eachFileNameInError = file.Name;
                        if (CheckIfRemoteFileExists(sftp, ftpPathDestFolder, file.Name))
                        {
                            eachFileNameInError = eachFileNameInError + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");//Change file name if the file already exists
                        }

                        file.MoveTo(ftpPathDestFolder + eachFileNameInError);
                }
            }
        }
    }

    /// <summary>
    /// Checks if Remote folder contains the given file name
    /// </summary>
    private static bool CheckIfRemoteFileExists(SftpClient sftpClient, string remoteFolderName, string remotefileName)
    {
        bool isFileExists = sftpClient
                            .ListDirectory(remoteFolderName)
                            .Any(
                                    f => f.IsRegularFile &&
                                    f.Name.ToLower() == remotefileName.ToLower()
                                );
        return isFileExists;
    }

    private static SftpClient GetSftpClient(string sftpHostName, string sftpUserID, string sftpPassword)
    {
        return new SftpClient(sftpHostName, sftpUserID, sftpPassword);
    }

}
}
