using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Dicom.Media
{
    public class NativeMediaWriter : DicomMediaWriterBase
    {
        public NativeMediaWriter ( ) : base ( ) {}
         
        public NativeMediaWriter ( IMediaStorageService mediaStorage ) : base ( mediaStorage ) {}

        public override string MediaType 
        { 
            get 
            {
                return MimeMediaTypes.DICOM ;
            }
        }

        protected override bool StoreMultiFrames
        {
            get
            {
                return false ;
            }
        }

        protected override void Upload( fo.DicomDataset dicomDataset, int frame, IStorageLocation location )
        {
            fo.DicomFile df = new fo.DicomFile ( dicomDataset ) ;


            using (Stream stream = new MemoryStream())
            {
                df.Save(stream);
                stream.Position = 0;

                location.Upload(stream);
            }
        }
    }
}
