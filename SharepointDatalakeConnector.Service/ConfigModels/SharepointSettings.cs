﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharepointDatalakeConnector.Service.ConfigModels
{
    public class SharepointSettings
    {
        public string SiteUrl { get; set; }
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string CertificateThumbprint { get; set; }

    }
}
