using System.Collections.Generic;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess
{
    public interface IDicomInstnaceStorageDataAccess
    {
        void StoreInstance        ( IObjectId objectId, IEnumerable<IDicomDataParameter> parameters, InstanceMetadata data ) ;
        void StoreInstanceMetadata( IObjectId objectId, InstanceMetadata data ) ;
        
        bool DeleteInstance ( IObjectId instance );
        bool DeleteStudy    ( IStudyId  study    );
        bool DeleteSeries   ( ISeriesId series   );
        
        IEnumerable<InstanceMetadata> GetStudyMetadata    ( IStudyId study );
        IEnumerable<InstanceMetadata> GetSeriesMetadata   ( ISeriesId series );
        InstanceMetadata              GetInstanceMetadata ( IObjectId instance ) ;

    }
}