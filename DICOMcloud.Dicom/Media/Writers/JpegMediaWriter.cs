using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Codec ;
using DICOMcloud.Core.Storage;


namespace DICOMcloud.Dicom.Media
{
    public class JpegMediaWriter : DicomMediaWriterBase
    {
        public JpegMediaWriter ( ) : base ( ) {}
         
        public JpegMediaWriter ( IMediaStorageService mediaStorage ) : base ( mediaStorage ) {}

        public override string MediaType
        {
            get
            {
                return MimeMediaTypes.Jpeg ;
            }
        }

        protected override bool StoreMultiFrames
        {
            get
            {
                return true ;
            }
        }

        protected override void Upload ( fo.DicomDataset dicomObject, int frame, IStorageLocation storeLocation )
        {
            var frameIndex = frame - 1 ;
            
            //TODO: this is still not working with fo-dicom 
            //when images are not compatible from one Transfer to the other
            if (dicomObject.InternalTransferSyntax != fo.DicomTransferSyntax.JPEGProcess1)
            {
                dicomObject = dicomObject.ChangeTransferSyntax ( fo.DicomTransferSyntax.JPEGProcess1 ) ;
            }
            
            DicomPixelData pd = DicomPixelData.Create(dicomObject) ;

                
            byte[] buffer = pd.GetFrame ( frameIndex ).Data ;

            storeLocation.Upload ( buffer) ;
        }
    }
}
