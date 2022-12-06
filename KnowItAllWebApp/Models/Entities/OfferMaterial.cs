using KnowItAllWebApp.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowItAllWebApp.Models.Entities
{

    public class OfferMaterial : BaseEntity
    {
        // Associated Dependencies:
        public Guid OfferId { get; set; }
        [Required]
        [ForeignKey("OfferId")]
        public virtual Offer Offer { get; set; }
        public Guid MaterialId { get; set; }
        [Required]
        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        // Attributes:
        [Required]
        public decimal Quantity { get; set; }        


        //// View 
        //public List<SelectListItem> MaterialsSelectList { get; set; }
    }
}
