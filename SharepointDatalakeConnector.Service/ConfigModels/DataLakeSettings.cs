using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharepointDatalakeConnector.Service.ConfigModels
{
    public class DataLakeSettings
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string ContainerName { get; set; }
    }
}
