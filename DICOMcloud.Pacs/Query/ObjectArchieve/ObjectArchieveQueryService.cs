using fo = Dicom;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Dicom.DataAccess.DB;
using DICOMcloud.Dicom.DataAccess.DB.Schema;
using DICOMcloud.Dicom.DataAccess.Matching;
using DICOMcloud.Dicom.Data.Services.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.Data.Services
{
    public class ObjectArchieveQueryService : DicomQueryServiceBase, IObjectArchieveQueryService
    {
        public ObjectArchieveQueryService ( IDicomStorageQueryDataAccess dataAccess ) : base ( dataAccess )
        {}
         
        public ICollection<fo.DicomDataset> FindStudies 
        ( 
            fo.DicomDataset request, 
            int? limit,
            int? offset    
        )
        {
            return Find ( request, new QueryOptions() { Limit = limit, Offset = offset, QueryLevel = StorageDbSchemaProvider.StudyTableName } ) ;
        }

        public ICollection<fo.DicomDataset> FindObjectInstances
        (
            fo.DicomDataset request,
            int? limit,
            int? offset
        )
        {
            return Find ( request, new QueryOptions() { Limit = limit, Offset = offset, QueryLevel = StorageDbSchemaProvider.ObjectInstanceTableName }) ;
        }

        public ICollection<fo.DicomDataset> FindSeries
        (
            fo.DicomDataset request,
            int? limit,
            int? offset
        )
        {
            return Find ( request, new QueryOptions() { Limit = limit, Offset = offset, QueryLevel = StorageDbSchemaProvider.SeriesTableName }) ;
        }

        protected override void DoFind
        (
           fo.DicomDataset request,
           QueryOptions options,
           IEnumerable<IMatchingCondition> conditions,
           ObjectArchieveResponseBuilder responseBuilder
        )
        {
            QueryDataAccess.Search ( conditions, responseBuilder, options ) ;
        }



        //private List<IMatchingCondition> BuildQueryCondition(fo.DicomItem element)
        //{
        //    List<IMatchingCondition> conditions = new List<IMatchingCondition> ( ) ;

        //    conditions.Add ( ConditionFactory.Process ( element )  ) ;

        //    return conditions ;
        //}
    }
}
