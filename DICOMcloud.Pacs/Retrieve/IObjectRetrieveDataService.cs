using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Dicom.Data;
using DICOMcloud.Dicom.Data.Services;
using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.Media;

namespace DICOMcloud.Pacs
{
    public interface IObjectRetrieveDataService
    { 
        IStorageLocation RetrieveSopInstance ( IObjectID query, DicomMediaProperties mediaInfo ) ;
    
        IEnumerable<IStorageLocation> RetrieveSopInstances ( IObjectID query, DicomMediaProperties mediaInfo ) ;
    }
}
