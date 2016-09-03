using fo = Dicom;
using DICOMcloud.Dicom.DataAccess.DB.Schema;
using DICOMcloud.Dicom.DataAccess.Matching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.DataAccess.DB.Query
{
    public partial class ObjectArchieveQueryBuilder    
    {
        private List<string> _returns = new List<string> ( ) ;
        private List<string> _conditions = new List<string> ( ) ;
        private List<string> _columnDefenitions = new List<string> ( ) ;
        private SqlJoinBuilder _joins = new SqlJoinBuilder ( ) ;
        private SortedDictionary<TableKey, List<string> > _processedColumns = new SortedDictionary<TableKey, List<string> > ( ) ;
        

        public ObjectArchieveQueryBuilder ( ) 
        {
            _returns    = new List<string>   ( ) ;
            _conditions = new List<string>   ( ) ;
            _joins      = new SqlJoinBuilder ( ) ;
        }

        //static ObjectArchieveQueryBuilder ( )
        //{
        //    _cachedJoins = new Dictionary<string, string> ( ) ;

        //    _cachedJoins.Add ( GetJoinKey ( StorageDbSchemaProvider.StudyTableName, StorageDbSchemaProvider.PatientTableName ), 
        //                                    SqlQueries.Joins.StudyToPatient ) ;

        //    _cachedJoins.Add ( GetJoinKey ( StorageDbSchemaProvider.SeriesTableName, StorageDbSchemaProvider.StudyTableName ), 
        //                                    SqlQueries.Joins.SeriesToStudy ) ;

        //    _cachedJoins.Add ( GetJoinKey ( StorageDbSchemaProvider.ObjectInstanceTableName, StorageDbSchemaProvider.SeriesTableName ), 
        //                                    SqlQueries.Joins.ObjectToSeries ) ;
        //}

        private static string GetJoinKey(string sourceTable, string destTable)
        {
            return string.Format ( SqlQueries.Table_Column_Formatted, sourceTable, destTable) ;
        }

        public virtual string GetQueryText ( string sourceTable )
        {
            string selectText = string.Join ( ",", _returns  ) ;
            string joinsText  = string.Join ( " ", _joins.ToString ( ) ) ;
            string whereText  = string.Join ( " AND ", _conditions ) ; 

            if ( string.IsNullOrWhiteSpace ( joinsText ))
            {
                joinsText = "" ;
            }

            if ( string.IsNullOrWhiteSpace ( whereText))
            {
                whereText = "" ;
            }
            else
            {
                whereText = " AND " + whereText ;
            }

            string tableParam = "@someTableParam " ;
            StringBuilder queryBuilder = new StringBuilder ( ) ;

            AppendDeclareTableParam ( tableParam, string.Join ( ",", _columnDefenitions), queryBuilder ) ;
            
            queryBuilder.Append ( "INSERT INTO " + tableParam  ) ;
            queryBuilder.AppendFormat ( SqlQueries.Select_Command_Formatted, selectText, sourceTable, joinsText , whereText ) ;

            foreach ( var tableToColumns in _processedColumns )
            {
                AppendSelectGroupBy ( tableParam, string.Join ( ",", tableToColumns.Value ), queryBuilder ) ;
            }

            return queryBuilder.ToString ( ) ;
        }

        public virtual IEnumerable<string> GetQueryResultTables ( ) 
        {
            foreach ( var table in _processedColumns )
            { 
                yield return table.Key.Name ;
            }
        }

        public virtual void ProcessColumn
        (   
            TableKey sourceTable,
            IQueryInfo queryInfo,
            ColumnInfo column, 
            IList<string> columnValues
        )
        {
            FillReturns ( column ) ;
            FillJoins ( sourceTable, column ) ;

            string whereCondition = AddMatching ( sourceTable, column, queryInfo, columnValues ) ;
        
            if ( !string.IsNullOrWhiteSpace ( whereCondition ) )
            { 
                _conditions.Add ( whereCondition ) ;
            }

            //_processedColumns.Add ( column ) ;
        }

        protected virtual string AddMatching 
        ( 
            string sourceTable, 
            ColumnInfo column, 
            IQueryInfo queryInfo, 
            IList<string> matchValues 
        )
        {
            if ( (null!= matchValues) && (matchValues.Count != 0) )
            {
                MatchBuilder matchBuilder = new MatchBuilder ( ) ;
                
                if ( column.IsDateTime && matchValues.Count >= 2 )
                {
                    matchBuilder.Column ( column ).GreaterThanOrEqual ( ).Value ( matchValues [ 0 ] ).And ( ).
                                 Column ( column ).LessThanOrEqual ( ).Value ( matchValues [ 1]  ) ;
                }
                else
                {
                    for ( int valueIndex = 0; valueIndex < matchValues.Count; valueIndex++ )
                    {
                        string stringValue = matchValues[valueIndex] ;
                    
                        if ( string.IsNullOrWhiteSpace (stringValue) )
                        { 
                            continue ;
                        }
                    
                        matchBuilder.Column ( column ) ;
                    
                        //TODO:??
                        //if ( queryInfo.)
                        if ( queryInfo.ExactMatch )
                        { 
                            matchBuilder.Equals ( ) ;
                        }
                        else
                        { 
                            matchBuilder.Like ( ) ;
                        }

                        matchBuilder.Value ( stringValue) ;

                        if ( valueIndex != matchValues.Count -1 )
                        { 
                            matchBuilder.Or ( ) ;
                        }
                    }
                }

                return matchBuilder.Match.ToString ( )  ;
            }

            return "" ;
        }

        protected virtual void FillReturns(ColumnInfo column )
        {
            if ( !_processedColumns.ContainsKey ( column.Table ) )
            {
                _processedColumns.Add ( column.Table, new List<string> ( ) ) ;
                
                FillReturns ( column.Table.KeyColumn ) ;

                if ( null!= column.Table.ForeignColumn )
                { 
                    FillReturns ( column.Table.ForeignColumn ) ;
                }
            }

            
            //always return any matching
            _returns.Add ( string.Format (SqlQueries.Table_Column_Formatted, column.Table.Name, column.Name )) ;
            
            _processedColumns[column.Table].Add ( column.Name ) ;
            
            _columnDefenitions.Add ( column.Defenition ) ;
        }

        protected virtual void FillJoins ( TableKey sourceTable, ColumnInfo column )
        {
            if ( !column.Table.Name.Equals (sourceTable, StringComparison.InvariantCultureIgnoreCase ) )
            {
                _joins.AddJoins ( sourceTable, column.Table ) ;
                //string joinKey = GetJoinKey ( sourceTable, column.Table.Name ) ;

                //if ( !_joins.ContainsKey ( joinKey ))
                //{
                //    _joins.Add (joinKey, _cachedJoins[joinKey] ) ;
                //}
            }
        }

        protected List<string>   Returns { get { return _returns ; } }
        protected List<string>   Conditions { get { return _conditions ; } }
        protected List<string>   ColumnDefenitions { get { return _columnDefenitions ; } }
        protected SqlJoinBuilder Joins { get { return _joins ; } }
    }
}
