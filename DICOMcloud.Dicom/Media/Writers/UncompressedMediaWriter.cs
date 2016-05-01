using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom ;
using Dicom.Imaging ;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Dicom.Media
{
    public class UncompressedMediaWriter : DicomMediaWriterBase
    {
        public UncompressedMediaWriter ( ) : base ( ) {}
         
        public UncompressedMediaWriter ( IMediaStorageService mediaStorage ) : base ( mediaStorage ) {}

        public override string MediaType
        {
            get
            {
                return MimeMediaTypes.UncompressedData ;
            }
        }

        protected override bool StoreMultiFrames
        {
            get
            {
                return true ;
            }
        }

        protected override void Upload ( fo.DicomDataset dicomDataset, int frame, IStorageLocation storeLocation)
        {
            UncompressedPixelDataWrapper uncompressedData ;
            byte[]                       buffer           ;
        
        
            uncompressedData = new UncompressedPixelDataWrapper ( dicomDataset ) ;
            buffer           = uncompressedData.PixelData.GetFrame ( frame - 1 ).Data ;
        
            storeLocation.Upload ( buffer ) ;
        }
    }
}
