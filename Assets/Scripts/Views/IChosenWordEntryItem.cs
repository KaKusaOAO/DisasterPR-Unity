public interface IChosenWordEntryItem
{
    public WordCardHolder Holder { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsSelected { get; set; }
    public bool IsEnabled { get; set; }
    public void Init();
}