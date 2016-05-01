
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Pacs;

namespace DICOMcloud.Dicom.Data.Services
{
    public interface IObjectQuery : IObjectID
    {
        ObjectQueryLevel QueryLevel     { get; set; }
    }
}
