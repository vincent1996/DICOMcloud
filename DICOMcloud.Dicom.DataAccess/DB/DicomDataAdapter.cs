using fo = Dicom;
using DICOMcloud.Dicom.DataAccess.DB.Schema;
using DICOMcloud.Dicom.DataAccess.Matching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using DICOMcloud.Core.Extensions;
using DICOMcloud.Dicom.DataAccess.DB.Query;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess.DB
{
    public abstract class DicomDataAdapter
    {
        public DicomDataAdapter ( DbSchemaProvider schemaProvider )
        {
            SchemaProvider = schemaProvider ;
        }

        public DbSchemaProvider SchemaProvider
        {
            get;
            protected set;
        }


        public IDbCommand CreateSelectCommand 
        ( 
            string sourceTable, 
            IEnumerable<IMatchingCondition> conditions, 
            IQueryOptions options,
            out string[] tables 
        )
        {
            var queryBuilder  = BuildQuery ( conditions, options, sourceTable ) ;
            string queryText  = queryBuilder.GetQueryText ( sourceTable ) ;
            var SelectCommand = CreateCommand ( ) ;


            tables = queryBuilder.GetQueryResultTables ( ).ToArray ( ) ;

            SelectCommand.CommandText = queryText ;
            
            SetConnectionIfNull ( SelectCommand ) ;
        
            return SelectCommand ;
        }

        public IDbCommand CreateInsertCommand 
        ( 
            IEnumerable<IDicomDataParameter> conditions,
            InstanceMetadata data = null
        )
        {
            IDbCommand insertCommand = CreateCommand ( ) ;

            BuildInsert ( conditions, data, insertCommand ) ;

            SetConnectionIfNull ( insertCommand ) ;
            
            return insertCommand ;
        
        }
        
        public IDbCommand CreateDeleteInstanceCommand ( string sopInstanceUID )
        {
            IDbCommand command  = CreateCommand ( ) ;

            BuildDelete ( sopInstanceUID, command ) ;

            SetConnectionIfNull ( command ) ;

            return command ;
        }

        public IDbCommand CreateUpdateMetadataCommand ( IObjectID objectId, InstanceMetadata data )
        {
            IDbCommand insertCommand = CreateCommand ( ) ;
            var instance             = objectId ;
            

            insertCommand = CreateCommand ( ) ;

            insertCommand.CommandText = string.Format ( @"
UPDATE {0} SET {2}=@{2}, {3}=@{3} WHERE {1}=@{1}

IF @@ROWCOUNT = 0
   INSERT INTO {0} ({2}, {3}) VALUES (@{2}, @{3})
", 
DB.Schema.StorageDbSchemaProvider.MetadataTable.TableName, 
DB.Schema.StorageDbSchemaProvider.MetadataTable.SopInstanceColumn, 
DB.Schema.StorageDbSchemaProvider.MetadataTable.MetadataColumn,
DB.Schema.StorageDbSchemaProvider.MetadataTable.OwnerColumn ) ;

             var sopParam   = CreateParameter ( "@" + DB.Schema.StorageDbSchemaProvider.MetadataTable.SopInstanceColumn, instance.SOPInstanceUID ) ;
             var metaParam  = CreateParameter ( "@" + DB.Schema.StorageDbSchemaProvider.MetadataTable.MetadataColumn, data.ToJson ( ) ) ;
             var ownerParam = CreateParameter ( "@" + DB.Schema.StorageDbSchemaProvider.MetadataTable.OwnerColumn, data.Owner ) ;
            
            insertCommand.Parameters.Add ( sopParam ) ;
            insertCommand.Parameters.Add ( metaParam ) ;
            insertCommand.Parameters.Add ( ownerParam ) ;

            SetConnectionIfNull ( insertCommand ) ;        
            
            return insertCommand ; 
        }

        public IDbCommand CreateGetMetadataCommand ( IObjectID instance )
        {
            IDbCommand command  = CreateCommand ( ) ;
            var        sopParam = CreateParameter ( "@" + DB.Schema.StorageDbSchemaProvider.MetadataTable.SopInstanceColumn, instance.SOPInstanceUID ) ;
            
             
             command.CommandText = string.Format ( "SELECT {0}, {1} FROM {2} WHERE {3}=@{3}", 
                                                  DB.Schema.StorageDbSchemaProvider.MetadataTable.MetadataColumn,
                                                  DB.Schema.StorageDbSchemaProvider.MetadataTable.OwnerColumn,
                                                  DB.Schema.StorageDbSchemaProvider.MetadataTable.TableName,
                                                  DB.Schema.StorageDbSchemaProvider.MetadataTable.SopInstanceColumn ) ;

            command.Parameters.Add ( sopParam );

            SetConnectionIfNull ( command ) ;
            
            return command ;
        }

        public abstract IDbConnection CreateConnection ( ) ;
        
        protected abstract IDbCommand  CreateCommand ( ) ;

        protected abstract IDbDataParameter CreateParameter ( string columnName, object Value ) ;

        protected virtual ObjectArchieveQueryBuilder BuildQuery 
        ( 
            IEnumerable<IMatchingCondition> conditions, 
            IQueryOptions options,
            string queryLevel 
        )
        {
            ObjectArchieveQueryBuilder queryBuilder = CreateQueryBuilder ( ) ;
            TableKey                   sourceTable  = SchemaProvider.GetTableInfo ( queryLevel ) ;


            if ( sourceTable == null )
            { 
                throw new ArgumentException ( "querylevel not supported" ) ;
            }

            if ( null != conditions )
            {
                foreach ( var condition in conditions )
                {
                    if ( condition.VR == fo.DicomVR.PN )
                    { 
                        List<PersonNameData> pnValues = new List<PersonNameData> ( ) ;

                         
                        pnValues = condition.GetPNValues ( ) ;
                        
                        foreach ( var values in pnValues )
                        {
                            int          index = -1 ;
                            string[]     stringValues = values.ToArray ( ) ;
                            List<string> pnConditions = new List<string> ( ) ;

                            foreach ( var column in SchemaProvider.GetColumnInfo ( condition.KeyTag ) )
                            { 
                                var columnValues = new string [] { stringValues[++index]} ;
                                
                                queryBuilder.ProcessColumn ( sourceTable, condition, column, columnValues ) ;
                            }
                        }
                    }
                    else
                    { 
                        IList<string> columnValues = GetValues ( condition )  ;

                        foreach ( var column in SchemaProvider.GetColumnInfo ( condition.KeyTag ) )
                        { 
                            queryBuilder.ProcessColumn ( sourceTable, condition, column, columnValues ) ;
                        }
                    }
                }
            }
        
            return queryBuilder ;
        }

        protected virtual void BuildInsert 
        ( 
            IEnumerable<IDicomDataParameter> conditions, 
            InstanceMetadata data, 
            IDbCommand insertCommand 
        )
        {
            if ( null == conditions ) throw new ArgumentNullException ( ) ;

            var stroageBuilder = CreateStorageBuilder ( ) ;
            
            FillParameters ( conditions, data, insertCommand, stroageBuilder ) ;
            
            insertCommand.CommandText = stroageBuilder.GetInsertText ( ) ;
        }

        protected virtual void BuildDelete ( string sopInstanceUID, IDbCommand command)
        {
            string delete = SqlDeleteStatments.GetDeleteInstanceCommandText (sopInstanceUID ) ;
            
             command.CommandText = delete ;

        }
        
        protected virtual void SetConnectionIfNull ( IDbCommand command )
        {
            if (command !=null && command.Connection == null)
            {
                command.Connection = CreateConnection ( ) ;
            }
        }

        protected virtual void FillParameters
        (
            IEnumerable<IDicomDataParameter> dicomParameters,
            InstanceMetadata data, 
            IDbCommand insertCommad,
            ObjectArchieveStorageBuilder stroageBuilder
        )
        {
            foreach ( var dicomParam in dicomParameters )
            {
                if ( dicomParam.VR == fo.DicomVR.PN )
                { 
                    List<PersonNameData> pnValues ;

                         
                    pnValues = dicomParam.GetPNValues ( ) ;
                        
                    foreach ( var values in pnValues )
                    {
                        string[] stringValues = values.ToArray ( ) ;
                        int index = -1 ;
                        List<string> pnConditions = new List<string> ( ) ;

                        foreach ( var column in SchemaProvider.GetColumnInfo ( dicomParam.KeyTag ) )
                        { 
                            column.Values = new string [] { stringValues[++index]} ;
                                
                            stroageBuilder.ProcessColumn ( column, insertCommad, CreateParameter ) ;
                        }
                    }
                    
                    continue ;
                }

                
                foreach ( var column in SchemaProvider.GetColumnInfo ( dicomParam.KeyTag ) )
                { 
                    column.Values = GetValues ( dicomParam ) ;
                        
                    stroageBuilder.ProcessColumn ( column, insertCommad, CreateParameter ) ;
                }
            }
        }

        protected virtual ObjectArchieveQueryBuilder CreateQueryBuilder ( ) 
        {
            return new ObjectArchieveQueryBuilder ( ) ;
        }

        protected virtual ObjectArchieveStorageBuilder CreateStorageBuilder ( ) 
        {
            return new ObjectArchieveStorageBuilder ( ) ;
        }

        protected virtual IList<string> GetValues ( IDicomDataParameter condition )
        {
            if ( condition is RangeMatching )
            {
                RangeMatching  rangeCondition  = (RangeMatching) condition ;
                fo.DicomItem dateElement     = rangeCondition.DateElement ;
                fo.DicomItem timeElement     = rangeCondition.TimeElement ;
                
                
                return GetDateTimeValues ( (fo.DicomElement) dateElement, (fo.DicomElement) timeElement ) ;
            }
            else if ( condition.VR.Equals ( fo.DicomVR.DA ) || condition.VR.Equals ( fo.DicomVR.DT ) )
            {
                fo.DicomElement dateElement = null ;
                fo.DicomElement timeElement = null ;

                foreach ( var element in condition.Elements )
                {
                    if ( element.ValueRepresentation.Equals ( fo.DicomVR.DA ) )
                    {
                        dateElement = (fo.DicomElement) element ;
                        continue ;
                    }

                    if ( element.ValueRepresentation.Equals ( fo.DicomVR.TM ) )
                    { 
                        timeElement = (fo.DicomElement) element ;
                    }
                }

                return GetDateTimeValues ( dateElement, timeElement ) ;
            }
            else
            { 
                return condition.GetValues ( ) ;
            }
        }

        private IList<string> GetDateTimeValues ( fo.DicomElement dateElement, fo.DicomElement timeElement )
        {
            List<string> values = new List<string> ( ) ; 
            int dateValuesCount = dateElement == null ? 0 : (int)dateElement.Count;
            int timeValuesCount = timeElement == null ? 0 : (int)timeElement.Count;
            int dateTimeIndex = 0;

            for (; dateTimeIndex < dateValuesCount || dateTimeIndex < timeValuesCount; dateTimeIndex++)
            {
                string dateString = null;
                string timeString = null;

                if (dateTimeIndex < dateValuesCount)
                {
                    dateString = dateElement == null ? null : dateElement.Get<string>(0); //TODO: test - original code returns "" as default
                }

                if (dateTimeIndex < dateValuesCount)
                {
                    timeString = timeElement == null ? null : timeElement.Get<string>(0); //TODO: test - original code returns "" as default
                }

                values.AddRange(GetDateTimeValues(dateString, timeString));
            }

            return values;
        }

        protected virtual IList<string> GetDateTimeValues ( string dateString, string timeString )
        {
            string date1String = null ;
            string time1String = null ;
            string date2String = null ;
            string time2String = null ;

            if ( dateString != null )
            { 
                dateString = dateString.Trim ( ) ;

                if ( !string.IsNullOrWhiteSpace ( dateString ) )
                { 
                    string[] dateRange = dateString.Split ( '-' ) ;

                    if ( dateRange.Length > 0 )
                    { 
                        date1String = dateRange [0 ] ;
                        time1String = "" ;
                    }

                    if ( dateRange.Length == 2 )
                    { 
                        date2String = dateRange [ 1 ] ;
                        time2String = "" ;
                    }
                }
            }
        

            if ( timeString != null )
            { 
                timeString = timeString.Trim ( ) ;

                if ( !string.IsNullOrWhiteSpace ( timeString ) )
                { 
                    string[] timeRange = timeString.Split ( '-' ) ;

                    if ( timeRange.Length > 0 )
                    { 
                        date1String = date1String ?? "" ;
                        time1String = timeRange [0 ] ; 
                    }

                    if ( timeRange.Length == 2 )
                    { 
                        date2String = date2String ?? "" ;
                        time2String = timeRange [ 1 ] ;
                    }
                }
            }
        
            return GetDateTimeQueryValues ( date1String, time1String, dateString, time2String ) ;
        }

        protected virtual IList<string> GetDateTimeQueryValues
        (
            string date1String, 
            string time1String, 
            string date2String, 
            string time2String
        )
        {
            List<string> values = new List<string> ( ) ;
            
            
            SanitizeDate ( ref date1String ) ;
            SanitizeDate ( ref date2String ) ;
            SanitizeTime ( ref time1String, true ) ;
            SanitizeTime ( ref time2String, false ) ;

            if ( string.IsNullOrEmpty (date1String) && string.IsNullOrEmpty(date2String) &&
                 string.IsNullOrEmpty (time1String) && string.IsNullOrEmpty(time2String) )
            {
                return values ;
            }

            if ( string.IsNullOrEmpty(date1String) ) 
            {
                //date should be either min or same as second
                date1String = string.IsNullOrEmpty ( date2String ) ? SqlConstants.MinDate : date2String  ;
            }

            if ( string.IsNullOrEmpty (time1String) )
            {
                time1String = string.IsNullOrEmpty ( time2String ) ? SqlConstants.MinTime : time2String ;
            }

            if ( string.IsNullOrEmpty(date2String) ) 
            {
                //date should be either min or same as second
                date2String = ( SqlConstants.MinDate == date1String ) ? SqlConstants.MaxDate : date1String ;
            }

            if ( string.IsNullOrEmpty (time2String) )
            {
                time2String = ( SqlConstants.MinTime == time1String ) ? SqlConstants.MaxTime : time1String ;
            } 

            values.Add ( date1String + " " + time1String ) ;
            values.Add ( date2String + " " + time2String ) ;
            
            return values ;
        }

        
        //TODO: currently not used any more
        protected virtual string CombineDateTime(string dateString, string timeString, bool secondInRange )
        {
            if ( string.IsNullOrWhiteSpace ( timeString ) && string.IsNullOrWhiteSpace ( dateString ) )
            {
                return ( secondInRange ) ? SqlConstants.MaxDateTime : SqlConstants.MinDateTime ;
            }

            if ( string.IsNullOrEmpty ( timeString ) )
            {
                timeString = ( secondInRange ) ? SqlConstants.MaxTime : SqlConstants.MinTime ;
            }

            if ( string.IsNullOrEmpty ( dateString ) )
            {
                dateString = ( secondInRange ) ? SqlConstants.MaxDate : SqlConstants.MinDate ;
            }
            

            return dateString + " " + timeString ;
        }

        protected virtual void SanitizeTime(ref string timeString, bool startTime )
        {
            if (null == timeString) { return ;}

            if ( string.IsNullOrEmpty ( timeString ) )
            { 
                timeString = "" ;

                return ;
            }

            if ( true )//TODO: add to config
            {
                timeString = timeString.Replace (":", "");
            }

            int length = timeString.Length ;

            if (length > "hhmm".Length) 
            {  
                timeString = timeString.Insert (4, ":") ; 
            }
            else if ( length == 4 )
            { 
                if ( startTime )
                {
                    timeString   += ":00" ;
                }
                else
                { 
                    timeString += ":59" ;
                }
            }
            
            if (timeString.Length > "hh".Length) 
            {  
                timeString = timeString.Insert (2, ":") ; 
            }
            else //it must equal
            { 
                if ( startTime )
                {
                    timeString   += ":00:00" ;
                }
                else
                { 
                    timeString += ":59:59" ;
                }
            }
            
            {//TODO: no support for fractions 
                int fractionsIndex ;

                if( ( fractionsIndex= timeString.LastIndexOf (".") ) > -1 )
                {
                    timeString = timeString.Substring ( 0, fractionsIndex ) ;
                }
            } 
        }

        protected virtual void SanitizeDate(ref string dateString )
        {
            if (null == dateString) { return ;}
            
            if ( string.IsNullOrEmpty ( dateString) )
            { 
                dateString = "" ;

                return ;
            }

            //TODO: make it a configuration option
            //a lot of dataset samples do not follow dicom standard
            if (true)
            {   
                dateString = dateString.Replace ( ".", "" ).Replace ( "-", "") ;
            }

            int length = dateString.Length ;

            if (length != 8) {  throw new ArgumentException ( "Invalid date value") ; }
            
            dateString = dateString.Insert ( 6, "-") ;

            dateString = dateString.Insert ( 4, "-") ;
        }
    
        public static class SqlConstants
        {
            public static string MinDate = "1753/1/1" ;
            public static string MaxDate = "9999/12/31" ;
            public static string MinTime = "00:00:00" ;
            public static string MaxTime = "23:59:59" ;

            public static string MaxDateTime = "9999/12/31 11:59:59"   ;
            public static string MinDateTime = "1753/1/1 00:00:00" ;
        }
    }
}
