using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Dicom.Media;

namespace DICOMcloud.Pacs.Commands
{
    public class DicomCommandFactory : IDicomCommandFactory
    {
        public IMediaStorageService            StorageService     { get; set; }
        public IDicomInstnaceStorageDataAccess DataAccess         { get; set; }
        public IDicomMediaWriterFactory        MediaWriterFactory { get; set; }
        public IDicomMediaIdFactory            MediaIdFactory     { get; set; }
        

        public DicomCommandFactory 
        ( 
            IMediaStorageService            storageService,
            IDicomInstnaceStorageDataAccess dataAccess,
            IDicomMediaWriterFactory        mediaWriterFactory,
            IDicomMediaIdFactory            mediaIdFactory
        ) 
        {
            StorageService     = storageService     ;
            DataAccess         = dataAccess         ;
            MediaWriterFactory = mediaWriterFactory ;
            MediaIdFactory     = mediaIdFactory     ;
        }
        
        public virtual IStoreCommand CreateStoreCommand ( ) 
        {
            return new StoreCommand ( DataAccess, MediaWriterFactory ) ;    
        }

        public virtual IDeleteCommand CreateDeleteCommand ( ) 
        {
            return new DeleteCommand ( StorageService, DataAccess, MediaIdFactory ) ;
        }
    }
}
