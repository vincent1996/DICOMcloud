using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;

namespace DICOMcloud.Dicom
{
    public interface IXmlStreamDicomConverter : IDicomConverter<Stream>
    {}

    public class XmlStreamDicomConverter : IXmlStreamDicomConverter
    {
        public XmlStreamDicomConverter ( ) : this ( new XmlDicomConverter ( ) )
        { }

        public XmlStreamDicomConverter ( IXmlDicomConverter xmlconverter )
        { 
            XmlConverter = xmlconverter ;
        }

        public Stream Convert(fo.DicomDataset ds)
        {
            return new MemoryStream ( ASCIIEncoding.UTF8.GetBytes ( XmlConverter.Convert (ds)));
        }

        public IXmlDicomConverter XmlConverter { get; set; }
    }
}
