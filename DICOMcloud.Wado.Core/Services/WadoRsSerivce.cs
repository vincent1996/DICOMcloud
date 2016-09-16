using DICOMcloud.Wado.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using DICOMcloud.Pacs;
using DICOMcloud.Dicom.Media;
using System.Net.Http.Headers;
using DICOMcloud.Core.Storage;
using DICOMcloud.Dicom.Data;
using fo = Dicom ;
using System.IO;

namespace DICOMcloud.Wado.Core
{
    public class WadoRsService : IWadoRsService
    {
        IObjectRetrieveDataService RetrieveService     { get; set;  }
        //IWadoRsMimeResponseCreator MimeResponseHandler { get; set;  }
        IWadoResponseService       ResponseService     { get; set;  }

        public WadoRsService ( IObjectRetrieveDataService retrieveService )
        {
            RetrieveService = retrieveService ;
        
            //MimeResponseHandler = new WadoRsMimeResponseCreator ( ) ;
            ResponseService = new WadoResponseService ( ) ; 
        }

        //DICOM Instances are returned in either DICOM or Bulk data format
        //DICOM format is part10 native, Buld data is based on the accept:
        //octect-stream, jpeg, jp2....
        public virtual HttpResponseMessage RetrieveStudy ( IWadoRsStudiesRequest request )
        {
            return RetrieveInstance ( request, new WadoRSInstanceRequest ( request ) ) ;
        }

        public virtual HttpResponseMessage RetrieveSeries ( IWadoRsSeriesRequest request )
        {
            return RetrieveInstance ( request, new WadoRSInstanceRequest ( request ) ) ;
        }

        public virtual HttpResponseMessage RetrieveInstance ( IWadoRSInstanceRequest request )
        {
            return RetrieveInstance ( request, request ) ;
        }

        public virtual HttpResponseMessage RetrieveInstance ( IWadoRequestHeader header, IObjectID request )
        {
            //IWadoRsMimeResponseCreator mimeResponseHandler = new WadoRsMimeResponseCreator ( ) ;
            IWadoResponseService responseService = new WadoResponseService ( ) ; 
            HttpResponseMessage response         = new HttpResponseMessage ( ) ;
            string    mimeType                   = null ;
            var wadoResponses                    = CreateRetrieveInstanceResponse ( header, request, out mimeType ) ;
            
            //System.Net.Http.Headers.MediaTypeWithQualityHeaderValue mediaType ;
            //if ( ((MimeMediaType)MimeMediaTypes.MultipartRelated).IsIn ( request.AcceptHeader, out mediaType ))
            
            MultipartContent multiContent = new MultipartContent ( "related", "DICOM DATA BOUNDARY") ;
            multiContent.Headers.ContentType.Parameters.Add ( new System.Net.Http.Headers.NameValueHeaderValue ( "type", "\"" + mimeType + "\"" ) );

            response.Content = multiContent ;
            
        
            foreach ( var wadoResponse in wadoResponses )
            { 
                StreamContent sContent = new StreamContent ( wadoResponse.Content ) ;
                sContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue (wadoResponse.MimeType ) ;

                multiContent.Add ( sContent ) ;
            }

            if ( wadoResponses.Count ( ) == 0 )
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
            }


            return response ;
        }

        public HttpResponseMessage RetrieveFrames ( IWadoRSFramesRequest request )
        {
            List<IWadoRsResponse> wadoResponses   = new List<IWadoRsResponse> ( ) ;
            string mimeType = null ;


            foreach ( int frame in request.Frames )
            {
                request.Frame = frame ;

                wadoResponses.AddRange ( CreateRetrieveInstanceResponse ( request, request, 
                                                                          out mimeType ) ) ;
            }

            return ResponseService.CreateWadoRsResponse ( request, wadoResponses, mimeType ) ;
        }

        public HttpResponseMessage RetrieveBulkData ( IWadoRsStudiesRequest request )
        {
            string mimeType = null ;

            var wadoResponses = CreateRetrieveInstanceResponse ( request, new WadoRSInstanceRequest ( request ), 
                                                                 out mimeType ) ;

            return ResponseService.CreateWadoRsResponse ( request, wadoResponses, mimeType ) ;
        }


        //Metadata can be XML (Required) or Json (optional) only. DICOM Instances are returned with no bulk data
        //Bulk data URL can be returned (which we should) 
        public virtual HttpResponseMessage RetrieveStudyMetadata(IWadoRsStudiesRequest request)
        {
            return RetrieveInstanceMetadata ( new WadoRSInstanceRequest ( request ) );
        }

        public virtual HttpResponseMessage RetrieveSeriesMetadata(IWadoRsSeriesRequest request)
        {
            return RetrieveInstanceMetadata ( new WadoRSInstanceRequest ( request ) );
        }

        public virtual HttpResponseMessage RetrieveInstanceMetadata(IWadoRSInstanceRequest request)
        {
            IWadoResponseService responseService = new WadoResponseService ( ) ; 
            string mimeType = null ;

            var wadoResponses = CreateRetrieveInstanceResponse(request, request, out mimeType);


            return responseService.CreateWadoRsResponse ( request, wadoResponses, mimeType ) ;
        }

        protected virtual IEnumerable<IWadoRsResponse> CreateRetrieveInstanceResponse 
        ( 
            IWadoRequestHeader header, 
            IObjectID objectID,
            out string mimeType 
        )
        {
            List<IWadoRsResponse> responses = new List<IWadoRsResponse> ( ) ;
            MediaTypeWithQualityHeaderValue mediaType = null ;
            

            mimeType = null ;

            MimeMediaType jsMime = MimeMediaTypes.MultipartRelated ;
            if ( jsMime.IsIn ( header.AcceptHeader, out mediaType ))
            {
                mimeType = ProcessMultipartRequest ( objectID, responses, mediaType ) ;
            }
            //TODO: all requests for retrieve is multipart except for "metadata in json format"
            //e.g: 
            //Instances:
            //Accept - A comma-separated list of representation schemes, in preference order, which will be accepted by the service in theresponse to this request. The types allowed for this request header are as follows:
            //multipart/related; type=application/dicom; [transfer-syntax={TransferSyntaxUID}]
            //Specifies that the response can be DICOM Instances encoded in PS3.10 format. If transfer-syntax is not specified the servercan freely choose which Transfer Syntax to use for each Instance.
            //multipart/related; type=application/octet-stream
            //Specifies that the response can be Little Endian uncompressed bulk data.
            //multipart/related; type={MediaType}
            //Specifies that the response can be pixel data encoded using a {MediaType} listed in Table 6.5-1 (including parameters).
            
            //Bulk Data:
            //multipart/related; type=application/octet-stream
            //multipart/related; type={MediaType}
            
            //metadata:
            //multipart/related; type=application/dicom+xml
            //application/json

            //else
            //if ( ((MimeMediaType)MimeMediaTypes.Json).IsIn (header.AcceptHeader, out mediaType))
            //{
            //    mimeType = ProcessJsonRequest ( objectID, responses ) ;
            //}
            //else
            //if ( ((MimeMediaType)MimeMediaTypes.Jpeg).IsIn (header.AcceptHeader, out mediaType))
            //{ 
            //    //.... and so on
            //}

            //return json metadata by default
            if ( null == mimeType ) //json is default, only need to check other formats... XML//|| 
            { 
                mediaType = header.AcceptHeader.Where ( n=>n.MediaType == MimeMediaTypes.Json ).FirstOrDefault ( ) ;
                mimeType = ProcessJsonRequest ( objectID, responses, mediaType ) ;
            }
            
            return responses ;
        }

        private MimeMediaType ProcessJsonRequest 
        ( 
            IObjectID objectID, 
            List<IWadoRsResponse> responses, 
            MediaTypeWithQualityHeaderValue mediaType
        )
        {
            //IDicomConverter<string> jsonConverter = CreateDicomConverter<string> (MimeMediaTypes.Json) ;
            IWadoRsResponse response = new WadoResponse ( ) ;
            StringBuilder fullJsonResponse = new StringBuilder ("[") ;
            StringBuilder jsonArray = new StringBuilder ( ) ;
            bool exists = false ;
            

            IEnumerable<NameValueHeaderValue> transferSyntaxHeader = null ;
            List<string> transferSyntaxes = new List<string> ( ) ;
            var defaultTransfer = "" ;
            
            if ( null != mediaType )
            {
                transferSyntaxHeader = mediaType.Parameters.Where (n=>n.Name == "transfer-syntax") ;
            }

            if ( null == transferSyntaxHeader || 0 == transferSyntaxHeader.Count ( ) )
            {
                transferSyntaxes.Add ( defaultTransfer ) ;
            }
            else
            {
                transferSyntaxes.AddRange ( transferSyntaxHeader.Select ( n=>n.Value ) ) ;
            }

            foreach ( var transfer in transferSyntaxes )
            {
                foreach ( IStorageLocation storage in GetLocations (objectID, new DicomMediaProperties ( MimeMediaTypes.Json, transfer == "*" ? defaultTransfer : transfer ) ) )
                {
                    exists = true ;

                    using (var memoryStream = new MemoryStream())
                    {
                        storage.Download ( memoryStream ) ;
                        jsonArray.Append ( System.Text.Encoding.UTF8.GetString(memoryStream.ToArray ( ) ) ) ;
                        jsonArray.Append (",") ;
                    }
                }
            }

            fullJsonResponse.Append(jsonArray.ToString().TrimEnd(','));
            fullJsonResponse.Append ("]") ;

            if ( exists ) 
            {
                response.Content =  new MemoryStream (System.Text.Encoding.UTF8.GetBytes(fullJsonResponse.ToString()));
                response.MimeType = MimeMediaTypes.Json ;
            
                responses.Add (response) ;
            }
            
            return MimeMediaTypes.Json ;
        }


        /// <Examples>
        /// Accept: multipart/related; type="image/jpx"; transfer-syntax=1.2.840.10008.1.2.4.92,
        /// Accept: multipart/related; type="image/jpx"; transfer-syntax=1.2.840.10008.1.2.4.93
        /// Accept: multipart/related; type="image/jpeg"
        /// </Examples>
        private string ProcessMultipartRequest
        (
            IObjectID objectID,
            List<IWadoRsResponse> responses, 
            MediaTypeWithQualityHeaderValue mediaType
        )
        {
            var mediaTypeHeader             = mediaType.Parameters.Where (n=>n.Name == "type").FirstOrDefault() ;
            var transferSyntaxHeader        = mediaType.Parameters.Where (n=>n.Name == "transfer-syntax") ;
            MimeMediaType requestedMimeType = (mediaTypeHeader != null) ? mediaTypeHeader.Value.Trim('"') : MimeMediaTypes.UncompressedData ;
            List<string> transferSyntaxes   = new List<string> ( ) ;
            string        defaultTransfer ;
            

            DefaultMediaTransferSyntax.Instance.TryGetValue ( requestedMimeType, out defaultTransfer ) ;

            if ( 0 == transferSyntaxHeader.Count ( ) )
            {
                transferSyntaxes.Add ( defaultTransfer ) ;
            }
            else
            {
                transferSyntaxes.AddRange ( transferSyntaxHeader.Select ( n=>n.Value ) ) ;
            }

            foreach ( var transfer in transferSyntaxes )
            {
                foreach ( IStorageLocation location in GetLocations ( objectID, new DicomMediaProperties ( requestedMimeType, (transfer == "*") ? defaultTransfer : transfer) ) )
                {
                    IWadoRsResponse response = new WadoResponse (location, requestedMimeType) { TransferSyntax = transfer } ;
                    

                    responses.Add ( response ) ;
                }

                if ( responses.Count > 0 )
                {
                    break ;//transfer found
                }
            }

            if ( responses.Count == 0 ) //TODO: generate on the fly!
            {
                
            }
            
            return requestedMimeType ;
        }

        protected virtual IEnumerable<IStorageLocation> GetLocations ( IObjectID request, DicomMediaProperties mediaInfo )
        {
            if ( null != request.Frame )
            {
                List<IStorageLocation> result = new List<IStorageLocation> ( ) ;

                
                result.Add ( RetrieveService.RetrieveSopInstance ( request, mediaInfo ) ) ;

                return result ;
            }
            else
            {
                return RetrieveService.RetrieveSopInstances ( request, mediaInfo ) ;
            }
        }
    }
}
