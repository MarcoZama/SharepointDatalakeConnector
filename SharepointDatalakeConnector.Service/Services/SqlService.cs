using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage;
using System.IO;
using SharepointDatalakeConnector.Service.ConfigModels;
using SharepointDatalakeConnector.Service.Interfaces;
using System.IO.Pipes;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SharepointDatalakeConnector.Service.Services
{
    public class SqlService : ISqlService
    {
        private readonly ILogger _log;
        private readonly DataLakeSettings _datalakeSettings;

        public SqlService(
            ILogger<SharepointService> log)
        {
            _log = log;
      
        }


        public void ExecuteStoreProcedure(string connString, string storedProcedureName, string FileLeafRef, string FileRefModified, string FileRef)
        {
            try
            {
                _log.LogInformation($"Execute SP {storedProcedureName} in database");



                using (SqlConnection connection = new SqlConnection(connString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        // Add parameters if needed
                        command.Parameters.AddWithValue("@FileLeafRef", $"{FileLeafRef}");
                        command.Parameters.AddWithValue("@FileRefModified", $"{FileRefModified}");
                        command.Parameters.AddWithValue("@FileRef", $"{FileRef}");
                        connection.Open();
                        // Execute the stored procedure
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
               }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error SP {storedProcedureName} in database {ex.Message}");
                throw;
            }
        

        }


    }
}