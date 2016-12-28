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
        IStorageLocation RetrieveSopInstance ( IObjectId query, DicomMediaProperties mediaInfo ) ;
    
        IEnumerable<IStorageLocation> RetrieveSopInstances ( IObjectId query, DicomMediaProperties mediaInfo ) ;

        IEnumerable<ObjectRetrieveResult> FindSopInstances 
        ( 
            IObjectId query, 
            string mediaType, 
            IEnumerable<string> transferSyntaxes, 
            string defaultTransfer
        ) ;

        IEnumerable<ObjectRetrieveResult> GetTransformedSopInstances 
        ( 
            IObjectId query, 
            string fromMediaType, 
            string fromTransferSyntax, 
            string toMediaType, 
            string toTransferSyntax 
        ) ;

        bool ObjetInstanceExist ( IObjectId objectId, string mediaType, string transferSyntax ) ;
    }
}
