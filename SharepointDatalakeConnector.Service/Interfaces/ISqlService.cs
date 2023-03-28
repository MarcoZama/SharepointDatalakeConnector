using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharepointDatalakeConnector.Service.Interfaces
{
    public  interface ISqlService
    {
        void ExecuteStoreProcedure(string connString, string storedProcedureName, string FileLeafRef, string FileRefModified, string FileRef);
    }
}
