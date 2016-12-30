using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Dicom;
using DICOMcloud.Dicom.Data;
using DICOMcloud.Dicom.DataAccess;
using fo = Dicom;


namespace DICOMcloud.Pacs.Commands
{
    public class DeleteCommandData
    {
        public ObjectId    ObjectInstance { get; set; }
        public ObjectLevel DeleteLevel    { get; set; }
    }
}
