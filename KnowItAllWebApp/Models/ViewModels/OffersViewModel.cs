using KnowItAllWebApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KnowItAllWebApp.Models.ViewModels
{
    public class OffersViewModel
    {
        public IEnumerable<Offer> Offers { get; set; }
    }
}
