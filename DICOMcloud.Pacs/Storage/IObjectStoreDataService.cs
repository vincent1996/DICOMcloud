using System.IO;
using DICOMcloud.Pacs.Commands;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Pacs
{
    public interface IObjectStoreService
    {
        StoreResult StoreDicom ( Stream dicomStream ) ;
    }
}