using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;

namespace SharepointDatalakeConnector.Service.Interfaces
{
    public interface IDatalakeService
    {
        DataLakeFileSystemClient GetDataLakeFileSystemClient(string accountName, string accountKey, string container);
        Task UploadFileAsync(DataLakeFileSystemClient fileSystemClient, string directory, string fileName, FileStream filestream);
        Task UploadFileBulkAsync(DataLakeFileSystemClient fileSystemClient, string directory, string fileName, byte[] byteArray);
        Task<List<PathItem>> ListFilesInDirectoryAsync(DataLakeFileSystemClient fileSystemClient, string directory);
    }
}