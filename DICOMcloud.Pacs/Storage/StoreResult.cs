using System;
using fo = Dicom;
using DICOMcloud.Pacs.Commands;

namespace DICOMcloud.Pacs
{
    public class StoreResult
    {
        public Exception Error { get; set; }
        public string Message { get; set; }
        public CommandStatus Status { get; set; }
    
        public fo.DicomDataset DataSet { get; set; }    
    }
}