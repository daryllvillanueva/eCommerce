namespace BestStoreMVC.Models
{
    public class StoreSearchModel
    {
        public int Id { get; set; }
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Sort { get; set; }
    }
}
