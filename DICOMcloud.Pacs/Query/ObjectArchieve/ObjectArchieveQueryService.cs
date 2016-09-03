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

        public ObjectArchieveQueryService ( IDicomStorageQueryDataAccess dataAccess, DbSchemaProvider schemaProvider ) 
        : base ( dataAccess, schemaProvider )
        {}
         
        public ICollection<fo.DicomDataset> FindStudies 
        ( 
            fo.DicomDataset request, 
            IQueryOptions options
        )
        {
            return Find ( request, options, StorageDbSchemaProvider.StudyTableName ) ;
        }

        public ICollection<fo.DicomDataset> FindObjectInstances
        (
            fo.DicomDataset request,
            IQueryOptions options
        )
        {
            return Find ( request, options, StorageDbSchemaProvider.ObjectInstanceTableName ) ;
        }

        public ICollection<fo.DicomDataset> FindSeries
        (
            fo.DicomDataset request,
            IQueryOptions options
        )
        {
            return Find ( request, options, StorageDbSchemaProvider.SeriesTableName ) ;
        }

        protected override void DoFind
        (
           fo.DicomDataset request,
           IQueryOptions options,
           string queryLevel,
           IEnumerable<IMatchingCondition> conditions,
           ObjectArchieveResponseBuilder responseBuilder
        )
        {
            QueryDataAccess.Search ( conditions, responseBuilder, options, queryLevel ) ;
        }



        //private List<IMatchingCondition> BuildQueryCondition(fo.DicomItem element)
        //{
        //    List<IMatchingCondition> conditions = new List<IMatchingCondition> ( ) ;

        //    conditions.Add ( ConditionFactory.Process ( element )  ) ;

        //    return conditions ;
        //}
    }
}
