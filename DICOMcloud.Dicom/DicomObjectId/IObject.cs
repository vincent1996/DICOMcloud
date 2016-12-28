using System;

namespace DICOMcloud.Dicom.Data
{
    public interface IObjectId : ISeriesId
    {
        string SOPInstanceUID {  get; set; }

        int? Frame { get; set; }
    }
}
