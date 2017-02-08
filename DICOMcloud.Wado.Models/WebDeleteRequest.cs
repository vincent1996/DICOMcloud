using DICOMcloud.Dicom;
using fo = Dicom;

namespace DICOMcloud.Wado.Models
{
    public interface IWebDeleteRequest
    {
        ObjectLevel     DeleteLevel { get; set; } 
        fo.DicomDataset Dataset     { get; set;  }
    }

    public class WebDeleteRequest : IWebDeleteRequest
    {
        public fo.DicomDataset Dataset
        {
            get ;
            set ;
        }

        public ObjectLevel DeleteLevel
        {
            get ;
            set ;
        }
    }
}
