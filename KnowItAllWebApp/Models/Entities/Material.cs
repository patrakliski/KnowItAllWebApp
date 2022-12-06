using KnowItAllWebApp.Models.Entities.Base;

namespace KnowItAllWebApp.Models.Entities
{
    public class Material : BaseEntity
    {
        // Attributes:
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
