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
using fo = Dicom;
using System.IO;
using System;

namespace DICOMcloud.Wado.Core
{
    public class WadoRsService : IWadoRsService
    {
        IObjectRetrieveDataService RetrieveService     { get; set;  }
        IWadoResponseService       ResponseService     { get; set;  }
        
        public string DicomDataBoundary
        {
            get;
            set;
        }

        public WadoRsService ( IObjectRetrieveDataService retrieveService )
        {
            RetrieveService   = retrieveService ;
            ResponseService   = new WadoResponseService ( ) ; 
            DicomDataBoundary =  "DICOM DATA BOUNDARY" ;
        }

        //DICOM Instances are returned in either DICOM or Bulk data format
        //DICOM format is part10 native, Bulk data is based on the accept:
        //octet-stream, jpeg, jp2....
        public virtual HttpResponseMessage RetrieveStudy ( IWadoRsStudiesRequest request )
        {
            return RetrieveMultipartInstance ( request, new WadoRSInstanceRequest ( request ) ) ;
        }

        public virtual HttpResponseMessage RetrieveSeries ( IWadoRsSeriesRequest request )
        {
            return RetrieveMultipartInstance ( request, new WadoRSInstanceRequest ( request ) ) ;
        }

        public virtual HttpResponseMessage RetrieveInstance ( IWadoRSInstanceRequest request )
        {
            return RetrieveMultipartInstance ( request, request ) ;
        }

        public virtual HttpResponseMessage RetrieveFrames ( IWadoRSFramesRequest request )
        {
            return RetrieveMultipartInstance ( request, request ) ;
        }

        public virtual HttpResponseMessage RetrieveBulkData ( IWadoRSInstanceRequest request )
        {
            //TODO: validation accept header is not dicom...

            return RetrieveMultipartInstance ( request, request ) ;
        }
        
        public virtual HttpResponseMessage RetrieveBulkData ( IWadoRSFramesRequest request )
        {
            //TODO: validation accept header is not dicom...

            return RetrieveMultipartInstance ( request, request ) ;
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
            if ( IsMultiPartRequest ( request ) )
            {
                var subMediaHeader = GetSubMediaType ( request.AcceptHeader.FirstOrDefault ( ) ) ;

                if ( null == subMediaHeader || subMediaHeader != MimeMediaTypes.xmlDicom ) 
                {
                    return new HttpResponseMessage ( System.Net.HttpStatusCode.BadRequest ) ;
                }

                return RetrieveMultipartInstance ( request, request ) ; //should be an XML request!
            }
            else //must be json, or just return json anyway (*/*)
            {
                return ProcessJsonRequest ( request, request ) ;
            }
        }

        public virtual HttpResponseMessage RetrieveMultipartInstance ( IWadoRequestHeader header, IObjectID request )
        {
            IWadoResponseService responseService ; 
            HttpResponseMessage response ;
            MultipartContent multiContent ;
            MediaTypeWithQualityHeaderValue selectedMediaTypeHeader ;
            

            if ( !IsMultiPartRequest ( header ) )
            {
                return  new HttpResponseMessage ( System.Net.HttpStatusCode.NotAcceptable ) ; //TODO: check error code in standard
            }

            responseService = new WadoResponseService ( ) ; 
            response        = new HttpResponseMessage ( ) ;
            multiContent    = new MultipartContent ( "related", DicomDataBoundary ) ;           
            selectedMediaTypeHeader = null ;

            response.Content = multiContent ;

            foreach ( var mediaTypeHeader in header.AcceptHeader ) 
            {

                if ( request is IWadoRSFramesRequest )
                {
                    var frames = ((IWadoRSFramesRequest) request ).Frames ;
                    foreach ( int frame in frames )
                    {
                        request.Frame = frame ;

                        foreach ( var wadoResponse in ProcessMultipartRequest ( request, mediaTypeHeader ) )
                        { 
                            AddMultipartContent ( multiContent, wadoResponse );

                            selectedMediaTypeHeader = mediaTypeHeader;
                        }
                    }
                }
                else
                {
                    foreach ( var wadoResponse in ProcessMultipartRequest ( request, mediaTypeHeader ) )
                    { 
                        AddMultipartContent ( multiContent, wadoResponse );

                        selectedMediaTypeHeader = mediaTypeHeader;
                    }
                }

                if (selectedMediaTypeHeader!= null) { break ; }
            }



            if ( selectedMediaTypeHeader != null )
            {
                multiContent.Headers.ContentType.Parameters.Add ( new System.Net.Http.Headers.NameValueHeaderValue ( "type", "\"" + GetSubMediaType (selectedMediaTypeHeader) + "\"" ) ) ;
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound; //check error code
            }


            return response ;
        }

        protected virtual HttpResponseMessage ProcessJsonRequest 
        ( 
            IWadoRequestHeader header, 
            IObjectID objectID
        )
        {
            List<IWadoRsResponse> responses = new List<IWadoRsResponse> ( ) ;
            HttpResponseMessage response = new HttpResponseMessage ( ) ;
            StringBuilder fullJsonResponse = new StringBuilder ("[") ;
            StringBuilder jsonArray = new StringBuilder ( ) ;
            string selectedTransfer = "" ;
            bool exists = false ;
            var mediaTypeHeader = header.AcceptHeader.FirstOrDefault ( ) ;

            IEnumerable<NameValueHeaderValue> transferSyntaxHeader = null ;
            List<string> transferSyntaxes = new List<string> ( ) ;
            var defaultTransfer = "" ;

            
            if ( null != mediaTypeHeader )
            {
                transferSyntaxHeader = mediaTypeHeader.Parameters.Where (n=>n.Name == "transfer-syntax") ;
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
                selectedTransfer = transfer == "*" ? defaultTransfer : transfer ;

                foreach ( IStorageLocation storage in GetLocations (objectID, new DicomMediaProperties ( MimeMediaTypes.Json, selectedTransfer ) ) )
                {
                    exists = true ;
                    
                    using (var memoryStream = new MemoryStream())
                    {
                        storage.Download ( memoryStream ) ;
                        jsonArray.Append ( System.Text.Encoding.UTF8.GetString(memoryStream.ToArray ( ) ) ) ;
                        jsonArray.Append (",") ;
                    }
                }

                if ( exists ) { break ; }
            }

            fullJsonResponse.Append(jsonArray.ToString().TrimEnd(','));
            fullJsonResponse.Append ("]") ;

            if ( exists ) 
            {
                var content  = new StreamContent ( new MemoryStream (System.Text.Encoding.UTF8.GetBytes(fullJsonResponse.ToString())) ) ;
            
                content.Headers.ContentType= new System.Net.Http.Headers.MediaTypeHeaderValue (MimeMediaTypes.Json);
            
                if ( !string.IsNullOrWhiteSpace ( selectedTransfer ) )
                {
                    content.Headers.ContentType.Parameters.Add ( new NameValueHeaderValue ( "transfer-syntax", "\"" + selectedTransfer + "\""));
                }

                response.Content =  content ;
            }
            else
            {
                response = new HttpResponseMessage ( System.Net.HttpStatusCode.NotFound ) ;
            }
            
            return response ;
        }


        /// <Examples>
        /// Accept: multipart/related; type="image/jpx"; transfer-syntax=1.2.840.10008.1.2.4.92,
        /// Accept: multipart/related; type="image/jpx"; transfer-syntax=1.2.840.10008.1.2.4.93
        /// Accept: multipart/related; type="image/jpeg"
        /// </Examples>
        protected virtual IEnumerable<IWadoRsResponse> ProcessMultipartRequest
        (
            IObjectID objectID,
            MediaTypeWithQualityHeaderValue mediaTypeHeader
            
        )
        {
            string              subMediaType;
            IEnumerable<string> transferSyntaxes ;
            string              defaultTransfer = null;
            bool                instancesFound = false ;

            subMediaType = GetSubMediaType(mediaTypeHeader) ;

            DefaultMediaTransferSyntax.Instance.TryGetValue ( subMediaType, out defaultTransfer );

            transferSyntaxes = GetRequestedTransferSyntax ( mediaTypeHeader, defaultTransfer );

            foreach ( var result in RetrieveService.FindSopInstances ( objectID, subMediaType, transferSyntaxes, defaultTransfer ) )
            {
                instancesFound = true ;

                yield return new WadoResponse ( result.Location.GetReadStream ( ), subMediaType ) { TransferSyntax = result.TransferSyntax };
            }

            if ( !instancesFound )
            {
                string defaultDicomTransfer ;


                DefaultMediaTransferSyntax.Instance.TryGetValue ( MimeMediaTypes.DICOM, out defaultDicomTransfer ) ; 
                

                foreach ( var result in RetrieveService.GetTransformedSopInstances ( objectID, MimeMediaTypes.DICOM, defaultDicomTransfer, subMediaType, transferSyntaxes.FirstOrDefault ( ) ) )
                {
                    yield return new WadoResponse ( result.Location.GetReadStream ( ), subMediaType ) { TransferSyntax = result.TransferSyntax };
                }
            }
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

        protected virtual bool IsMultiPartRequest ( IWadoRequestHeader header )
        {
            return ( (MimeMediaType) MimeMediaTypes.MultipartRelated ).IsIn ( header.AcceptHeader ) ;
        }

        
        private static void AddMultipartContent ( MultipartContent multiContent, IWadoRsResponse wadoResponse )
        {
            StreamContent sContent = new StreamContent ( wadoResponse.Content );

            sContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue ( wadoResponse.MimeType );

            multiContent.Add ( sContent );
        }

        private static IEnumerable<string> GetRequestedTransferSyntax  (  MediaTypeWithQualityHeaderValue mediaTypeHeader, string defaultTransfer )
        {
            //TODO: this should be extended to include query parameters in the request?
            List<string> transferSyntaxes ;
            IEnumerable<NameValueHeaderValue> transferSyntaxHeader ;

            
            transferSyntaxes     = new List<string> ( ) ;
            transferSyntaxHeader = mediaTypeHeader.Parameters.Where ( n => n.Name == "transfer-syntax" );

            if ( 0 == transferSyntaxHeader.Count ( ) )
            {
                transferSyntaxes.Add ( defaultTransfer );
            }
            else
            {
                transferSyntaxes.AddRange ( transferSyntaxHeader.Select ( n => n.Value ) );
            }

            return transferSyntaxes ;
        }

        private static string GetSubMediaType ( MediaTypeWithQualityHeaderValue mediaTypeHeader )
        {
        
            var subMediaTypeHeader = mediaTypeHeader.Parameters.Where ( n => n.Name == "type" ).FirstOrDefault ( );

            return subMediaTypeHeader.Value.Trim ( '"' ) ;
        }
    }
}
