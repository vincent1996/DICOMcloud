using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom ;

namespace DICOMcloud.Dicom.Data
{
    public class ObjectID : IObjectID
    {

        public ObjectID ( ) 
        {}

        public ObjectID ( fo.DicomDataset dataset )
        {
            StudyInstanceUID  = dataset.Get<string> (fo.DicomTag.StudyInstanceUID, 0, "" ) ;
            SeriesInstanceUID = dataset.Get<string> (fo.DicomTag.SeriesInstanceUID, 0, "" ) ;
            SOPInstanceUID    = dataset.Get<string> (fo.DicomTag.SOPInstanceUID, 0, "" ) ;
        }

        public string SeriesInstanceUID
        {
            get ;
            set ;
        }

        public string SOPInstanceUID
        {
            get ;
            set ;
        }

        public string StudyInstanceUID
        {
            get ;
            set ;
        }
    
        public int? Frame { get; set; }    
    }
}
