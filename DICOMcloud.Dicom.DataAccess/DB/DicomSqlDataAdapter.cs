using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using DICOMcloud.Dicom.DataAccess.Matching;
using fo = Dicom;
using DICOMcloud.Dicom.DataAccess.DB.Query;
using DICOMcloud.Dicom.Data;
using DICOMcloud.Dicom.DataAccess.DB.Schema;

namespace DICOMcloud.Dicom.DataAccess.DB
{
    public class DicomSqlDataAdapter : DicomDataAdapter
    {
        public DicomSqlDataAdapter ( string connectionString ) 
        :  this ( connectionString, new DbSchemaProvider ( ) )
        { 
        }

        public DicomSqlDataAdapter 
        (   
            string connectionString, 
            DbSchemaProvider schemaProvider 
        ) : base ( schemaProvider )
        { 
            ConnectionString = connectionString ;
        }

        public override IDbConnection CreateConnection ( )
        {
            return new SqlConnection ( ConnectionString ) ;
        }
        
        public string ConnectionString {  get; protected set ; }

        protected override IDbCommand CreateCommand ( )
        { 
            return new System.Data.SqlClient.SqlCommand ( ) ;
        }
        
        protected override IDbDataParameter CreateParameter ( string parameterName, object value )
        {
            return new SqlParameter (  parameterName, value ) ;
        }
    }
}
