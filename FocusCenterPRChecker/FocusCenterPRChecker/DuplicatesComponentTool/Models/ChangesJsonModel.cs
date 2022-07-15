namespace FocusCenterPRChecker.DuplicatesComponentTool.Models
{
    public class ChangesJsonModel
    {
        public object changeCounts { get; set; }
        public ChangeModel[] Changes { get; set; }
    }
    public class ChangeModel
    {
        public ItemModel Item { get; set; }
        public string ChangeType { get; set; }

    }

    public class ItemModel
    {
        public string GitObjectType { get; set; }
        public string Path { get; set; }
    }
}
