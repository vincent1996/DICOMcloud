using fo = Dicom;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.Data.Services
{
    public partial class ObjectDatasetResponseBuilder
    { 
        class KeyToDataSetCollection : ConcurrentDictionary<string,fo.DicomDataset>{}

        class ResultSetCollection : ConcurrentDictionary<string, KeyToDataSetCollection> { }
    }
}
