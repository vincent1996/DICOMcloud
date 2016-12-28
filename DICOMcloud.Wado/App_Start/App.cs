using System;
using System.Configuration;
using System.Web.Http;
using DICOMcloud.Core.Azure.Storage;
using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.Data.Services;
using DICOMcloud.Dicom.DataAccess;
using DICOMcloud.Dicom.DataAccess.DB.Schema;
using DICOMcloud.Dicom.Media;
using DICOMcloud.Pacs;
using DICOMcloud.Pacs.Commands;
using DICOMcloud.Wado.Core;
using Microsoft.Azure;
using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.Storage;
using Unity.WebApi;
using fo = Dicom;


namespace DICOMcloud.Wado
{
    public partial class App
    {
        private App ()
        {
            fo.Log.LogManager.SetImplementation ( Dicom.TraceLogManager.Instance ) ;
            
            RegisterComponents ( ) ;
             
            var path = System.IO.Path.Combine ( System.Web.Hosting.HostingEnvironment.MapPath ( "~/"), "bin" ) ;

            System.Diagnostics.Trace.TraceInformation ( "Path: " + path ) ;

            fo.Imaging.Codec.TranscoderManager.LoadCodecs ( path ) ;
        }

        public static void Config ( ) 
        {
            Instance = new App ( ) ;
        }

        private static App Instance {get; set; }

        public void RegisterComponents()
        {
            var container        = new UnityContainer();
            var connectionString = CloudConfigurationManager.GetSetting("app:PacsDataArchieve");
            var storageConection = CloudConfigurationManager.GetSetting("app:PacsStorageConnection");
            var dataAccess       = new DicomInstanceArchieveDataAccess(connectionString);


            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
            
            container.RegisterType<DbSchemaProvider> ( new InjectionConstructor() ) ; //default constructor

            container.RegisterType<IObjectArchieveQueryService, ObjectArchieveQueryService>();
            container.RegisterType<IObjectStoreService, ObjectStoreService>();
            container.RegisterType<IObjectRetrieveDataService, ObjectRetrieveDataService>();

            container.RegisterType<IStoreCommand, StoreCommand>();
            container.RegisterType<IWadoRsService, WadoRsService>();
            
            container.RegisterInstance<IDicomInstnaceStorageDataAccess>(dataAccess);
            container.RegisterInstance<IDicomStorageQueryDataAccess>   (dataAccess);
            
            container.RegisterType<IDicomMediaIdFactory,DicomMediaIdFactory> ();

            if ( System.IO.Path.IsPathRooted( storageConection ) )
            {
                container.RegisterType<IMediaStorageService, FileStorageService> ( new InjectionConstructor ( storageConection ) ) ;
            }
            else
            {
                var storageAccount = CloudStorageAccount.Parse( storageConection ) ;

                
                container.RegisterInstance<CloudStorageAccount>(storageAccount);
                
                container.RegisterType<IMediaStorageService, AzureStorageService> ( new InjectionConstructor ( storageAccount ) ) ;
            }

            RegisterMediaWriters(container);
        }
    }
}