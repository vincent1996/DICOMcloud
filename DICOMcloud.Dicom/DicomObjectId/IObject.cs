using System;

namespace DICOMcloud.Dicom.Data
{
    public interface IObjectID : ISeriesID
    {
        string SOPInstanceUID {  get; set; }

        int? Frame { get; set; }
    }
}
