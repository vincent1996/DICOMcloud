using fo = Dicom;
using DICOMcloud.Core.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Dicom.Media;
using System.Diagnostics;
using DICOMcloud.Core.Messaging;
using DICOMcloud.Dicom.Data.Services;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Pacs.Commands
{
    public class StoreCommand : DicomCommand<fo.DicomDataset,StoreCommandResult>, IStoreCommand
    {
        public StoreCommand ( ) : this ( null, null ) 
        {}

        public StoreCommand 
        ( 
            IDicomInstnaceStorageDataAccess dataStorage, 
            IDicomMediaWriterFactory mediaFactory
        )
        : base ( dataStorage )
        {
            Settings = new StorageSettings ( ) ;
            MediaFactory = mediaFactory ;
        }

        public override StoreCommandResult Execute ( fo.DicomDataset dicomObject )
        {

            //TODO: Check against supported types/association, validation, can store, return appropriate error
            
            var media = SaveDicomMedia ( dicomObject ) ;

            StoreQueryModel ( dicomObject ) ;
            
            StoreObjectMetadata ( dicomObject, media ) ;

            EventBroker.Publish ( new DicomStoreSuccessEventArgs ( media, new ObjectID ( dicomObject ) ) ) ;            
            
            return null ;
        }

        protected virtual void StoreObjectMetadata(fo.DicomDataset dicomObject, Dictionary<string, IList<IStorageLocation>> media)
        {
            string metadata = "{\"Media\":\"" + string.Join ( ";", media.Keys ) + "\"}" ; 
        
            DataAccess.StoreInstanceMetadata ( new ObjectID ( dicomObject ) , metadata ) ;
        }

        protected virtual Dictionary<string,IList<IStorageLocation>> SaveDicomMedia 
        ( 
            fo.DicomDataset dicomObject
        )
        {
            Dictionary<string,IList<IStorageLocation>> mediaLocations ;


            mediaLocations = new Dictionary<string, IList<IStorageLocation>> ( ) ;

            foreach ( string mediaType in Settings.MediaTypes )
            {
                IDicomMediaWriter writer ;


                writer = MediaFactory.GetMediaWriter ( mediaType ) ;

                if ( null != writer )
                {
                    try
                    {
                        mediaLocations.Add ( writer.MediaType, writer.CreateMedia ( dicomObject ) ) ;
                    }
                    catch ( Exception ex )
                    {
                        Trace.TraceError ( "Error creating media: " + ex.ToString ( ) ) ;

                        throw ;
                    }
                }
                else
                {
                    //TODO: log something
                    Trace.TraceWarning ( "Media writer not found for mediaType: " + mediaType ) ;
                }
            }

            return mediaLocations ;
        }



        protected virtual void StoreQueryModel
        (
            fo.DicomDataset dicomObject
        )
        {
            IDicomDataParameterFactory<StoreParameter> condFactory ;
            IEnumerable<StoreParameter>                conditions ;

            condFactory = new DicomStoreParameterFactory ( ) ;
            conditions = condFactory.ProcessDataSet ( dicomObject ) ;

            DataAccess.StoreInstance ( conditions, 0, 0 ) ;
        }
        

        public StorageSettings Settings { get; set;  }
        public IDicomMediaWriterFactory MediaFactory { get; set; }
    }

    public class StorageSettings
    {
        public StorageSettings ( ) 
        {
            MediaTypes = new List<string> ( ) ;
        
            MediaTypes.Add ( MimeMediaTypes.DICOM ) ;
            MediaTypes.Add ( MimeMediaTypes.Json ) ;
            MediaTypes.Add ( MimeMediaTypes.UncompressedData ) ;
            MediaTypes.Add ( MimeMediaTypes.xmlDicom ) ;
            MediaTypes.Add ( MimeMediaTypes.Jpeg ) ;
        }

        public IList<string> MediaTypes ;
    }
}
