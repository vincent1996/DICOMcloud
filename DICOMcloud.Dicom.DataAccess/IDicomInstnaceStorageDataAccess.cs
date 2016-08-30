using System.Collections.Generic;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess
{
    public interface IDicomInstnaceStorageDataAccess
    {
        void DeleteInstance ( string instance ) ;
        void StoreInstance(IObjectID objectId,  IEnumerable<IDicomDataParameter> parameters, InstanceMetadata data ) ;
        void StoreInstanceMetadata( IObjectID objectId, InstanceMetadata data ) ;
        InstanceMetadata GetInstanceMetadata( IObjectID instance ) ;
    }
}