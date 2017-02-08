using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;

namespace DICOMcloud.Pacs.Commands
{
    public interface IStoreCommandResult : IDicomCommandResult
    {
        fo.DicomDataset ReferencedSopInstance { get; set; }
    }

    public class StoreCommandResult : DicomCommandResult
    {
        public fo.DicomDataset ReferencedSopInstance { get; set; }
    }
}
