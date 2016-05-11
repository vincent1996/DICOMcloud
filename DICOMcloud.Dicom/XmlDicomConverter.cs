using fo = Dicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace DICOMcloud.Dicom
{
    
    public interface IXmlDicomConverter : IDicomConverter<string>
    {}

    public class XmlDicomConverter : IXmlDicomConverter
    {
        protected XmlDicomWriterService WriterService {get; set; }

        static XmlDicomConverter ( ) 
        {
            PN_Components.Add ( Constants.PN_COMP_ALPHABETIC  );
            PN_Components.Add ( Constants.PN_COMP_IDEOGRAPHIC );
            PN_Components.Add ( Constants.PN_COMP_PHONETIC    );
        }

        public XmlDicomConverter()
        {
            WriterService = new XmlDicomWriterService ( ) ;
        }

        public string Convert ( fo.DicomDataset ds )
        {
            StringBuilder sb = new StringBuilder ( ) ;
            XmlWriter writer = XmlTextWriter.Create (sb);
            
            WriteHeaders(writer);

            writer.WriteStartElement(Constants.ROOT_ELEMENT_NAME) ;
            
            WriteChildren ( ds, writer ) ;
            
            writer.WriteEndElement ( ) ;

            writer.Close ( ) ;

            return sb.ToString ( ) ;
        }

        public fo.DicomDataset Convert ( string xmlDcm )
        {
            fo.DicomDataset ds = new fo.DicomDataset( ) ;
            
            
            XDocument document = XDocument.Parse ( xmlDcm ) ;

            
            //WriteHeaders(writer);

            ReadChildren(ds, document.Root );

            return ds ;
        }

        #region Write Methods

        private void WriteHeaders(XmlWriter writer)
        {
            writer.WriteStartDocument ();
        }

        private void WriteChildren ( fo.DicomDataset ds, XmlWriter writer ) 
        {
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.FileMetaInformationVersion], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.MediaStorageSopClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.MediaStorageSopInstanceUID], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.TransferSyntaxUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.ImplementationClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.ImplementationVersionName], writer ) ;

            foreach ( var element in ds.OfType<fo.DicomItem> ( ) )
            {
                //TODO:
                //WriterService.WriteElement (element,writer);
                WriteDicomAttribute ( ds, element, writer ) ;
            }
        }
        
        private void WriteDicomAttribute 
        ( 
            fo.DicomDataset ds, 
            fo.DicomItem element, 
            XmlWriter writer 
        )
        {
            if ( null == element ) { return ; }

            fo.DicomVR dicomVr = element.ValueRepresentation ;

            writer.WriteStartElement ( Constants.ATTRIBUTE_NAME ) ;

            writer.WriteAttributeString ( Constants.ATTRIBUTE_KEYWORD, element.Tag.DictionaryEntry.Keyword ) ;
            writer.WriteAttributeString ( Constants.ATTRIBUTE_TAG, element.Tag.ToString("J", null) ) ;
            writer.WriteAttributeString ( Constants.ATTRIBUTE_VR, element.ValueRepresentation.Code.ToUpper ( ) ) ;

            if ( element.Tag.IsPrivate && null != element.Tag.PrivateCreator ) 
            {
                writer.WriteAttributeString ( Constants.ATTRIBUTE_PRIVATE_CREATOR, element.Tag.PrivateCreator.Creator ) ;
            }

            if ( dicomVr.Code == fo.DicomVR.SQ.Code )
            {
                WriteSequence ( ( fo.DicomSequence ) element, writer ) ;
            }
            else
            {
                WriteElement ( ds, (fo.DicomElement) element, writer, dicomVr ) ;
            }

            writer.WriteEndElement ( ) ;
        }

        private void WriteElement(fo.DicomDataset ds, fo.DicomElement element, XmlWriter writer, fo.DicomVR dicomVr)
        {
            //Element value can be:
            // 1. PN
            // 2. Binary
            // 3. Value

            if ( dicomVr.Equals (fo.DicomVR.PN) )
            {
                for (int index = 0; index < element.Count; index++)
                {
                    writer.WriteStartElement ( Constants.PN_PERSON_NAME );
                        WriteNumberAttrib(writer, index) ;

                        var pnComponents = GetTrimmedString ( element.Get<string> ( ) ).Split ( '=') ;

                        for ( int compIndex = 0; (compIndex < pnComponents.Length) && (compIndex < 3); compIndex++ )
                        {
                            writer.WriteStartElement ( PN_Components[compIndex] ) ;

                                fo.DicomPersonName pn = new fo.DicomPersonName ( element.Tag, pnComponents[compIndex]  ) ; //TODO: >>>Include Table 10.2-1 “Person Name Components Macro”
                            
                                writer.WriteElementString ( Constants.PN_Family, pn.Last ) ;
                                writer.WriteElementString ( Constants.PN_Given, pn.First ) ;
                                writer.WriteElementString ( Constants.PN_Midlle, pn.Middle ) ;
                                writer.WriteElementString ( Constants.PN_Prefix, pn.Prefix ) ;
                                writer.WriteElementString ( Constants.PN_Suffix, pn.Suffix ) ;

                            writer.WriteEndElement ( ) ;
                        }
                    writer.WriteEndElement ( ) ;
                }
            }
            else if ( IsBinary ( dicomVr ) )
            {
                //TODO: Add BulkData element support
                byte[] data = element.Buffer.Data;

                writer.WriteBase64 ( data, 0, data.Length ) ;
            }
            else 
            {
                WriteValue(ds, element, writer);
            }
        }

        private static void WriteValue( fo.DicomDataset ds, fo.DicomElement element, XmlWriter writer )
        {
            fo.DicomVR dicomVr = element.ValueRepresentation ;


            for ( int index = 0; index < element.Count; index++ )
            {
                writer.WriteStartElement ( Constants.ATTRIBUTE_VALUE_NAME ) ;

                WriteNumberAttrib ( writer, index ) ;
                    
                if ( dicomVr.Equals(fo.DicomVR.AT))
                {
                    //TODO: check standard
                    writer.WriteString ( GetTrimmedString ( ds.Get<string> ( element.Tag, index, string.Empty ) ) ) ;
                }
                else
                {
                    writer.WriteString ( GetTrimmedString ( ds.Get<string> ( element.Tag, index, string.Empty ) ) ); 
                }

                writer.WriteEndElement ( );
            }
        }
        
        private static void WriteNumberAttrib(XmlWriter writer, int index)
        {
            writer.WriteAttributeString("number", (index + 1).ToString());
        }

        private void WriteSequence(fo.DicomSequence element, XmlWriter writer )
        {
            for ( int index = 0; index < element.Items.Count; index++ )
            {
                var item = element.Items [ index ] ;

                writer.WriteStartElement ( Constants.ATTRIBUTE_ITEM_NAME ) ;
                WriteNumberAttrib(writer, index);
                
                WriteChildren(item, writer);

                writer.WriteEndElement ( ) ;
            }
        }

        #endregion

        #region Read Methods
        
        private void ReadChildren ( fo.DicomDataset ds, XContainer document ) 
        {
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.FileMetaInformationVersion], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.MediaStorageSopClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.MediaStorageSopInstanceUID], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.TransferSyntaxUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.ImplementationClassUid], writer ) ;
            //WriteDicomAttribute ( ds, ds[fo.DicomTag.ImplementationVersionName], writer ) ;

            foreach ( var element in document.Elements (Constants.ATTRIBUTE_NAME) ) 
            {
                ReadDicomAttribute(ds, element);
            }
        }

        private void ReadDicomAttribute ( fo.DicomDataset ds, XElement element )
        {
            XAttribute              vrNode  ;
            fo.DicomTag             tag ;
            fo.DicomDictionaryEntry dicEntry ;
            fo.DicomVR              dicomVR  ;


            vrNode  = element.Attribute( Constants.ATTRIBUTE_VR ) ;
            tag     = fo.DicomTag.Parse ( element.Attribute(Constants.ATTRIBUTE_TAG).Value ) ;
            dicomVR = null ;
            
            if ( tag.IsPrivate ) 
            {
                tag = ds.GetPrivateTag ( tag ) ;
            
                if ( null != vrNode )
                {
                    dicomVR = fo.DicomVR.Parse ( vrNode.Value ) ;
                }
            }
            
            if ( null == dicomVR )
            {
                dicEntry = fo.DicomDictionary.Default[tag];
                dicomVR  = dicEntry.ValueRepresentations.FirstOrDefault ( ) ;
            }

            if ( dicomVR == fo.DicomVR.SQ )
            {
                ReadSequence ( ds, element, tag  ) ;
            }
            else
            {
                ReadElement ( ds, element, tag, dicomVR ) ;
            }

        }

        private void ReadSequence ( fo.DicomDataset ds,  XElement element, fo.DicomTag tag )
        {
            fo.DicomSequence seq = new fo.DicomSequence ( tag, new fo.DicomDataset[0] ) ;


            foreach ( var item in  element.Elements ( Constants.ATTRIBUTE_ITEM_NAME ) )
            {
                fo.DicomDataset itemDs = new fo.DicomDataset ( ) ;
                
                ReadChildren ( itemDs, item ) ;

                seq.Items.Add ( itemDs ) ;
            }

            ds.Add ( seq ) ;
        }
        
        private void ReadElement ( fo.DicomDataset ds, XElement element, fo.DicomTag tag, fo.DicomVR dicomVr )
        {
            if ( dicomVr == fo.DicomVR.PN )
            {
                string personNameValue = "" ;

                foreach ( var personNameElementValue in element.Elements ( ).OrderBy ( n=>n.Attribute (Constants.ATTRIBUTE_NUMBER)))
                {
                    foreach ( var personNameComponent in personNameElementValue.Elements ( ) )
                    {
                        if ( personNameComponent.Name == Constants.PN_COMP_ALPHABETIC || 
                             personNameComponent.Name == Constants.PN_COMP_IDEOGRAPHIC || 
                             personNameComponent.Name == Constants.PN_COMP_PHONETIC )
                        {
                            personNameValue = UpdatePersonName ( personNameValue, personNameComponent, Constants.PN_Family );
                            personNameValue = UpdatePersonName ( personNameValue, personNameComponent, Constants.PN_Given );
                            personNameValue = UpdatePersonName ( personNameValue, personNameComponent, Constants.PN_Midlle );
                            personNameValue = UpdatePersonName ( personNameValue, personNameComponent, Constants.PN_Prefix );
                            personNameValue = UpdatePersonName ( personNameValue, personNameComponent, Constants.PN_Suffix, true );

                            personNameValue = personNameValue.TrimEnd ( '^') ; // extra cleanup 

                            personNameValue += "=";
                        }
                    }

                    personNameValue = personNameValue.TrimEnd ( '=') ;
                    
                    personNameValue += "\\" ;
                }

                personNameValue = personNameValue.TrimEnd ( '\\' ) ;
                ds.Add<string> ( tag, personNameValue ) ;
            }
            else if ( IsBinary ( dicomVr ) )
            {
                var data = System.Convert.FromBase64String ( element.Value ) ;

                ds.Add<byte> ( tag, data ) ;
            }
            else 
            {
                var values = ReadValue ( element );

                ds.Add<string> ( tag, values.ToArray ( ) );
            }            
        }

        private static string UpdatePersonName
        ( 
            string personNameValue, 
            XElement personNameComponent, 
            string partName,
            bool isLastPart = false
        )
        {
            XElement partElement = personNameComponent.Element ( partName ) ;


            if ( null == partElement )
            {
                personNameValue += "" ;
            }
            else
            {
                personNameValue += partElement.Value ?? "" ;
            }

            if ( !isLastPart )
            {
                personNameValue += "^" ;
            }

            return personNameValue ;
        }

        private static IList<string> ReadValue ( XElement element )
        {
            SortedList<int,string> values = new SortedList<int, string> ( ) ;
            
            
            foreach ( var valueElement in element.Elements (Constants.ATTRIBUTE_VALUE_NAME) )
            {
                values.Add ( int.Parse ( valueElement.Attribute ( Constants.ATTRIBUTE_NUMBER ).Value ), 
                             valueElement.Value ) ;
            }

            return values.Values ;
        }

        #endregion

        private bool IsBinary ( fo.DicomVR dicomVr ) 
        {
            return dicomVr.Equals (fo.DicomVR.OB) || dicomVr.Equals(fo.DicomVR.OD) ||
                   dicomVr.Equals (fo.DicomVR.OF) || dicomVr.Equals(fo.DicomVR.OW) ||
                   dicomVr.Equals (fo.DicomVR.UN ) ;
        }

        //trimming the padding the only allowed raw value transformation in XML
        //part 19 A.1.1
        private static string GetTrimmedString ( string value )
        {
            return value.TrimEnd (PADDING) ;
        }
         
        //TODO: fo dicom VR has property to read padding char
        private static char[] PADDING = new char[] {'\0',' '};
        private static List<string> PN_Components = new List<string> ( ) ;

        private static class Constants
        {
            public const string ROOT_ELEMENT_NAME = "NativeDicomModel" ;
            public const string ATTRIBUTE_NAME = "DicomAttribute" ;
            public const string ATTRIBUTE_VALUE_NAME = "Value" ;
            public const string ATTRIBUTE_ITEM_NAME = "Item" ;

            public const string ATTRIBUTE_TAG = "tag" ;
            public const string ATTRIBUTE_VR = "vr" ;
            public const string ATTRIBUTE_NUMBER = "number" ;
            public const string ATTRIBUTE_KEYWORD = "keyword" ;
            public const string ATTRIBUTE_PRIVATE_CREATOR = "privateCreator" ;

            public const string PN_PERSON_NAME = "PersonName" ;
            public const string PN_Family = "FamilyName" ;
            public const string PN_Given  = "GivenName" ;
            public const string PN_Midlle = "MiddleName" ;
            public const string PN_Prefix = "NamePrefix" ;
            public const string PN_Suffix = "NameSuffix" ;
            public const string PN_COMP_ALPHABETIC  = "AlphabeticName" ;
            public const string PN_COMP_IDEOGRAPHIC = "IdeographicName" ;
            public const string PN_COMP_PHONETIC    = "PhoneticName" ;
        }
    }
}
