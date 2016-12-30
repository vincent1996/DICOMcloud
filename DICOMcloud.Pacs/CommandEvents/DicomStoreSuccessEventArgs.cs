using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core;
using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.DataAccess;

namespace DICOMcloud.Dicom.Data.Services
{
    public class DicomStoreSuccessEventArgs : EventArgs, IPropertiesEventArgs
    {
        private Dictionary<string, string> _properties ;

        public DicomStoreSuccessEventArgs ( InstanceMetadata instanceMetadata )
        {
            _properties = new Dictionary<string, string> ( ) ;

            InstanceMetadata = instanceMetadata ;
        }

        public InstanceMetadata InstanceMetadata { get ; private set ; }

        public Dictionary<string, string> Properties
        {
            get
            {
                return _properties ;
            }
        }
    }
}
