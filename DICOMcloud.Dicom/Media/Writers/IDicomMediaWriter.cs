using fo = Dicom;
using DICOMcloud.Core.Storage;
using System.Collections.Generic;

namespace DICOMcloud.Dicom.Media
{
    public interface IDicomMediaWriter : IMediaWriter<DicomMediaWriterParameters>
    {
        IList<IStorageLocation> CreateMedia ( DicomMediaWriterParameters data, ILocationProvider storage, int[] frameList ) ;
    }
}