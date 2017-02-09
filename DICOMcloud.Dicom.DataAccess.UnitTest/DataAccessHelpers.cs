using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICOMcloud.Dicom.DataAccess;
using fo = Dicom;


namespace DICOMcloud.Dicom.DataAccess.UnitTest
{
    public class DataAccessHelpers
    {
        public DataAccessHelpers ( ) 
        {
            throw new NotImplementedException ( "specify a connection string below" ) ;
            //TODO: To run the test against a database, uncomment the line below and pass the connection string to your database
            //DataAccess = new DicomInstanceArchieveDataAccess ( "" ) ;
        }
        
        public IDicomInstanceArchieveDataAccess DataAccess { get; set; }
    }
}
