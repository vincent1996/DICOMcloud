using System.Collections.Generic;
using DICOMcloud.Dicom;
using DICOMcloud.Dicom.Data;


namespace DICOMcloud.Pacs.Commands
{
    public class DeleteCommandData
    {
        public IEnumerable<IObjectId> Instances   { get; set; }
        public ObjectLevel            DeleteLevel { get; set; }
    }
}
