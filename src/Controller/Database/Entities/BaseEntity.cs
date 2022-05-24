using System.ComponentModel.DataAnnotations;

namespace Controller.Database.Entities
{
    /// <summary>
    /// Base entity so that we don't have to add Id and [Key] to each entity
    /// </summary>
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
