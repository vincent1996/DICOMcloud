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
    public class DeleteCommand : IDicomCommand<DeleteCommandData, DicomCommandResult>
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
                    DeleteStudy ( commandData.ObjectInstance ) ;
                }
                break;

                case Dicom.ObjectLevel.Series:
                {
                    DeleteSeries ( commandData.ObjectInstance ) ;
                }
                break;

                case Dicom.ObjectLevel.Instance:
                {
                    return DeleteInstance ( commandData.ObjectInstance ) ;
                }
                //break;

                default:
                {
                    throw new ApplicationException ( "Invalid delete level" ) ;//TODO:
                }
            }
            
            return new DicomCommandResult ( );//TODO: currently nothing to return    
        }

        protected  virtual DicomCommandResult DeleteStudy ( IStudyId study )
        {
            DeleteMediaLocations   ( study ) ;
            DataAccess.DeleteStudy ( study );
                        
            return new DicomCommandResult ( );//TODO: currently nothing to return    
        }

        protected  virtual DicomCommandResult DeleteSeries ( ISeriesId series )
        {
            DeleteMediaLocations    ( series ) ;
            DataAccess.DeleteSeries ( series );
                        
            return new DicomCommandResult ( );//TODO: currently nothing to return    
        }

        protected  virtual DicomCommandResult DeleteInstance ( IObjectId instance )
        {
            DeleteMediaLocations      ( instance );
            DataAccess.DeleteInstance ( instance ); //delete from DB after all dependencies are completed
            
            return new DicomCommandResult ( );//TODO: currently nothing to return    
        }

        private void DeleteMediaLocations ( IStudyId study )
        {
            //TODO: uncomment
            //var studyMeta  = DataAccess.GetStudyMetadata ( study ) ;
            
            
            //if ( null != studyMeta )
            //{
            //    foreach ( var objectMetaRaw in studyMeta )
            //    {
            //        DeleteMediaLocations ( objectMetaRaw ) ;
            //    }
            //}
        }

        private void DeleteMediaLocations ( ISeriesId series )
        {
            //TODO: uncomment
            //var seriesMeta = DataAccess.GetSeriesMetadata ( series ) ;
            
            
            //if ( null != seriesMeta )
            //{
            //    foreach ( var objectMetaRaw in seriesMeta )
            //    {
            //        DeleteMediaLocations ( objectMetaRaw ) ;
            //    }
            //}
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
