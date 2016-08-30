using System;
using System.IO;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Dicom.Media;
using DICOMcloud.Pacs.Commands;
using fo = Dicom;

namespace DICOMcloud.Pacs
{
    public class ObjectStoreService : IObjectStoreService
    {
        public IDicomInstnaceStorageDataAccess DataAccess   { get; set; }
        public IDicomMediaWriterFactory MediaFactory        { get; set ; }
        
        //public ObjectStoreDataService ( ) {}
        
        public ObjectStoreService 
        ( 
            IDicomInstnaceStorageDataAccess dataAccess,
            IDicomMediaWriterFactory mediaFactory
        )
        {
            DataAccess   = dataAccess ;
            MediaFactory = mediaFactory ;
        }
        
        public StoreResult StoreDicom
        ( 
            fo.DicomDataset dataset,
            InstanceMetadata metadata
        )
        {
            StoreCommand     storeCommand = CreateStoreCommand ( ) ;
            StoreCommandData storeData    = new StoreCommandData ( ) { Dataset = dataset, Metadata = metadata } ;
            StoreResult      storeResult  = new StoreResult ( ) ;

            try
            {
                //currently not used
                StoreCommandResult result = new StoreCommandResult ( ) ;


                result = storeCommand.Execute ( storeData ) ;

                storeResult.DataSet = dataset ;
                storeResult.Status  = CommandStatus.Success ;
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Trace.Fail ( "Error storing object", ex.ToString ( ) );
                storeResult.Status = CommandStatus.Failed ;

                //TODO: must catch specific exception types and set status, message and "code" accoringely
                storeResult.DataSet = dataset ;
                storeResult.Status  = CommandStatus.Failed ;
                storeResult.Error   = ex ;
                storeResult.Message = ex.Message ;
            }
            
            return storeResult ;    
        }

        protected virtual StoreCommand CreateStoreCommand ( )
        {
            return new StoreCommand ( DataAccess, MediaFactory ) ;
        }
    }
}
