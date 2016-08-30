using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;
using DICOMcloud.Dicom.Data;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Dicom.Media;
using DICOMcloud.Core.Extensions;
using DICOMcloud.Pacs;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Pacs.Commands
{
    public class DeleteCommand : IDicomCommand<IObjectID, DicomCommandResult>
    {
        public IMediaStorageService            StorageService { get; set; }
        public IDicomInstnaceStorageDataAccess DataAccess { get; set; }
        
        public DeleteCommand
        ( 
            IMediaStorageService storageService,    
            IDicomInstnaceStorageDataAccess dataAccess
        )
        {
            StorageService = storageService ;
            DataAccess     = dataAccess ;
        }
        
        public DicomCommandResult Execute ( IObjectID instance )
        {
            DataAccess.DeleteInstance ( instance.SOPInstanceUID );
            DeleteMediaLocations      ( instance );
            
            return new DicomCommandResult ( );//TODO: currently nothing to return    
        }

        private void DeleteMediaLocations ( IObjectID instance )
        {
            var objectMetaRaw  = DataAccess.GetInstanceMetadata ( instance );
            
            
            if ( null != objectMetaRaw )
            {
                var mediaLocations = objectMetaRaw.MediaLocations;


                foreach ( var dicomMediaLocation in mediaLocations )
                {
                    foreach ( var locationParts in dicomMediaLocation.Locations )
                    {
                        IStorageLocation location;
                        DicomMediaId mediaId = new DicomMediaId ( locationParts.Parts );


                        location = StorageService.GetLocation ( mediaId );
                        location.Delete ( );

                    }
                }
            }
        }
    }
}
