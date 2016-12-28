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
        
        public ImageObjectHandler ( IMediaStorageService mediaStorage, IDicomMediaIdFactory mediaFactory ) : base ( mediaStorage, mediaFactory )
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

        
        protected override WadoResponse DoProcess(IWadoUriRequest request, string mimeType)
        {
            var dcmLocation = MediaStorage.GetLocation ( MediaFactory.Create ( request, new DicomMediaProperties { MediaType = MimeMediaTypes.DICOM, 
                                                                                                                   TransferSyntax = (request.ImageRequestInfo != null ) ? request.ImageRequestInfo.TransferSyntax : "" } ) ) ;

            if ( !dcmLocation.Exists ( ) )
            {
                throw new ApplicationException ( "Object Not Found - return proper wado error ") ;
            }

            //if (string.Compare(mimeType, MimeMediaTypes.DICOM, true) == 0)
            {
                return new WadoResponse(Location.GetReadStream ( ), mimeType);
            }
      }
   }
}
