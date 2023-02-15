using Azure.Storage.Files.DataLake;
using SharepointDatalakeConnector.Service.Models;

namespace SharepointDatalakeConnector.Service.Interfaces
{
    public interface ISharepointService
    {
        Task<List<DocumentLibraryItem>> GetFileByExtensionFromDocumentLibraryAsync(string siteUri, string extension, string fromDateTime);

        Task<byte[]> DownloadFileFromSharepointAsync(string siteUri, string documentUrl);

    }
}