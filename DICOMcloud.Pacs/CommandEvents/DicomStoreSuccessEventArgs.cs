using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.DataAccess;

namespace DICOMcloud.Dicom.Data.Services
{
    public class DicomStoreSuccessEventArgs : EventArgs
    {
        public DicomStoreSuccessEventArgs ( InstanceMetadata instanceMetadata )
        {
            InstanceMetadata = instanceMetadata ;
        }

        public InstanceMetadata InstanceMetadata { get ; private set ; }
    }
}
