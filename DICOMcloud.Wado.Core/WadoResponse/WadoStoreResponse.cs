﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;
using DICOMcloud.Dicom;
using DICOMcloud.Dicom.Data;
using DICOMcloud.Pacs;
using DICOMcloud.Pacs.Commands;
using System.Net;

namespace DICOMcloud.Wado.Core
{
    public class WadoStoreResponse
    {
        private fo.DicomDataset _dataset ;
        public RetieveUrlProvider UrlProvider { get; set; }
        public string StudyInstanceUID { get; private set; }
        public HttpStatusCode HttpStatus { get; private set ; }

        private bool _successAdded = false ;
        private bool _failureAdded = false ;

        public WadoStoreResponse ( )
        : this ( "" )
        {

        }

        public WadoStoreResponse ( string studyInstanceUID )
        {
            _dataset         = new fo.DicomDataset ( ) ;
            UrlProvider      = new RetieveUrlProvider ( ) ;
            StudyInstanceUID = studyInstanceUID ;
            HttpStatus       = HttpStatusCode.Unused ;
        }

        public void AddResult ( StoreResult result ) 
        {
            switch ( result.Status )
            {
                case CommandStatus.Success:
                {
                    AddSuccessItem ( result ) ;
                }
                break;

                case CommandStatus.Failed:
                {
                    AddFailedItem ( result ) ;
                }
                break;

                default:
                {}
                break;
            }
        }

        public fo.DicomDataset GetResponseContent ( )
        {
            _dataset.AddOrUpdate<string>(fo.DicomTag.RetrieveURI, UrlProvider.GetStudyUrl ( StudyInstanceUID ?? "" ) ) ;
        
            return _dataset ;
        }

        public void AddResult ( Exception ex, Stream dicomStream )
        {
            fo.DicomFile dataSet = fo.DicomFile.Open ( dicomStream ) ;
            
            AddFailedItem ( new StoreResult ( ) { DataSet = GetReferencedInstsance ( dataSet.Dataset ), 
                                                  Error =  ex,
                                                  Status = CommandStatus.Failed,
                                                  Message = "Error" } ) ;
        }

        private void AddFailedItem ( StoreResult result )
        {
            var ds                 = result.DataSet ;
            var referencedInstance = GetReferencedInstsance ( ds ) ;
            var failedSeq          = new fo.DicomSequence ( fo.DicomTag.FailedSOPSequence ) ;
            var item               = new fo.DicomDataset ( ) ;


            referencedInstance.Merge ( item ) ;

            _dataset.AddOrUpdate (failedSeq);
            failedSeq.Items.Add ( item ) ;

            item.AddOrUpdate<UInt16> (fo.DicomTag.FailureReason, 272 ) ; //TODO: for now 272 == "0110 - Processing failure", must map proper result code from org. exception
            
            if ( !string.IsNullOrEmpty (result.Message) )
            {
                //TODO: temp
                item.AddOrUpdate<string> ( fo.DicomTag.RetrieveURI, result.Message );
            }

            if ( _successAdded )
            {
                HttpStatus = HttpStatusCode.Accepted ;
            }
            else
            {
                HttpStatus = HttpStatusCode.Conflict ; //should figure out the true reason from a wrapped exception code
            }

            _failureAdded = true ;

        }

        private void AddSuccessItem ( StoreResult result )
        {
            var ds                 = result.DataSet ;
            var referencedInstance = GetReferencedInstsance ( ds ) ;
            var referencedSeq      = new fo.DicomSequence ( fo.DicomTag.ReferencedInstanceSequence ) ;
            var item               = new fo.DicomDataset ( ) ;


            referencedInstance.Merge ( item ) ;

            _dataset.AddOrUpdate ( referencedSeq ) ;
            referencedSeq.Items.Add ( item ) ;
            
            item.AddOrUpdate<string> (fo.DicomTag.RetrieveURI, UrlProvider.GetInstanceUrl ( new ObjectId ( ds ) ) ) ; 


            if ( _failureAdded )
            {
                HttpStatus = HttpStatusCode.Accepted ; 
            }
            else
            {
                HttpStatus = HttpStatusCode.OK ;
            }

            _successAdded = true ;
        }

        

        private fo.DicomDataset GetReferencedInstsance ( fo.DicomDataset ds )
        {
            var classUID = ds.Get<fo.DicomElement> ( fo.DicomTag.SOPClassUID, null ) ;
            var sopUID   = ds.Get<fo.DicomElement> ( fo.DicomTag.SOPInstanceUID, null ) ;
            var dataset  = new fo.DicomDataset ( ) ;


            dataset.AddOrUpdate ( classUID ) ;
            dataset.AddOrUpdate ( sopUID ) ;

            return dataset ;
        }
    }
}

//6.6.1.3.2.1.2 Failure Reason
//*****************************
//A7xx - Refused out ofResources
//The STOW-RS Service did not store the instance because it was out of resources.

//A9xx - Error: Data Set does notmatch SOP Class
//The STOW-RS Service did not store the instance because the instance does not conform to itsspecified SOP Class.

//Cxxx - Error: Cannotunderstand
//The STOW-RS Service did not store the instance because it cannot understand certain Data Ele-ments.

//C122 - Referenced TransferSyntax not supported
//The STOW-RS Service did not store the instance because it does not support the requestedTransfer Syntax for the instance.

//0110 - Processing failure
//The STOW-RS Service did not store the instance because of a general failure in processing theoperation.

//0122 - Referenced SOP Classnot supported
//The STOW-RS Service did not store the instance because it does not support the requested SOPClass.
//*********************************
