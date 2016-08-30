using System.IO;
using DICOMcloud.Pacs.Commands;
using DICOMcloud.Core.Storage;
using fo = Dicom;
using DICOMcloud.Dicom.DataAccess;

namespace DICOMcloud.Pacs
{
    public interface IObjectStoreService
    {
        StoreResult StoreDicom ( fo.DicomDataset dataset, InstanceMetadata metadata ) ;
    }
}