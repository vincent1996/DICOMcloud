using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;
using Dicom.Imaging ;
using DICOMcloud.Core.Storage;


namespace DICOMcloud.Dicom.Media
{
    public abstract class DicomMediaWriterBase : IDicomMediaWriter
    {
        public IMediaStorageService MediaStorage { get; set; }

        public DicomMediaWriterBase() : this(new FileStorageService())
        { }

        public DicomMediaWriterBase ( IMediaStorageService mediaStorage )
        {
            MediaStorage = mediaStorage ;
        }

        public abstract string MediaType
        {
            get ;
        }

        public IList<IStorageLocation> CreateMedia(fo.DicomDataset data)
        {
            if (null != MediaStorage)
            {
                string                 key          = null;
                int                    framesCount  = 1;
                List<IStorageLocation> locations    = new List<IStorageLocation> ( ) ;

                if ( StoreMultiFrames )
                {
                    DicomPixelData pd ;


                    pd          = DicomPixelData.Create ( data ) ;
                    framesCount = pd.NumberOfFrames ;
                }
                
                for ( int frame = 1; frame <= framesCount; frame++ )
                {
                    var storeLocation = MediaStorage.GetLocation(new DicomMediaId ( data, frame, MediaType));

                    Upload ( data, frame, storeLocation ) ;
                
                    locations.Add ( storeLocation ) ;
                }

                return locations ;
            }

            throw new InvalidOperationException ( "No MediaStorage service found") ;
        }

        protected abstract bool StoreMultiFrames { get; }

        protected abstract void Upload(fo.DicomDataset dataset, int frame, IStorageLocation storeLocation);
    }
}
