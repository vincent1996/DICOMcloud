using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Core.Storage
{
    public class LocationDownloadedEventArgs : LocationEventArgs
    {
        public LocationDownloadedEventArgs ( IStorageLocation location ) 
        : base ( location )
        {
        }

    }
}
