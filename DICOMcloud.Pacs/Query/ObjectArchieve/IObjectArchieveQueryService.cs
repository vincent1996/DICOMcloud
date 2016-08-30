using System.Collections.Generic;
using DICOMcloud.Dicom.DataAccess;
using fo = Dicom;

namespace DICOMcloud.Dicom.Data.Services
{
    public interface IObjectArchieveQueryService
    {
        ICollection<fo.DicomDataset> FindStudies 
        ( 
            fo.DicomDataset request, 
            IQueryOptions options

        ) ;

        ICollection<fo.DicomDataset> FindObjectInstances
        (
            fo.DicomDataset request,
            IQueryOptions options

        ) ;

        ICollection<fo.DicomDataset> FindSeries
        (
            fo.DicomDataset request,
            IQueryOptions options
        ) ;
    }
}