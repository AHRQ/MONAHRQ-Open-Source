namespace Monahrq.Websites.Events
{
    public class ItemsForNewlyAddedDatasetsEventArgs
    {
        public string DatasetType { get; set; }
        public bool Refresh { get; set; }
    }
}