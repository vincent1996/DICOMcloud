using DICOMcloud.Dicom.DataAccess.DB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.DataAccess.DB
{
    public partial class SqlDeleteStatments
    {
        public SqlDeleteStatments ( ) : this ( new StorageDbSchemaProvider ( ) )
        {}

        public SqlDeleteStatments ( StorageDbSchemaProvider schemaProvider )
        {
            SchemaProvider = schemaProvider ;
        }

        public StorageDbSchemaProvider SchemaProvider { get; set; }
    }
}
