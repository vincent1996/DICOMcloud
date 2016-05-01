using fo = Dicom;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Dicom.Media
{
    public interface IDicomMediaWriter : IMediaWriter<fo.DicomDataset>
    {
    }
}