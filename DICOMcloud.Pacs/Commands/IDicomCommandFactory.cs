namespace DICOMcloud.Pacs.Commands
{
    public interface IDicomCommandFactory
    {
        IDeleteCommand CreateDeleteCommand ( );
        IStoreCommand CreateStoreCommand ( );
    }
}