using System.Net.Http.Headers;
using DICOMcloud.Dicom;

namespace DICOMcloud.Wado.Models
{
    public interface IWadoRequestHeader
   {
      HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> AcceptHeader        { get; set; }
      HttpHeaderValueCollection<StringWithQualityHeaderValue>    AcceptCharsetHeader { get; set; }
      
      //ObjectLevel QueryLevel { get; set; } 
   }
}
