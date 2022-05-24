namespace Controller.Database.Entities
{
    public class DocumentInfo : BaseEntity
    {
        /// <summary>
        /// ID of the document
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Position of the document in the query results
        /// </summary>
        public int OrderInQuery { get; set; }
    }
}
