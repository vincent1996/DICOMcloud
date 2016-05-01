using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Core.Storage;

namespace DICOMcloud.Dicom.Media
{
    public class XmlMediaWriter : DicomMediaWriter
    {
        public XmlMediaWriter ( IMediaStorageService mediaStorage ) : base ( mediaStorage, new XmlDicomConverter ( ), MimeMediaTypes.xmlDicom )
        {}
    }
}
