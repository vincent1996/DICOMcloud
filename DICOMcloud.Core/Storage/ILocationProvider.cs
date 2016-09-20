using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Core.Storage
{
    public interface ILocationProvider
    {
        IStorageLocation GetLocation     ( IMediaId key ) ;
    }
}
