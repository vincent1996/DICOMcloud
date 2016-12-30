using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Extensions ;
using DICOMcloud.Dicom.DataAccess.Matching;
using DICOMcloud.Dicom.DataAccess.DB;
using System.Data.SqlClient;
using DICOMcloud.Dicom.DataAccess.DB.Schema;
using System.Data;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess
{
    public interface IDicomInstanceArchieveDataAccess : IDicomInstnaceStorageDataAccess, IDicomStorageQueryDataAccess
    {}

    public class DicomInstanceArchieveDataAccess : IDicomInstanceArchieveDataAccess
    {
        public string ConnectionString { get; set ; }

        public DicomInstanceArchieveDataAccess() : this("") { 
        
        }
        public DicomInstanceArchieveDataAccess ( string connectionString )
        { 
            ConnectionString = connectionString ;
        }

        public virtual void Search
        (
            IEnumerable<IMatchingCondition> conditions, 
            IStorageDataReader responseBuilder,
            IQueryOptions options,
            string queryLevel
        )
        {
            string[]         tables ;
            DicomDataAdapter dbAdapter ;
            IDbCommand       cmd ;


            dbAdapter = CreateDataAdapter ( ) ;
            cmd       = dbAdapter.CreateSelectCommand ( queryLevel, conditions, options, out tables );

            cmd.Connection.Open ( );

            try
            {
                List<IDicomDataParameter> parameters = new List<IDicomDataParameter> ( );

                using ( var reader = cmd.ExecuteReader ( ) )
                {
                    int currentTableIndex = -1;


                    do
                    {
                        currentTableIndex++;

                        responseBuilder.BeginResultSet ( tables[currentTableIndex] );

                        while ( reader.Read ( ) )
                        {
                            responseBuilder.BeginRead ( );

                            for ( int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++ )
                            {
                                string columnName = reader.GetName ( columnIndex );
                                string tableName = tables[currentTableIndex];

                                object value = reader.GetValue ( columnIndex );

                                responseBuilder.ReadData ( tableName, columnName, value );
                            }

                            responseBuilder.EndRead ( );
                        }

                        responseBuilder.EndResultSet ( );

                    } while ( reader.NextResult ( ) );
                }
            }
            finally
            {
                if ( cmd.Connection.State == System.Data.ConnectionState.Open )
                {
                    cmd.Connection.Close ( );
                }
            }
        }

        public virtual void StoreInstance 
        ( 
            IObjectId objectId,  
            IEnumerable<IDicomDataParameter> parameters, 
            InstanceMetadata data = null
        )
        {
            //TODO: use transation
            //dbAdapter.CreateTransation ( ) 

            var cmd = CreateDataAdapter ( ).CreateInsertCommand ( parameters, data );

            cmd.Connection.Open ( );

            try
            {
                int rowsInserted = cmd.ExecuteNonQuery ( );

                if ( rowsInserted <= 0 )
                {
                    //return duplicate instance?!!!
                }

                if ( null != data )
                {
                    StoreInstanceMetadata ( objectId, data );
                }
            }
            finally
            {
                cmd.Connection.Close ( );
            }
        }

        public virtual void StoreInstanceMetadata ( IObjectId objectId, InstanceMetadata data )
        {
            StoreInstanceMetadata ( objectId, data, CreateDataAdapter ( ) );
        }

        public virtual InstanceMetadata GetInstanceMetadata( IObjectId instance ) 
        {
            DicomDataAdapter dbAdapter = CreateDataAdapter ( ) ;
            
            
            var cmd = dbAdapter.CreateGetMetadataCommand ( instance ) ;
        
            cmd.Connection.Open ( ) ;

            try
            {
                InstanceMetadata metadata      = null ;
                object           metadataValue = cmd.ExecuteScalar     ( ) ;
                

                if ( null != metadataValue && DBNull.Value != metadataValue )
                {   
                    string metaDataString = (string) metadataValue ;


                    metadata = metaDataString.FromJson<InstanceMetadata> ( ) ;
                }

                return metadata ;
            }
            finally
            { 
                cmd.Connection.Close ( ) ;
            }
        }

        public virtual void DeleteStudy ( IStudyId study )
        {
            DicomDataAdapter dbAdapter = CreateDataAdapter ( );
            IDbCommand cmd;
            long       studyKey = GetStudyKey ( dbAdapter, study ) ;
            
            
            cmd = dbAdapter.CreateDeleteStudyCommand ( studyKey );

            ExecuteScalar ( cmd ) ;
        }

        public virtual void DeleteSeries ( ISeriesId series )
        {
            DicomDataAdapter dbAdapter = CreateDataAdapter ( );
            IDbCommand cmd;
            long       seriesKey = GetSeriesKey ( dbAdapter, series ) ;
            
            
            cmd = dbAdapter.CreateDeleteSeriesCommand ( seriesKey );

            ExecuteScalar ( cmd ) ;
        }

        public virtual void DeleteInstance ( IObjectId instance )
        {
            DicomDataAdapter dbAdapter = CreateDataAdapter ( );
            IDbCommand cmd;
            long       instanceKey = GetInstanceKey ( dbAdapter, instance ) ;
            
            
            cmd = dbAdapter.CreateDeleteInstancCommand ( instanceKey );

            ExecuteScalar ( cmd );
        }

        protected virtual void StoreInstanceMetadata 
        ( 
            IObjectId objectId,
            InstanceMetadata data, 
            DicomDataAdapter dbAdapter 
        )
        {
            //TODO: use transaction
            //dbAdapter.CreateTransaction ( ) 

            var cmd = dbAdapter.CreateUpdateMetadataCommand ( objectId, data );

            cmd.Connection.Open ( );

            try
            {
                int rowsInserted = cmd.ExecuteNonQuery ( );

                if ( rowsInserted <= 0 )
                {
                    //TODO: return duplicate instance?!!!
                }
            }
            finally
            {
                cmd.Connection.Close ( );
            }
        }

        protected virtual DicomDataAdapter CreateDataAdapter ( )
        {
            return new DicomSqlDataAdapter ( ConnectionString ) ;
        }

        protected virtual object ExecuteScalar ( IDbCommand cmd )
        {
            cmd.Connection.Open ( );

            try
            {
                return cmd.ExecuteScalar ( ) ;
            }
            finally
            {
                cmd.Connection.Close ( );
            }
        }

        protected virtual long GetStudyKey ( DicomDataAdapter adapter, IStudyId study )
        {
            using ( var studyKeyCmd = adapter.CreateSelectStudyKeyCommand ( study ) )
            {
                var result = ExecuteScalar ( studyKeyCmd );

                return GetValue<long> ( result, -1 );
            }
        }

        protected virtual long GetSeriesKey ( DicomDataAdapter adapter, ISeriesId series )
        {
            using ( var seriesKeyCmd = adapter.CreateSelectSeriesKeyCommand ( series ) )
            {
                var result = ExecuteScalar ( seriesKeyCmd );

                return GetValue<long> ( result, -1 );
            }
        }
        
        protected virtual long GetInstanceKey ( DicomDataAdapter adapter, IObjectId instance )
        {
            using ( var sopKeyCmd = adapter.CreateSelectInstanceKeyCommand ( instance ) )
            {
                var result = ExecuteScalar ( sopKeyCmd );

                return GetValue<long> ( result, -1 );
            }
        }

        private static T GetValue<T> ( object result, T defaultValu )
        {
            if ( result != null && result != DBNull.Value )
            {
                return (T) result;
            }
            else
            {
                return defaultValu;
            }
        }
    }
}
