using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.DataAccess.Commands
{
    public class ScalarCommand<T> : IDataAdapterCommand<T>
    {
        public ScalarCommand ( IDbCommand command )
        {
            Command = command ;
        }

        public virtual bool Execute ( ) 
        {
            Command.Connection.Open ( );

            try
            {
                var value = Command.ExecuteScalar ( ) ;

                if ( null != value && DBNull.Value != value )
                {
                    if ( value is T )
                    {
                        Result = (T) value ;

                        return true ;
                    }
                    else
                    {
                        throw new InvalidOperationException ( "Generic type T is not supported for this Command value." ) ;
                    }
                }
                
                Result = default (T) ;
                
                return false ;
            }
            finally
            {
                Command.Connection.Close ( );
            }
        }

        public T Result 
        { 
            get; 
            protected set;
        }

        public IDbCommand Command 
        {
            get;
            private set ;
        }
    }
}
