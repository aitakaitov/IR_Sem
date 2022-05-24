namespace Model.Queries
{
    /// <summary>
    /// Generic query
    /// </summary>
    public class BasicQuery
    {
        public string QueryText { get; set; } = string.Empty;
        public int TopCount { get; set; } = 10;
    }
}
