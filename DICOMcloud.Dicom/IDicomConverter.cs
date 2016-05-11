using fo = Dicom ;

namespace DICOMcloud.Dicom
{
    public interface IDicomConverter<T>
    {
        
        T Convert ( fo.DicomDataset dicom ) ;

        fo.DicomDataset Convert ( T value ) ;
    }
}