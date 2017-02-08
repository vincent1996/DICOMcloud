using System.Net.Http;
using System.Threading.Tasks;
using DICOMcloud.Wado.Models;

namespace DICOMcloud.Wado.Core
{
    public interface IWebObjectStoreService
    {
        Task<HttpResponseMessage> ProcessRequest ( IWebStoreRequest request, string studyInstanceUID );
        Task<HttpResponseMessage> ProcessDelete  ( IWebDeleteRequest request ) ;
    }
}