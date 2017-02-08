using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.DataAccess.Commands
{
    public interface IDataAdapterCommand<T>
    {
        bool Execute ( ) ;

        T Result { get; }
    }
}
