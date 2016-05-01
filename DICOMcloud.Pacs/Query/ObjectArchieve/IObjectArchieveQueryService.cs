using System.Collections.Generic;
using fo = Dicom;

namespace DICOMcloud.Dicom.Data.Services
{
    public interface IObjectArchieveQueryService
    {
        ICollection<fo.DicomDataset> FindStudies 
        ( 
            fo.DicomDataset request, 
            int? limit,
            int? offset    
        ) ;

        ICollection<fo.DicomDataset> FindObjectInstances
        (
            fo.DicomDataset request,
            int? limit,
            int? offset
        ) ;

        ICollection<fo.DicomDataset> FindSeries
        (
            fo.DicomDataset request,
            int? limit,
            int? offset
        ) ;
    }
}