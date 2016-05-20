using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using foDicom = Dicom;

namespace DICOMcloud.Dicom
{
    public interface IJsonDicomConverter : IDicomConverter<string>
    {
    }

    public class JsonDicomConverter : IJsonDicomConverter
    {
        private int _minValueIndex;

        public JsonDicomConverter ( )
        {
            IncludeEmptyElements = false;
        }

        public bool IncludeEmptyElements
        {
            get
            {
                return ( _minValueIndex == -1 );
            }
            set
            {
                _minValueIndex = ( value ? -1 : 0 );
            }
        }

        public string Convert ( foDicom.DicomDataset ds )
        {
            StringBuilder sb = new StringBuilder ( );
            StringWriter sw = new StringWriter ( sb );


            using ( JsonWriter writer = new JsonTextWriter ( sw ) )
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject ( );

                WriteChildren ( ds, writer );

                writer.WriteEndObject ( );
            }

            return sb.ToString ( );
        }

        public foDicom.DicomDataset Convert ( string jsonDcm )
        {
            throw new NotImplementedException ( "Converting JSON DICOM to Native DICOM is not supported." );
        }

        #region Write Methods
        
        protected virtual void WriteChildren ( foDicom.DicomDataset ds, JsonWriter writer )
        {
            foreach ( var element in ds )
            {
                WriteDicomItem ( ds, element, writer );
            }
        }

        protected virtual void WriteDicomItem
        (
            foDicom.DicomDataset ds,
            foDicom.DicomItem element,
            JsonWriter writer
        )
        {
            //group length element must not be written
            if ( null == element || element.Tag.Element == 0x0000 )
            {
                return;
            }

            foDicom.DicomVR dicomVr = element.ValueRepresentation;

            writer.WritePropertyName ( ( (uint) element.Tag ).ToString ( "X8", null ) );
            writer.WriteStartObject ( );

            writer.WritePropertyName ( "temp" );
            writer.WriteValue ( element.Tag.DictionaryEntry.Keyword );

            writer.WritePropertyName ( "vr" );
            writer.WriteValue ( element.ValueRepresentation.Code );


            switch ( element.ValueRepresentation.Code ) 
            {
                case foDicom.DicomVRCode.SQ:
                {
                    WriteVR_SQ ( (foDicom.DicomSequence) element, writer );                
                }
                break;

                case foDicom.DicomVRCode.PN:
                {
                    WriteVR_PN ( (foDicom.DicomElement) element, writer );
                }
                break;

                case foDicom.DicomVRCode.OB:
                case foDicom.DicomVRCode.OD:
                case foDicom.DicomVRCode.OF:
                case foDicom.DicomVRCode.OW:
                case foDicom.DicomVRCode.OL:
                case foDicom.DicomVRCode.UN:
                { 
                    WriteVR_Binary ( element, writer );                    
                }
                break;

                default:
                {
                    WriteVR_Default ( ds, (foDicom.DicomElement) element, writer, dicomVr );                
                }
                break;
            }

            writer.WriteEndObject ( );
        }

        protected virtual void WriteVR_SQ ( foDicom.DicomSequence element, JsonWriter writer )
        {
            for ( int index = 0; index < element.Items.Count; index++ )
            {
                StringBuilder sqBuilder = new StringBuilder ( );
                StringWriter sw = new StringWriter ( sqBuilder );

                using ( JsonWriter sqWriter = new JsonTextWriter ( sw ) )
                {
                    var item = element.Items[index];
                    
                    
                    sqWriter.Formatting = Formatting.Indented;//TODO: make it an option

                    sqWriter.WriteStartArray ( );

                    sqWriter.WriteStartObject ( );
                    
                    if ( null != item )
                    {
                        WriteChildren ( item, sqWriter );
                    }
                    
                    sqWriter.WriteEndObject ( );
                    sqWriter.WriteEndArray ( );

                }

                WriteSequenceValue ( writer, sqBuilder.ToString ( ) );
            }
        }

        protected virtual void WriteSequenceValue ( JsonWriter writer, string data )
        {
            writer.WritePropertyName ( JsonConstants.ValueField );
            writer.WriteRawValue ( data );
        }

        protected virtual void WriteVR_Default
        ( 
            foDicom.DicomDataset ds, 
            foDicom.DicomElement element, 
            JsonWriter writer, 
            foDicom.DicomVR dicomVr 
        )
        {
            WriteValue (  ds, element, writer );
        }

        protected virtual void WriteVR_Binary ( foDicom.DicomItem item, JsonWriter writer )
        {
            if ( item is foDicom.DicomFragmentSequence )
            {
                var dicomfragmentSq = (foDicom.DicomFragmentSequence) item;
            
                foreach ( var fragment in dicomfragmentSq )
                {
                    WriteStringValue ( writer, System.Convert.ToBase64String ( fragment.Data ) );
                }
            }
            else
            {
                var dicomElement = (foDicom.DicomElement) item;
                

                WriteStringValue ( writer, System.Convert.ToBase64String ( dicomElement.Buffer.Data ) );
            }
        }

        protected virtual void WriteVR_PN ( foDicom.DicomElement element, JsonWriter writer )
        {
            writer.WritePropertyName ( JsonConstants.ValueField );
            writer.WriteStartArray   (                          );

            for ( int index = 0; index < element.Count; index++ )
            {
                writer.WriteStartObject ( );
                
                var pnComponents = GetTrimmedString ( element.Get<string> ( ) ).Split ( '=' );

                for ( int compIndex = 0; ( compIndex < pnComponents.Length ) && ( compIndex < 3 ); compIndex++ )
                {
                    writer.WritePropertyName ( Utilities.PersonNameComponents.PN_Components[compIndex] );
                    writer.WriteValue        ( GetTrimmedString ( pnComponents[compIndex] )            );
                    writer.WriteEndObject    (                                                         );
                }
            }

            writer.WriteEndArray ( );
        }

        protected virtual void WriteValue 
        (   
            foDicom.DicomDataset ds, 
            foDicom.DicomElement element, 
            JsonWriter writer 
        )
        {
            foDicom.DicomVR dicomVr = element.ValueRepresentation ;


            for ( int index = 0; index < element.Count; index++ )
            {
                string stringValue = element.Get<string> ( ) ;

                if ( _numberBasedVrs.Contains ( element.ValueRepresentation.Name ) )
                {
                    WriteNumberValue ( writer, stringValue );
                }
                else
                {
                    WriteStringValue ( writer, stringValue );
                }
            }
        }


        protected virtual void WriteStringValue ( JsonWriter writer, string data )
        {
            data = data ?? "";

            data = GetTrimmedString ( data );
            writer.WritePropertyName ( JsonConstants.ValueField );
            writer.WriteStartArray ( );
            writer.WriteValue ( data ); //TODO: can/should trim?
            writer.WriteEndArray ( );

        }

        protected virtual void WriteNumberValue ( JsonWriter writer, string data )
        {
            data = data ?? "";

            data = GetTrimmedString ( data );
            writer.WritePropertyName ( JsonConstants.ValueField );
            writer.WriteStartArray ( );
            writer.WriteValue ( data ); //TODO: handle numbers to be with no ""
            writer.WriteEndArray ( );
        }

        #endregion

        private string GetTrimmedString ( string value )
        {
            return value.TrimEnd ( PADDING );
        }

        private static char[] PADDING = new char[] { '\0', ' ' };

        private static List<string> _numberBasedVrs = new List<string>();
        private const string QuoutedStringFormat = "\"{0}\"";
        private const string QuoutedKeyValueStringFormat = "\"{0}\":\"{1}\"";
        private const string QuoutedKeyValueArrayFormat = "\"Value\":[\"{0}\"]";
        private const string SequenceValueFormatted = "\"Value\":[{\"{0}\"}]";
        private const string NumberValueFormatted = "\"Value\":[{1}]";

        private abstract class JsonConstants
        {
            public const string ValueField = "Value";
            public const string Alphabetic = "Alphabetic";
        }
    }
}
