using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom;
using DICOMcloud.Dicom.Media;
using DICOMcloud.Pacs;
using DICOMcloud.Wado.Models;
using fo = Dicom;

namespace DICOMcloud.Wado.Core
{
    public class ImageObjectHandler : ObjectHandlerBase
    {
        private List<string> _supportedMime = new List<string>();
        IObjectRetrieveDataService RetrieveService     { get; set;  }
      
        public ImageObjectHandler(IMediaStorageService mediaStorage ) : base ( mediaStorage )
        {
            _supportedMime.Add(MimeMediaTypes.DICOM);
            _supportedMime.Add(MimeMediaTypes.Jpeg);
            _supportedMime.Add(MimeMediaTypes.UncompressedData);
            _supportedMime.Add(MimeMediaTypes.WebP);
        }

        public override bool CanProcess(string mimeType)
        {
            return _supportedMime.Contains(mimeType, StringComparer.CurrentCultureIgnoreCase);
        }
      //TODO: I should be able to replace this with the media readers now
        protected override WadoResponse DoProcess(IWadoUriRequest request, string mimeType)
        {
            var dcmLocation = MediaStorage.GetLocation ( new DicomMediaId ( request, MimeMediaTypes.DICOM, (request.ImageRequestInfo != null ) ? request.ImageRequestInfo.TransferSyntax : "") ) ;
            //var dcmLocation = RetrieveService.RetrieveSopInstances ( request, mimeType ).FirstOrDefault();


            if ( !dcmLocation.Exists ( ) )
            {
                throw new ApplicationException ( "Object Not Found - return proper wado error ") ;
            }

            //if (string.Compare(mimeType, MimeMediaTypes.DICOM, true) == 0)
            {
                return new WadoResponse(Location.GetReadStream ( ), mimeType);
            }

            fo.DicomFile file = fo.DicomFile.Open ( dcmLocation.GetReadStream ( ) ) ;
            var frameIndex = request.ImageRequestInfo.FrameNumber - 1 ?? 0  ;
                    
            frameIndex = Math.Max ( frameIndex, 0 ) ;

            if (string.Compare(mimeType, MimeMediaTypes.Jpeg, true) == 0)
            {
                WadoResponse response = new WadoResponse();
                fo.DicomDataset ds = file.Dataset ;


                if (file.FileMetaInfo.TransferSyntax != fo.DicomTransferSyntax.JPEGProcess1)
                {
                    
                    ds = file.Dataset.ChangeTransferSyntax ( fo.DicomTransferSyntax.JPEGProcess1 ) ;
                }
                
                DicomPixelData pd = DicomPixelData.Create(ds) ;

                
                byte[] buffer = pd.GetFrame ( frameIndex ).Data ;

                response.Content = new MemoryStream(buffer);
                response.MimeType = mimeType ;

                return response ;
            }

            if ( string.Compare(mimeType, MimeMediaTypes.UncompressedData) == 0)
            {
                WadoResponse                 response = null ;
                UncompressedPixelDataWrapper pd       = null ;
                byte[]                       buffer   = null ;


                response = new WadoResponse          ( ) ;
                pd       = new UncompressedPixelDataWrapper ( file.Dataset ) ;
                buffer   = pd.PixelData.GetFrame ( frameIndex ).Data ;
            
                response.Content  = new MemoryStream(buffer);
                response.MimeType = mimeType ;

                return response ;
            }

        //if ( string.Compare(mimeType, MimeMediaTypes.WebP) == 0)
        //{
        //    WadoResponse response = new WadoResponse ( ) ;
                
        //    byte[] buffer = File.ReadAllBytes(Location) ;

        //    response.Content  = new MemoryStream(buffer);
        //    response.MimeType = mimeType ;

        //    return response ;
        //}

         return null;
      }
   }
}
