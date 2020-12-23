namespace Data.Core.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Regions")]
    public class Region
    {
        [Column("RegionId"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public long RegionId { get; init; }
        
        [Column("Name"), Required]
        public string Name { get; init; }
        
        [Column("CreationTimestamp"), Required]
        public DateTime CreationTimestamp { get; init; }
    }
}