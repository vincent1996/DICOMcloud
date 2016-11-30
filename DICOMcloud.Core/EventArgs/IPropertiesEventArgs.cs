using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Core
{
    public interface IPropertiesEventArgs
    {
        Dictionary<string,string> Properties { get; }
    }
}
