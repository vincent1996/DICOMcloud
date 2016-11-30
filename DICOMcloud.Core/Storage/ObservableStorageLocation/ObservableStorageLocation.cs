using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Messaging;

namespace DICOMcloud.Core.Storage
{
    public abstract class ObservableStorageLocation : IStorageLocation
    {
        
        public abstract string ContentType
        {
            get ;
        }

        public abstract string ID
        {
            get ;
        }

        public abstract IMediaId MediaId
        {
            get ;
        }

        public abstract string Metadata
        {
            get ;

            set ;
        }

        public abstract string Name
        {
            get ;
        }

        public abstract long Size
        {
            get ;
        }

        public virtual void Delete ( )
        {
            DoDelete ( ) ;
        }

        public virtual Stream Download ( )
        {
            var stream = DoDownload ( ) ;
            
            EventBroker.Instance.Publish ( CreateLocationDownloadedEventArgs ( stream ) ) ;

            return stream ;
        }

        public virtual void Download ( Stream stream )
        {
            DoDownload ( stream ) ;

            EventBroker.Instance.Publish ( CreateLocationDownloadedEventArgs ( stream ) ) ;
        }

        public abstract bool Exists ( ) ;

        public virtual Stream GetReadStream ( )
        {
            var stream  = DoGetReadStream ( ) ;

            EventBroker.Instance.Publish ( CreateLocationDownloadedEventArgs ( stream ) ) ;

            return stream ;
        }

        public virtual void Upload ( string fileName )
        {
            DoUpload ( fileName ) ; 

            EventBroker.Instance.Publish ( CreateLocationUploadedEventArgs ( fileName ) ) ;
        }
        
        public virtual void Upload ( byte[] buffer )
        {
            DoUpload ( buffer ) ;

            EventBroker.Instance.Publish ( CreateLocationUploadedEventArgs ( buffer ) ) ;
        }
        
        public virtual void Upload ( Stream stream )
        {
            DoUpload ( stream ) ;

            EventBroker.Instance.Publish ( CreateLocationUploadedEventArgs ( stream ) ) ;
        }

        protected virtual void OnLocationDownloadedEventCreated ( LocationDownloadedEventArgs args ) 
        {}

        protected virtual void OnLocationUploadedEventCreated ( LocationUploadedEventArgs args ) 
        {}

        protected virtual LocationDownloadedEventArgs CreateLocationDownloadedEventArgs ( Stream stream )
        {
            var args = new LocationDownloadedEventArgs ( this ) ;

            args.ContentLength = stream.Length ;

            OnLocationDownloadedEventCreated ( args ) ;

            return args ;
        }
        
        protected virtual LocationUploadedEventArgs CreateLocationUploadedEventArgs ( string fileName )
        {              
            var args = new LocationUploadedEventArgs ( this ) ;
            
            args.ContentLength = new FileInfo ( fileName ).Length ;
            
            OnLocationUploadedEventCreated ( args ) ;

            return args ;
        }

        protected virtual LocationUploadedEventArgs CreateLocationUploadedEventArgs ( byte[] buffer )
        {   
            var args = new LocationUploadedEventArgs ( this ) ;

            args.ContentLength = buffer.LongLength ;

            OnLocationUploadedEventCreated ( args ) ;

            return args ;
        }

        protected virtual LocationUploadedEventArgs CreateLocationUploadedEventArgs ( Stream stream )
        {              
            var args = new LocationUploadedEventArgs ( this ) ;

            args.ContentLength = stream.Length ;

            OnLocationUploadedEventCreated ( args ) ;
            
            return args ;
        }

        protected abstract void DoDelete ( );
        protected abstract Stream DoDownload ( );
        protected abstract void DoDownload ( Stream stream );
        protected abstract Stream DoGetReadStream ( );
        protected abstract void DoUpload ( string fileName );
        protected abstract void DoUpload ( byte[] buffer );
        protected abstract void DoUpload ( Stream stream );
    }
}
