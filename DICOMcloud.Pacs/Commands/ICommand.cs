using fo = Dicom;

namespace DICOMcloud.Pacs.Commands
{
    public interface IDicomCommand<T, R>
    {
        R Execute ( T dataObject );
    }
}