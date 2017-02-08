using fo = Dicom;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Dicom.DataAccess.DB.Schema;
using DICOMcloud.Dicom.DataAccess.Matching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.Data.Services.Query
{

    //TODO: base class for query services
    public abstract class DicomQueryServiceBase : IDicomQueryService
    {
        public IDicomStorageQueryDataAccess QueryDataAccess { get; protected set; }
        public DbSchemaProvider             SchemaProvider  { get; protected set; }
        
        public DicomQueryServiceBase ( IDicomStorageQueryDataAccess queryDataAccess )
        : this ( queryDataAccess, new StorageDbSchemaProvider ( ) )
        {
        }

        public DicomQueryServiceBase ( IDicomStorageQueryDataAccess queryDataAccess, DbSchemaProvider schemaProvider )
        {
            QueryDataAccess = queryDataAccess ;
            SchemaProvider  = schemaProvider ;
        }

        public ICollection<fo.DicomDataset> Find 
        ( 
            fo.DicomDataset request, 
            IQueryOptions options,
            string queryLevel
        )
        {

            IEnumerable<IMatchingCondition> conditions = null;
            ObjectDatasetResponseBuilder responseBuilder = CreateResponseBuilder ( queryLevel ) ;


            conditions = BuildConditions ( request );

            DoFind ( request, options, queryLevel, conditions, responseBuilder );

            return responseBuilder.GetResponse ( );
        }

        protected virtual ObjectDatasetResponseBuilder CreateResponseBuilder 
        ( 
            string queryLevel
        )
        {
            return new ObjectDatasetResponseBuilder ( SchemaProvider, queryLevel );
        }

        protected virtual IEnumerable<IMatchingCondition> BuildConditions
        (
            fo.DicomDataset request
        )
        {
            ConditionFactory condFactory = new ConditionFactory ( ) ;
            IEnumerable<IMatchingCondition> conditions ;
            
            condFactory.BeginProcessingElements ( ) ;

            foreach ( var element in request )
            {
                condFactory.ProcessElement ( element ) ;
            }

            conditions = condFactory.EndProcessingElements ( ) ;

            return conditions ;
        }

        protected abstract void DoFind
        (
            fo.DicomDataset request,
            IQueryOptions options, 
            string queryLevel,
            IEnumerable<IMatchingCondition> conditions, 
            ObjectDatasetResponseBuilder responseBuilder
        ) ;
    }
}
