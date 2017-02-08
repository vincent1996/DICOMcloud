
using DICOMcloud.Wado.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using DICOMcloud.Pacs;
using fo = Dicom;
using DICOMcloud.Dicom;

namespace DICOMcloud.Wado.Core
{
    public class DeleteRsRequestModelConverter : RsRequestModelConverter<IWebDeleteRequest>
    {
        public DeleteRsRequestModelConverter ( )
        { }


        public override bool TryParse ( HttpRequestMessage request, ModelBindingContext bindingContext, out IWebDeleteRequest result )
        {
            var studyParam    = bindingContext.ValueProvider.GetValue ("studyInstanceUID") ;
            var seriesParam   = bindingContext.ValueProvider.GetValue ("seriesInstanceUID") ;
            var instanceParam = bindingContext.ValueProvider.GetValue ("sopInstanceUID" ) ;


            result = null ;

            if ( null == studyParam  && 
                 null == seriesParam  && 
                 null == instanceParam  )
            {
                return false ;
            }
            else
            {
                result = new WebDeleteRequest ( ) 
                { 
                    Dataset     = new fo.DicomDataset ( ),
                    DeleteLevel = ObjectLevel.Unknown 
                } ;

                if ( null != studyParam ) 
                { 
                    result.Dataset.Add ( fo.DicomTag.StudyInstanceUID, studyParam.AttemptedValue ) ; 
                    
                    result.DeleteLevel = ObjectLevel.Study ;
                }

                if ( null != seriesParam  ) 
                { 
                    result.Dataset.Add ( fo.DicomTag.StudyInstanceUID, seriesParam.AttemptedValue ) ;

                    result.DeleteLevel = ObjectLevel.Series ;
                }
                
                if ( null != instanceParam ) 
                { 
                    result.Dataset.Add ( fo.DicomTag.StudyInstanceUID, instanceParam.AttemptedValue ) ; 

                    result.DeleteLevel = ObjectLevel.Instance ;
                }
            }

            return result.DeleteLevel != ObjectLevel.Unknown ;
        }
   }
}
