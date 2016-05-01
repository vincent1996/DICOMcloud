using System;
using System.IO;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Pacs.Commands;
using fo = Dicom;

namespace DICOMcloud.Pacs
{
    public class ObjectStoreService : IObjectStoreService
    {
        public IDicomInstnaceStorageDataAccess DataAccess   { get; set; }
        public IStoreCommand                    StoreCommand { get; set; }
        //public ObjectStoreDataService ( ) {}
        
        public ObjectStoreService 
        ( 
            IDicomInstnaceStorageDataAccess dataAccess,
            IStoreCommand                   storeCommand
        )
        {
            DataAccess   = dataAccess ;
            StoreCommand = storeCommand ;
        }
        
        public StoreResult StoreDicom
        ( 
            Stream dicomStream
        )
        {
            StoreResult storeResult  = new StoreResult ( ) ;
            fo.DicomDataset dicomObject  = null ;


            try
            {
                //currently not used
                StoreCommandResult result = new StoreCommandResult ( ) ;


                dicomObject = GetDicom(dicomStream);
                
                result = StoreCommand.Execute ( dicomObject );

                storeResult.DataSet = dicomObject ;
                storeResult.Status  = CommandStatus.Success ;
            }
            catch ( Exception ex )
            {
                storeResult.Status = CommandStatus.Failed ;

                //TODO: must catch specific exception types and set status, message and "code" accoringely
                storeResult.DataSet = dicomObject ;
                storeResult.Status  = CommandStatus.Failed ;
                storeResult.Error   = ex ;
                storeResult.Message = ex.Message ;
            }
            
            return storeResult ;    
        }

        protected virtual fo.DicomDataset GetDicom ( Stream dicomStream )
        {
            fo.DicomFile dicom ;


            dicom = fo.DicomFile.Open ( dicomStream ) ;

            return dicom.Dataset ;
        }
    }
}
