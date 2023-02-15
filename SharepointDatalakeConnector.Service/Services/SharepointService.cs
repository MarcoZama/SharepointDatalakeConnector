using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using SharepointDatalakeConnector.Service.ConfigModels;
using SharepointDatalakeConnector.Service.Interfaces;
using SharepointDatalakeConnector.Service.Models;

namespace SharepointDatalakeConnector.Service.Services
{
    public class SharepointService : ISharepointService
    {
        private readonly IPnPContextFactory _pnPContextFactory;
        private readonly ILogger<SharepointService> _log;
        private readonly DataLakeSettings _datalakeSettings;


        public SharepointService(
            ILogger<SharepointService> log,
            IOptions<DataLakeSettings> datalakeOptions,
            IPnPContextFactory pnPContextFactory)
        {
            _log = log;
            _datalakeSettings = datalakeOptions.Value;
            _pnPContextFactory = pnPContextFactory;

        }

        public async Task<byte[]> DownloadFileFromSharepointAsync(string siteUri, string documentUrl)
        {
            try
            {
                using (var context = await _pnPContextFactory.CreateAsync(new Uri(siteUri)))
                {
                    // Get a reference to the file
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    // Download the file as stream
                    return await testDocument.GetContentBytesAsync();
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message, ex);   
                throw;
            }
                 
        }

        public async Task<List<DocumentLibraryItem>> GetFileByExtensionFromDocumentLibraryAsync(string siteUri, string extension, string fromDateTime)
        {
            // Create a PnPContext
            using (var context = await _pnPContextFactory.CreateAsync(new Uri(siteUri)))
            {
                var myList = context.Web.Lists.GetByTitle("Documents", p => p.Title,
                                                             p => p.Fields.QueryProperties(p => p.InternalName,
                                                                                           p => p.FieldTypeKind,
                                                                                           p => p.TypeAsString,
                                                                                           p => p.Title));

                var folder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;

                string viewXML = @$"<View Scope='RecursiveAll'>                              
                                <Query>
                                      <Where>
                                        <And>
                                        <Eq><FieldRef Name='File_x0020_Type'/>
                                        <Value Type='Text'>{extension}</Value>
                                        </Eq>
                                        <Leq>
                                            <FieldRef Name='Created'/>
                                            <Value IncludeTimeValue='TRUE' Type='DateTime'>{fromDateTime}</Value>
                                        </Leq>
                                        </And>
                                      </Where>
                                </Query>
                                <ViewFields>
                                    <FieldRef Name='FileRef' />
                                    <FieldRef Name='FileLeafRef' />
                                    <FieldRef Name='ID' />
                                </ViewFields>
                                <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
                                <RowLimit Paged='TRUE'>1000</RowLimit>
                                </View>";
                bool paging = true;
                string nextPage = null;
                List<DocumentLibraryItem> list = new List<DocumentLibraryItem>();
                while (paging)
                {
                    var output = await myList.LoadListDataAsStreamAsync(new RenderListDataOptions()
                    {
                        ViewXml = viewXML,
                        RenderOptions = RenderListDataOptionsFlags.ListData,
                        Paging = nextPage ?? null,
                    }).ConfigureAwait(false);

                    if (output.ContainsKey("NextHref"))
                    {
                        nextPage = output["NextHref"].ToString().Substring(1);
                    }
                    else
                    {
                        paging = false;
                    }
                }

                // Iterate over the retrieved list items
                foreach (var listItem in myList.Items.AsRequested())
                {
                    // Do something with the list item
                    //Console.WriteLine(listItem["FileRef"]);
                    list.Add(new DocumentLibraryItem() { 
                        FileLeafRef = listItem["FileLeafRef"].ToString(),
                        FileRef = listItem["FileRef"].ToString(),
                        ID = listItem["ID"].ToString(),
                    });
                }
                return list;
            }
        }



    }
}