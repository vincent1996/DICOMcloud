using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Core.Storage
{
    public class LocationEventArgs : EventArgs, IPropertiesEventArgs
    {
        public IStorageLocation Location
        {
            get; set;
        }

        public long ContentLength
        {
            get; set;
        }


        public LocationEventArgs ( IStorageLocation location ) 
        {
            Location = location ;

            Properties = new Dictionary<string, string> ( ) ;
        }

        public Dictionary<string,string> Properties
        {
            get; set;
        }
    }
}
