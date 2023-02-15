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

namespace SharepointDatalakeConnector.Service.Services
{
    public class DatalakeService : IDatalakeService
    {
        private readonly IPnPContextFactory _pnPContextFactory;
        private readonly ILogger<SharepointService> _log;
        private readonly DataLakeSettings _datalakeSettings;


        public DatalakeService(
            ILogger<SharepointService> log,
            IOptions<DataLakeSettings> datalakeOptions,
            IPnPContextFactory pnPContextFactory)
        {
            _log = log;
            _datalakeSettings = datalakeOptions.Value;
            _pnPContextFactory = pnPContextFactory;

        }


        public async Task UploadFileBulkAsync(DataLakeFileSystemClient fileSystemClient, string directory, string fileName, byte[] byteArray)
        {
            try
            {
                DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(directory);

                DataLakeFileClient fileClient = directoryClient.GetFileClient(fileName);
                using (var stream = new MemoryStream(byteArray))
                {
                    await fileClient.UploadAsync(stream);
                }                
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message, ex);
                throw;
            }
        

        }

        public async Task UploadFileAsync(DataLakeFileSystemClient fileSystemClient, string directory, string fileName, FileStream filestream)
        {
            DataLakeDirectoryClient directoryClient =
                fileSystemClient.GetDirectoryClient(directory);

            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync(fileName);

            long fileSize = filestream.Length;

            await fileClient.AppendAsync(filestream, offset: 0);

            await fileClient.FlushAsync(position: fileSize);

        }

        public DataLakeFileSystemClient GetDataLakeFileSystemClient(string accountName, string accountKey, string container)
        {
            StorageSharedKeyCredential sharedKeyCredential =
                new StorageSharedKeyCredential(accountName, accountKey);

            string dfsUri = "https://" + accountName + ".dfs.core.windows.net";

            var dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);

            return dataLakeServiceClient.GetFileSystemClient(container);
        }

        public async Task<List<PathItem>> ListFilesInDirectoryAsync(DataLakeFileSystemClient fileSystemClient, string directory)
        {
            IAsyncEnumerator<PathItem> enumerator =
                fileSystemClient.GetPathsAsync(directory).GetAsyncEnumerator();

            await enumerator.MoveNextAsync();

            PathItem item = enumerator.Current;
            var items = new List<PathItem>();
            while (item != null)
            {
                items.Add(item);
                if (!await enumerator.MoveNextAsync())
                {
                    break;
                }

                item = enumerator.Current;
            }
            return items;
        }

    }
}