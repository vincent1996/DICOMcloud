using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.DataAccess.Commands
{
    public delegate T SetValueCallback <T> ( string columenName, object value ) ;

    public class ResultSetQueryCommand<T> : IDataAdapterCommand<IEnumerable<T>>
    {
        public ResultSetQueryCommand 
        ( 
            IDbCommand command, 
            string table, 
            string[] columnsFilter,
            SetValueCallback<T> setValueCallback 
        )
        {
            if( null == columnsFilter || columnsFilter.Length == 0 ) { throw new ArgumentException ( "columnsFilter must have at least one column.") ; }

            Command          = command ;
            Table            = table ;
            Columns          = columnsFilter ;
            SetValueCallback = setValueCallback ;
        }

        public virtual bool Execute ( ) 
        {
            Command.Connection.Open ( );

            
            using ( var reader = Command.ExecuteReader ( CommandBehavior.CloseConnection | CommandBehavior.KeyInfo ) )
            {
                do
                {
                    //table name is not availabile in GetSchemaTable to compare. depend on the Column name to be unique across tables.
                    if ( null != reader.GetSchemaTable ( ).Select ( "ColumnName ='" + Columns.First() + "'"  ).FirstOrDefault ( ) )
                    {
                        List<T> results = new List<T> ( ) ;

                        while ( reader.Read ( ) )
                        {
                            T model = default(T);


                            foreach (var column in Columns )
                            {    
                                int colIndex = reader.GetOrdinal ( column ) ;
                        
                                if ( colIndex > -1 )
                                {
                                    var value = reader.GetValue ( colIndex ) ;


                                    model = this.SetValueCallback ( column, value ) ;
                                }
                            }

                            results.Add ( model ) ;
                        }

                        Result = results.AsEnumerable<T> ( ) ;

                        return true ;
                    }
                } while ( reader.NextResult ( ) ) ;
            }

            return false ;
        }

        public IEnumerable<T> Result
        { 
            get; 
            protected set;
        }

        public IDbCommand Command 
        {
            get;
            private set ;
        }

        private string   Table   { get; set; }
        private string[] Columns { get; set; }
        
        private SetValueCallback<T> SetValueCallback { get; set; }
    }
}
