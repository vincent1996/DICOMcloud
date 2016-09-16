using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Dicom.Media
{
    public class DicomMediaWriterFactory : IDicomMediaWriterFactory
    {
        protected Func<string, IDicomMediaWriter> MediaFactory { get; private set; }
        protected IMediaStorageService StorageService { get; private set ; }


        public DicomMediaWriterFactory ( IMediaStorageService storageService ) 
        {
            Init ( CreateDefualtWriters, storageService ) ;
        }

        public DicomMediaWriterFactory 
        ( 
            Func<string, IDicomMediaWriter> mediaFactory, 
            IMediaStorageService storageService 
        ) 
        {
            Init ( mediaFactory, storageService ) ;
        }

        private void Init 
        ( 
            Func<string, IDicomMediaWriter> mediaFactory, 
            IMediaStorageService storageService 
        )
        {
            MediaFactory   = mediaFactory ;
            StorageService = storageService ;
        }

        public virtual IDicomMediaWriter GetMediaWriter ( string mediaType )
        {
            try
            {
                IDicomMediaWriter writer = null ;
            
                writer = MediaFactory ( mediaType ) ;
            
                if ( null == writer )
                {
                    Trace.TraceInformation ( "Requested media writer not registered: " + mediaType ) ;
                }
                
                return writer ;
            }
            catch
            {
                return null ;
            }
        }
        
        protected virtual IDicomMediaWriter CreateDefualtWriters ( string mimeType  ) 
        {
            if ( mimeType == MimeMediaTypes.DICOM )
            {
                return new NativeMediaWriter ( StorageService ) ;
            }

            return null ;
        }
    }
}
