using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Core.Storage
{
    public interface IStorageContainer
    { 
        string Connection
        {
            get;
        }

        IStorageLocation              GetLocation    ( string name = null, IMediaId id = null ) ;
        IEnumerable<IStorageLocation> GetLocations   (string v);
        void                          DeleteLocation ( IStorageLocation location );
        bool                          LocationExists ( string v );
    }
}
