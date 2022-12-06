using KnowItAllWebApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KnowItAllWebApp.Models.ViewModels
{
    public class OfferMaterialsViewModel
    {
        public Material Material { get; set; }
        public List<SelectListItem> MaterialsSelectList { get; set; }
        public IEnumerable<OfferMaterial> OfferMaterials { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public Offer Offer { get; set; }
        public OfferMaterial OfferMaterial { get; set; }
    }
}
