using System.Collections.Generic;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess
{
    public interface IDicomInstnaceStorageDataAccess
    {
        void StoreInstance        ( IObjectId objectId, IEnumerable<IDicomDataParameter> parameters, InstanceMetadata data ) ;
        void StoreInstanceMetadata( IObjectId objectId, InstanceMetadata data ) ;
        
        void DeleteInstance ( IObjectId instance );
        void DeleteStudy    ( IStudyId  study    );
        void DeleteSeries   ( ISeriesId series   );
        
        //InstanceMetadata[] GetStudyMetadata    ( IStudyId  study    );
        //InstanceMetadata[] GetSeriesMetadata   ( ISeriesId study    );
        InstanceMetadata   GetInstanceMetadata ( IObjectId instance ) ;

    }
}