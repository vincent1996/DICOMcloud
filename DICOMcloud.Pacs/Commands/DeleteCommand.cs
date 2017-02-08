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
    public class DeleteCommand : IDicomCommand<DeleteCommandData, DicomCommandResult>, IDeleteCommand
    {
        public IMediaStorageService            StorageService { get; set; }
        public IDicomInstnaceStorageDataAccess DataAccess     { get; set; }
        public IDicomMediaIdFactory            MediaFactory   { get; set; }
        
        public DeleteCommand
        ( 
            IMediaStorageService storageService,    
            IDicomInstnaceStorageDataAccess dataAccess,
            IDicomMediaIdFactory mediaFactory
        )
        {
            StorageService = storageService ;
            DataAccess     = dataAccess ;
            MediaFactory   = mediaFactory ;
        }
        
        public DicomCommandResult Execute ( DeleteCommandData commandData )
        {
            switch ( commandData.DeleteLevel )
            {
                case Dicom.ObjectLevel.Study:
                {
                    return DeleteStudy ( commandData.Instances ) ;
                }

                case Dicom.ObjectLevel.Series:
                {
                    return DeleteSeries ( commandData.Instances ) ;
                }

                case Dicom.ObjectLevel.Instance:
                {
                    return DeleteInstance ( commandData.Instances ) ;
                }

                default:
                {
                    throw new ApplicationException ( "Invalid delete level" ) ;//TODO:
                }
            }
        }

        protected  virtual DicomCommandResult DeleteStudy ( IEnumerable<IStudyId> studies )
        {
            foreach (var study in studies )
            {
                DeleteMediaLocations   ( study ) ;
                DataAccess.DeleteStudy ( study );
            }
                  
            return new DicomCommandResult ( ) { Status = CommandStatus.Success } ;//TODO: currently nothing to return    
        }

        protected  virtual DicomCommandResult DeleteSeries ( IEnumerable<ISeriesId> seriesIds )
        {
            foreach ( var series in seriesIds )
            {
                DeleteMediaLocations    ( series ) ;
                DataAccess.DeleteSeries ( series );
            }
                        
            return new DicomCommandResult ( ) { Status = CommandStatus.Success } ;//TODO: currently nothing to return    
        }

        protected  virtual DicomCommandResult DeleteInstance ( IEnumerable<IObjectId> instances )
        {
            foreach ( var instance in instances )
            {
                DeleteMediaLocations      ( instance );
                DataAccess.DeleteInstance ( instance ); //delete from DB after all dependencies are completed
            }

            return new DicomCommandResult ( ) { Status = CommandStatus.Success } ;//TODO: currently nothing to return    
        }

        private void DeleteMediaLocations ( IStudyId study )
        {
            var studyMeta = DataAccess.GetStudyMetadata ( study );


            if ( null != studyMeta )
            {
                foreach ( var objectMetaRaw in studyMeta )
                {
                    DeleteMediaLocations ( objectMetaRaw );
                }
            }
        }

        private void DeleteMediaLocations ( ISeriesId series )
        {
            var seriesMeta = DataAccess.GetSeriesMetadata ( series );


            if ( null != seriesMeta )
            {
                foreach ( var objectMetaRaw in seriesMeta )
                {
                    DeleteMediaLocations ( objectMetaRaw );
                }
            }
        }

        private void DeleteMediaLocations ( IObjectId instance )
        {
            var objectMetaRaw = DataAccess.GetInstanceMetadata ( instance );

            DeleteMediaLocations ( objectMetaRaw );
        }

        private void DeleteMediaLocations ( InstanceMetadata objectMetaRaw )
        {
            if ( null != objectMetaRaw )
            {
                var mediaLocations = objectMetaRaw.MediaLocations;


                foreach ( var dicomMediaLocation in mediaLocations )
                {
                    foreach ( var locationParts in dicomMediaLocation.Locations )
                    {
                        IStorageLocation location;
                        IMediaId mediaId;


                        mediaId = MediaFactory.Create ( locationParts.Parts );
                        location = StorageService.GetLocation ( mediaId );
                        
                        location.Delete ( );

                    }
                }
            }
        }

        private static IEnumerable<DicomMediaLocations> GetDistinctMedia ( InstanceMetadata objectMetaRaw )
        {
            //http://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property
            return objectMetaRaw.MediaLocations
                                                .GroupBy ( n => new
                                                {
                                                    n.TransferSyntax,
                                                    n.MediaType
                                                } )
                                                .Select ( m => m.First ( ) );
        }
    }
}
