using System.Collections.Generic;
using fo = Dicom;
using DICOMcloud.Dicom.DataAccess;

namespace DICOMcloud.Dicom.Data.Services.Query
{
    public interface IDicomQueryService
    {
        ICollection<fo.DicomDataset> Find 
        ( 
            fo.DicomDataset request, 
            QueryOptions options 
        ) ;
    }
}