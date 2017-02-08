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
            DataAdapter      = CreateDataAdapter ( ) ;
        }

        public virtual void Search
        (
            IEnumerable<IMatchingCondition> conditions, 
            IStorageDataReader responseBuilder,
            IQueryOptions options,
            string queryLevel
        )
        {
            string[]   tables ;
            IDbCommand cmd ;

            
            cmd = DataAdapter.CreateSelectCommand ( queryLevel, conditions, options, out tables );

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

            var cmd = DataAdapter.CreateInsertCommand ( parameters, data );

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
            StoreInstanceMetadata ( objectId, data, DataAdapter );
        }

        public virtual IEnumerable<InstanceMetadata> GetStudyMetadata ( IStudyId study ) 
        {
            var command = DataAdapter.CreateGetMetadataCommand ( study ) ;
        

            command.Execute ( ) ;

            return command.Result ;
            //return GetInstanceMetadata ( DataAdapter, command ) ;
        }
        
        public virtual IEnumerable<InstanceMetadata> GetSeriesMetadata ( ISeriesId series ) 
        {
            var command = DataAdapter.CreateGetMetadataCommand ( series ) ;
        
            command.Execute ( ) ;

            return command.Result ;

            //return GetInstanceMetadata ( DataAdapter, command ) ;
        }

        public virtual InstanceMetadata GetInstanceMetadata ( IObjectId instance ) 
        {
            var command = DataAdapter.CreateGetMetadataCommand ( instance ) ;
        

            command.Execute ( ) ;
            return command.Result ; //GetInstanceMetadata ( DataAdapter, command ).FirstOrDefault ( ) ;
        }

        public virtual bool DeleteStudy ( IStudyId study )
        {
            long studyKey  = GetStudyKey ( DataAdapter, study ) ;
            
            
            return DataAdapter.CreateDeleteStudyCommand ( studyKey ).Execute ( ) ;
        }

        public virtual bool DeleteSeries ( ISeriesId series )
        {
            long seriesKey = GetSeriesKey ( DataAdapter, series ) ;
            
            
            return DataAdapter.CreateDeleteSeriesCommand ( seriesKey ).Execute ( ) ;
        }

        public virtual bool DeleteInstance ( IObjectId instance )
        {
            long instanceKey = GetInstanceKey ( DataAdapter, instance ) ;
            
            
            return DataAdapter.CreateDeleteInstancCommand ( instanceKey ).Execute ( ) ;
        }

        protected virtual bool StoreInstanceMetadata 
        ( 
            IObjectId objectId,
            InstanceMetadata data, 
            DicomDataAdapter dbAdapter 
        )
        {
            return dbAdapter.CreateUpdateMetadataCommand ( objectId, data ).Execute ( ) ;
        }

        protected virtual DicomDataAdapter CreateDataAdapter ( )
        {
            return new DicomSqlDataAdapter ( ConnectionString ) ;
        }

        protected virtual long GetStudyKey ( DicomDataAdapter adapter, IStudyId study )
        {
            var cmd = adapter.CreateSelectStudyKeyCommand ( study ) ;


            if ( cmd.Execute ( ) )
            {
                return cmd.Result ;
            }
            else
            {
                throw new KeyNotFoundException ( "study is not found." ) ;
            }
        }

        protected virtual long GetSeriesKey ( DicomDataAdapter adapter, ISeriesId series )
        {
            var cmd = adapter.CreateSelectSeriesKeyCommand ( series ) ;


            if ( cmd.Execute ( ) )
            {
                return cmd.Result ;
            }
            else
            {
                throw new KeyNotFoundException ( "series is not found." ) ;
            }
        }

        protected virtual long GetInstanceKey ( DicomDataAdapter adapter, IObjectId instance )
        {
            var cmd = adapter.CreateSelectInstanceKeyCommand ( instance ) ;


            if ( cmd.Execute ( ) )
            {
                return cmd.Result ;
            }
            else
            {
                throw new KeyNotFoundException ( "Instance is not found." ) ;
            }
            
        }

        protected DicomDataAdapter DataAdapter { get; set; }
        
        private static T GetDbScalarValue<T> ( object result, T defaultValue )
        {
            if ( result != null && result != DBNull.Value )
            {
                return (T) result;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
