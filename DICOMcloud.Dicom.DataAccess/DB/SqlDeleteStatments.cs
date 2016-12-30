using DICOMcloud.Dicom.DataAccess.DB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMcloud.Dicom.DataAccess.DB
{
    static class SqlDeleteStatments
    {
        public static string GetDeleteInstanceCommandText 
        ( 
            //string sopColKeyName, //0
            //string seriesColKeyName, //1
            //string studyColKeyName, //2
            //string patientColKeyName,//3
            //string sopInstanceTableName, //4
            //string seriesTableName, //5
            //string studyTableName, //6
            //string patientTableName, //7
            //string sopInstanceRefSeriesColName, //8
            //string seriesRefStudyColName, //9
            //string studyRefPatientColName, //10
            //string sopInstanceUidColName, //11
            long sopInstanceKey //12
        ) 
        {
            StorageDbSchemaProvider schemaProvider = new StorageDbSchemaProvider ( ) ;

            
            return string.Format ( Delete.Delete_Instance_Command_Formatted,
                                   schemaProvider.ObjectInstanceTable.KeyColumn.Name, 
                                   schemaProvider.SeriesTable.KeyColumn.Name, 
                                   schemaProvider.StudyTable.KeyColumn.Name, 
                                   schemaProvider.PatientTable.KeyColumn.Name,
                                   schemaProvider.ObjectInstanceTable.Name, 
                                   schemaProvider.SeriesTable.Name, 
                                   schemaProvider.StudyTable.Name, 
                                   schemaProvider.PatientTable.Name,
                                   schemaProvider.ObjectInstanceTable.ForeignColumn.Name,
                                   schemaProvider.SeriesTable.ForeignColumn,
                                   schemaProvider.StudyTable.ForeignColumn,
                                   schemaProvider.ObjectInstanceTable.ModelKeyColumns.FirstOrDefault().Name, 
                                   sopInstanceKey ) ;
        }

        public class Delete
        {
            static Delete ( )
            {
                List<string> DbAliasNames = new List<string> ( ) ;
                DbAliasNames.Add ( "sopKey") ; //0
                DbAliasNames.Add ( "seriesKey" ) ; //1
                DbAliasNames.Add ( "studyKey" ) ; //2
                DbAliasNames.Add ( "patientKey" ) ; //3
                DbAliasNames.Add ( "sopTableName" ) ; //4
                DbAliasNames.Add ( "seriesTableName" ) ; //5
                DbAliasNames.Add ( "studyTableName" ) ; //6
                DbAliasNames.Add ( "patientTableName" ) ; //7
                DbAliasNames.Add ( "sopInstanceRefSeriesColName" ) ; //8
                DbAliasNames.Add ( "seriesRefStudyColName" ) ; //9
                DbAliasNames.Add ( "studyRefPatientColName" ) ; //10
                DbAliasNames.Add ( "sopInstanceUidColName" ) ; //11
            
                Delete_Instance_Command_Formatted = __delete_Instance_Command ;

                for ( int index = 0; index < DbAliasNames.Count; index++ )
                {
                    string stringReplaced = "{" + DbAliasNames[index] + "}" ;
                    string indexReplacing = "{" + index + "}" ;
                     
                    Delete_Instance_Command_Formatted =  Delete_Instance_Command_Formatted.Replace ( stringReplaced, indexReplacing ) ;
                }

            }

            public static readonly string Delete_Instance_Command_Formatted ;
            private static readonly string __delete_Instance_Command =            
            @"
declare @sop bigint
declare @series bigint
declare @study bigint
declare @patient bigint
declare @sopCount int
declare @seriesCount int
declare @studyCount int

SELECT @sop = instance.{sopKey}, @series = ser.{seriesKey}, @study = stud.{studyKey}, @patient = p.{patientKey}
FROM {sopTableName} instance, {seriesTableName} ser, {studyTableName} stud, {patientTableName} p
where 
    instance.{sopInstanceRefSeriesColName} = ser.{seriesKey} AND 
    Ser.{seriesRefStudyColName} = stud.{studyKey} AND
    stud.{studyRefPatientColName} = p.{patientKey} AND
    instance.{sopKey} = {12}

Delete from {sopTableName} where {sopTableName}.{sopKey} = @sop

SELECT @sopCount = COUNT(*)
FROM {sopTableName} instance
WHERE instance.{sopInstanceRefSeriesColName} = @series

/* if 0 entries, remove orphaned' series record */
IF (@sopCount = 0) 
DELETE FROM {seriesTableName} WHERE (@series = {seriesTableName}.{seriesKey});

SELECT  @seriesCount = COUNT(*) FROM {seriesTableName} ser
WHERE ser.{seriesRefStudyColName} = @study

/* if 0 entries, remove orphaned study record */
IF (@seriesCount = 0) 
DELETE FROM {studyTableName} WHERE (@study = {studyTableName}.{studyKey});

SELECT  @studyCount = COUNT(*) FROM {studyTableName} stud
WHERE stud.{studyRefPatientColName} = @patient

/* if 0 entries, remove orphaned patient record */
IF (@studyCount = 0) 
DELETE FROM {patientTableName} WHERE (@patient = {patientTableName}.{patientKey});
            ";
        }
    }
}
