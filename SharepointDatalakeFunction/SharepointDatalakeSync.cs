using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using SharepointDatalakeConnector.Service.ConfigModels;
using SharepointDatalakeConnector.Service.Interfaces;
using SharepointDatalakeConnector.Service.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SharepointDatalakeFunction
{
    public class SharepointDatalakeSyncFunction
    {
        private readonly ISharepointService _sharepointService;
        private readonly ISqlService _sqlService;

        private readonly IDatalakeService _datalakeService;
        private readonly ILogger<SharepointDatalakeSyncFunction> _log;
        private readonly DataLakeSettings _datalakeSettings;
        private readonly SharepointSettings _sharepointSettings;
        private readonly DatabaseSettings _databaseSettings;

        private readonly IPnPContextFactory _contextFactory;

        public SharepointDatalakeSyncFunction(
            ISharepointService sharepointService,
            ISqlService sqlService,
            IDatalakeService datalakeService,
            ILogger<SharepointDatalakeSyncFunction> log,
            IOptions<DataLakeSettings> datalakeSettings,
            IOptions<SharepointSettings> sharepointSettings,
            IOptions<DatabaseSettings> databaseSettings,
            IPnPContextFactory contextFactory)
        {
            _sharepointService = sharepointService;
            _datalakeService = datalakeService;
            _log = log;
            _datalakeSettings = datalakeSettings.Value;
            _sharepointSettings = sharepointSettings.Value;
            _databaseSettings = databaseSettings.Value;
            _contextFactory = contextFactory;
            _sqlService = sqlService;
        }


        [FunctionName("SharepointDatalakeSync")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string sidcode = req.Query["ext"];
            string fromDatetime = req.Query["fromDatetime"];
            string folder = req.Query["folder"];

            var connectionString = _databaseSettings.Base;

            _log.LogInformation("C# HTTP trigger function processed a request.");

            var files = await _sharepointService.GetFileByExtensionFromDocumentLibraryAsync(_sharepointSettings.SiteUrl, sidcode, fromDatetime);

            var datalakeFileSystemClient = _datalakeService.GetDataLakeFileSystemClient(_datalakeSettings.AccountName, _datalakeSettings.AccountKey, _datalakeSettings.ContainerName);
            foreach ( var file in files) 
            {
                _log.LogInformation($"{file.FileLeafRef}");
                // download from spo
                var byteArray = await _sharepointService.DownloadFileFromSharepointAsync(_sharepointSettings.SiteUrl, file.FileRef);

                //remove filename from pricipal dir
                var fileRefModified = file.FileRef.Replace($"{file.FileLeafRef}", "");

                //upload to datalake v2
                await _datalakeService.UploadFileBulkAsync(datalakeFileSystemClient, fileRefModified, file.FileLeafRef, byteArray);
                _sqlService.ExecuteStoreProcedure(connectionString, "SacMi.sp_InsertCopyLog", file.FileLeafRef, fileRefModified, file.FileRef);

            }
            return new OkResult();
        }
    }
}
