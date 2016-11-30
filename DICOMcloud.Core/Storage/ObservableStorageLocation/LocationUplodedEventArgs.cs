using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Core.Storage
{
    public class LocationUploadedEventArgs : LocationEventArgs
    {
        public LocationUploadedEventArgs ( IStorageLocation location ) : base ( location ) 
        {
            
        }
    }
}
