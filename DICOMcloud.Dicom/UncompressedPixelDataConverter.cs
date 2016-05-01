using fo = Dicom;
using DICOMcloud.Core.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace DICOMcloud.Dicom
{
    
    public interface IUncompressedPixelDataConverter : IDicomConverter<Stream>
    {}

    public class UncompressedPixelDataConverter : IUncompressedPixelDataConverter
    {
        public UncompressedPixelDataConverter()
        {
        }

        //TODO: is this used? update with fo-dicom
        public Stream Convert ( fo.DicomDataset ds )
        {
            //fo.DicomDataset command = new fo.DicomDataset () ;

            //command[fo.DicomTag.TransferSyntaxUid] = ds[fo.DicomTag.TransferSyntaxUid] ;
            //DicomMessage message = new DicomMessage (command, ds) ;

            
            //DicomPixelData pd = DicomPixelData.CreateFrom ( message ) ;
            //string tempFile = System.IO.Path.GetTempFileName ( ) ; 
            //System.IO.File.WriteAllBytes ( tempFile, pd.GetFrame (0));
        
            //return new DICOMcloud.Storage.TempStream (new TempFile(tempFile));
            return null ;
        }
    }
}
