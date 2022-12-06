using KnowItAllWebApp.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowItAllWebApp.Models.Entities
{
    [Table("Offer")]
    public class Offer : BaseEntity
    {
        // Attributes:
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }
        [Required]
        public string Time { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public bool Status { get; set; } = false;
        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
