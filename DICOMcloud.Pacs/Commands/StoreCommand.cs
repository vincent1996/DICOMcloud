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
    public class StoreCommand : DicomCommand<StoreCommandData,StoreCommandResult>, IStoreCommand
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

        public override StoreCommandResult Execute ( StoreCommandData request )
        {

            //TODO: Check against supported types/association, validation, can store, return appropriate error
            
            request.Metadata.MediaLocations = SaveDicomMedia ( request.Dataset ) ;

            StoreQueryModel ( request ) ;
            
            EventBroker.Publish ( new DicomStoreSuccessEventArgs ( request.Metadata ) ) ;            
            
            return null ;
        }

        protected virtual DicomMediaLocations[] SaveDicomMedia 
        ( 
            fo.DicomDataset dicomObject
        )
        {
            List<DicomMediaLocations> mediaLocations = new List<DicomMediaLocations> ( ) ;


            foreach ( string mediaType in Settings.MediaTypes )
            {
                DicomMediaLocations mediaLocation ;
                IDicomMediaWriter   writer ;


                mediaLocation = new DicomMediaLocations ( ) { MediaType = mediaType } ;
                writer        = MediaFactory.GetMediaWriter ( mediaType ) ;

                if ( null != writer )
                {
                    try
                    {
                        IList<IStorageLocation> createdMedia = writer.CreateMedia ( dicomObject ) ;
                        
                        
                        mediaLocation.Locations = createdMedia.Select ( media => new MediaLocationParts { Parts = media.MediaId.GetIdParts ( ) } ).ToList ( ) ;
                    
                        mediaLocations.Add ( mediaLocation ) ;    
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

            return mediaLocations.ToArray ( ) ;
        }



        protected virtual void StoreQueryModel
        (
            StoreCommandData data
        )
        {
            IDicomDataParameterFactory<StoreParameter> condFactory ;
            IEnumerable<StoreParameter>                conditions ;

            condFactory = new DicomStoreParameterFactory ( ) ;
            conditions = condFactory.ProcessDataSet ( data.Dataset ) ;

            DataAccess.StoreInstance ( new ObjectID ( data.Dataset ), conditions, data.Metadata ) ;
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
            //MediaTypes.Add ( MimeMediaTypes.Jpeg ) ;
        }

        public IList<string> MediaTypes ;
    }
}
