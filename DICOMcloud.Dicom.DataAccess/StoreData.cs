using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Dicom.Data;

namespace DICOMcloud.Dicom.DataAccess
{
    public class InstanceMetadata
    {
        public InstanceMetadata ( )
        {

        }

        public string Owner
        {
            get ;
            set ;
        }

        public DicomMediaLocations[] MediaLocations
        {
            get ; 
            set ; 
        }

        public Dictionary<string,string> Properties
        {
            get; set;
        }
    }

    public class DicomMediaLocations
    {
        public string MediaType
        {
            get; set;
        }

        public List<MediaLocationParts> Locations
        {
            get; set;
        }
    }

    public class MediaLocationParts
    {
        public string[] Parts
        {
            get; set;
        }
    }
}
