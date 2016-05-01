using System.Collections.Generic;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess
{
    public interface IDicomInstnaceStorageDataAccess
    {
        void DeleteInstance ( string instance ) ;
        void StoreInstance(IEnumerable<IDicomDataParameter> conditions, int offset, int limit ) ;
        void StoreInstanceMetadata( IObjectID instance, string metadata ) ;
        string GetInstanceMetadata( IObjectID instance ) ;
    }
}