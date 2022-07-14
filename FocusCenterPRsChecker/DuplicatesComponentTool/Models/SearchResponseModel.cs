namespace FocusCenterPRsChecker.DuplicatesComponentTool.Models
{
    public class SearchResponseModel
    {
        public int Count { get; set; }
        public FoundComponent[] Results { get; set; }
    }

    public class FoundComponent
    {
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}
