using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.Data;
using DICOMcloud.Dicom.Media;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fo = Dicom ;
using Dicom.Imaging.Codec ;

namespace DICOMcloud.Pacs
{
    public class ObjectRetrieveDataService : IObjectRetrieveDataService
    {
        public IMediaStorageService StorageService { get; private set; }
        public IDicomMediaWriterFactory MediaWriterFactory { get; private set ; }


        public ObjectRetrieveDataService ( IMediaStorageService mediaStorage, IDicomMediaWriterFactory mediaWriterFactory )
        {
            AnyTransferSyntaxValue = "*" ;
            
            StorageService     = mediaStorage ;
            MediaWriterFactory = mediaWriterFactory ;
        }
        
        public virtual IStorageLocation RetrieveSopInstance ( IObjectID query, DicomMediaProperties mediaInfo ) 
        {
            return StorageService.GetLocation ( new DicomMediaId ( query, mediaInfo )) ;
        }
        
        public virtual IEnumerable<IStorageLocation> RetrieveSopInstances ( IObjectID query, DicomMediaProperties mediaInfo ) 
        {
            return StorageService.EnumerateLocation ( new DicomMediaId ( query, mediaInfo )) ;
        }

        public virtual IEnumerable<ObjectRetrieveResult> FindSopInstances
        ( 
            IObjectID query, 
            string mediaType, 
            IEnumerable<string> transferSyntaxes, 
            string defaultTransfer
        ) 
        {
            foreach ( var transfer in transferSyntaxes )
            {
                string instanceTransfer = (transfer == AnyTransferSyntaxValue) ? defaultTransfer : transfer ;

                var    mediaProperties = new DicomMediaProperties ( mediaType, instanceTransfer ) ;
                var    mediaID         = new DicomMediaId ( query, mediaProperties ) ;
                var    found           = false ;
                
                foreach ( IStorageLocation location in StorageService.EnumerateLocation ( mediaID ) )
                {
                    found = true ;

                    yield return new ObjectRetrieveResult ( location, transfer ) ;
                }
                
                if (found)
                {
                    break ;
                }
            }
        }

        public virtual IEnumerable<ObjectRetrieveResult> GetTransformedSopInstances 
        ( 
            IObjectID query, 
            string fromMediaType, 
            string fromTransferSyntax, 
            string toMediaType, 
            string toTransferSyntax 
        ) 
        {
            var fromMediaProp = new DicomMediaProperties ( fromMediaType, fromTransferSyntax ) ;
            var fromMediaID   = new DicomMediaId         ( query, fromMediaProp ) ;
            var frameList     = ( null != query.Frame ) ? new int[] { query.Frame.Value } : null ;
            
             
            if ( StorageService.Exists ( fromMediaID ) ) 
            {
                foreach ( IStorageLocation location in StorageService.EnumerateLocation ( fromMediaID ) )
                {
                    fo.DicomFile defaultFile = fo.DicomFile.Open ( location.GetReadStream ( ) ) ;

                    foreach ( var transformedLocation in  TransformDataset ( defaultFile.Dataset, toMediaType, toTransferSyntax, frameList ) )
                    {
                        yield return new ObjectRetrieveResult ( transformedLocation, toTransferSyntax ) ; 
                    }
                }
            }
        }

        public virtual IEnumerable<IStorageLocation> TransformDataset ( fo.DicomDataset dataset, string mediaType, string instanceTransfer, int[] frameList = null ) 
        {
            var mediaProperties  = new DicomMediaProperties ( mediaType, instanceTransfer ) ;
            var writerParams     = new DicomMediaWriterParameters ( ) { Dataset = dataset, MediaInfo = mediaProperties } ;
            var locationProvider = new MemoryStorageProvider ( ) ;
            

            if ( null == frameList )
            {
                return MediaWriterFactory.GetMediaWriter ( mediaType ).CreateMedia ( writerParams, locationProvider ) ;
            }
            else
            {
                return MediaWriterFactory.GetMediaWriter ( mediaType ).CreateMedia ( writerParams, locationProvider, frameList ) ;
            }
            
        }

        public virtual fo.DicomDataset RetrieveDicomDataset ( IObjectID objectId, DicomMediaProperties mediainfo )
        {
            IStorageLocation location    ;
            fo.DicomFile defaultFile ;


            location    = RetrieveSopInstance ( objectId, mediainfo ) ;

            if ( location == null )
            {
                return null ;
            }

            defaultFile = fo.DicomFile.Open ( location.GetReadStream ( ) ) ;

            return defaultFile.Dataset ;

        }

        public virtual bool ObjetInstanceExist ( IObjectID objectId, string mediaType, string transferSyntax )
        {
            var mediaProperties = new DicomMediaProperties ( mediaType, transferSyntax ) ;
            var mediaID         = new DicomMediaId         ( objectId, mediaProperties ) ;
            
                
            return StorageService.Exists (  mediaID ) ;
        }
        
        public virtual string AnyTransferSyntaxValue
        {
            get; set;
        }
    }
}
