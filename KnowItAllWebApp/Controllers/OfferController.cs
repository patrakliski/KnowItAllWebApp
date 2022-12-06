using KnowItAllWebApp.DataAccess;
using KnowItAllWebApp.Models;
using KnowItAllWebApp.Models.Entities;
using KnowItAllWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Security.Policy;

namespace KnowItAllWebApp.Controllers
{
    public class OfferController : Controller
    {
        #region Private Fields
        private readonly KnowItAllContext _context;
        #endregion

        public OfferController(KnowItAllContext context
            )
        {
            _context = context;
        }

        #region Public Properties:
        public OfferMaterialsViewModel OfferMaterialsViewModel { get; set; } = new();
        public OffersViewModel OffersViewModel { get; set; } = new();
        [TempData]
        public string Message { get; set; }
        #endregion

        #region Public Methods:
        // On Initial load Offer is empty, empty Material fields are shown
        // After Material is added to Offer, OfferMaterials are shown on the side, not calculated
        // Calculate event is trigered on click on Calculate button
        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            var _offerId = HttpContext.Request.Query["id"];

            // if offer is not created
            if (id == null)
            {
                OfferMaterialsViewModel = new OfferMaterialsViewModel();

                OfferMaterialsViewModel.OfferMaterial = new OfferMaterial
                {
                    OfferId = Guid.NewGuid()
                };
                OfferMaterialsViewModel.Offer = new Offer();
                OfferMaterialsViewModel.Offer.Id = OfferMaterialsViewModel.OfferMaterial.OfferId;
                OfferMaterialsViewModel.Offer.Time = "";

                _context.Add(OfferMaterialsViewModel.Offer); // After first material is added, offer is created
                await _context.SaveChangesAsync();
                OfferMaterialsViewModel.MaterialsSelectList = await MaterialsSelectList();
            }
            else
            {
                OfferMaterialsViewModel = new OfferMaterialsViewModel();
                OfferMaterialsViewModel.OfferMaterial = new OfferMaterial
                {
                    OfferId = Guid.Parse(id)
                };
                OfferMaterialsViewModel.MaterialsSelectList = await MaterialsSelectList();
                OfferMaterialsViewModel.OfferMaterials = await _context.OfferMaterial.Where(x => x.OfferId == Guid.Parse(id)).ToListAsync();
                OfferMaterialsViewModel.Offer = await _context.Offer.Where(x => x.Id == Guid.Parse(id)).FirstAsync();
            }

            return View(OfferMaterialsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(OfferMaterialsViewModel offerViewModel)
        {
            var _currentOfferMaterialList = await GetAllOfferMaterials(offerViewModel.Offer.Id);
            if (_currentOfferMaterialList.Count() >= 2)
            {
                var _offer = CalculateOfferTimeAndPrice(_currentOfferMaterialList);
                _offer.Id = offerViewModel.Offer.Id;

                // update the Offer after calculate
                _context.Update(_offer);
                _context.Entry(_offer).State = EntityState.Unchanged;
                _context.Entry(_offer).Property(x => x.Time).IsModified = true;
                _context.Entry(_offer).Property(x => x.Price).IsModified = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                Message = $"Please add at least two Materials!";
            }

            return RedirectToAction("Index", new { id = offerViewModel.Offer.Id });
        }


        // Method that adds Material to Offer
        [HttpPost]
        public async Task<IActionResult> AddOfferMaterial(OfferMaterialsViewModel offerViewModel)
        {
            OfferMaterialsViewModel = new OfferMaterialsViewModel();
            List<OfferMaterial> _offerMaterialsList = new List<OfferMaterial>();
            OfferMaterialsViewModel.OfferMaterial = offerViewModel.OfferMaterial;
            OfferMaterialsViewModel.OfferMaterial.Id = Guid.NewGuid();
            _offerMaterialsList.Add(offerViewModel.OfferMaterial);

            // Rules :
            // When there is cotton, there must 3:1 water, 
            // and when there is sand there must be 2:1 water
            switch (offerViewModel.OfferMaterial.MaterialId.ToString().ToUpper())
            {
                // Cotton
                case "130814F9-323F-4278-9547-4EF1B420F691":

                    OfferMaterial _offerMaterialCottonWater = new OfferMaterial();
                    _offerMaterialCottonWater.Id = Guid.NewGuid();
                    _offerMaterialCottonWater.MaterialId = Guid.Parse("B36D70BC-CBB6-43A6-B9A5-6E90696483FA");
                    _offerMaterialCottonWater.Material = await _context.Material.Where(x => x.Id == _offerMaterialCottonWater.MaterialId).FirstAsync();
                    _offerMaterialCottonWater.OfferId = offerViewModel.OfferMaterial.OfferId;
                    _offerMaterialCottonWater.Quantity += (offerViewModel.OfferMaterial.Quantity / 3);
                    _offerMaterialsList.Add(_offerMaterialCottonWater);
                    break;
                // Sand
                case "A14368EE-7659-4BC0-A0E0-74312E6DEDB7":
                    OfferMaterial _offerMaterialSandWater = new OfferMaterial();
                    _offerMaterialSandWater.Id = Guid.NewGuid();
                    _offerMaterialSandWater.MaterialId = Guid.Parse("B36D70BC-CBB6-43A6-B9A5-6E90696483FA");
                    _offerMaterialSandWater.Material = await _context.Material.Where(x => x.Id == _offerMaterialSandWater.MaterialId).FirstAsync();
                    _offerMaterialSandWater.OfferId = offerViewModel.OfferMaterial.OfferId;
                    _offerMaterialSandWater.Quantity += (offerViewModel.OfferMaterial.Quantity / 2);
                    _offerMaterialsList.Add(_offerMaterialSandWater);
                    break;
                default:
                    break;
            }

            OfferMaterialsViewModel.OfferMaterials = _offerMaterialsList;

            OfferMaterialsViewModel.Offer = PopulateOffer(offerViewModel.OfferMaterial.OfferId);

            _context.Update(OfferMaterialsViewModel.Offer); // Offer is updated
            _context.Entry(OfferMaterialsViewModel.Offer).Property(x => x.Number).IsModified = false; // Number is autoincrementing in Db,
                                                                                                      // so we set the object not to be updated

            foreach (var item in OfferMaterialsViewModel.OfferMaterials)
            {
                _context.Add(item); // Offer Material is ready to be added to Db
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id = offerViewModel.OfferMaterial.OfferId });

        }

        // Method that updates the offer Status to Accepted (true = Accepted, false = not yet Accepted)
        [HttpPost]
        public async Task<IActionResult> AcceptOffer(string id)
        {
            var _offer = new Offer();
            _offer.Id = Guid.Parse(id);
            _offer.Status = true;

            _context.Update(_offer);
            _context.Entry(_offer).State = EntityState.Unchanged;
            _context.Entry(_offer).Property(x => x.Status).IsModified = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("OffersTable");

        }

        // Returns view with populated table with all the offers from Db
        // Table consists number of offer, time to manufacture and price.
        public async Task<IActionResult> AcceptedOffersTable()
        {
            var _offersViewModel = new OffersViewModel();
            _offersViewModel.Offers = await _context.Offer.Where(x => x.Status == true).ToListAsync();
            return View(_offersViewModel);
        }

        // Returns view with populated table with all the offers from Db
        // Table consists number of offer, time to manufacture and price.
        public async Task<IActionResult> OffersTable()
        {
            var _offersViewModel = new OffersViewModel();
            _offersViewModel.Offers = await _context.Offer.ToListAsync();
            return View(_offersViewModel);
        }

        // Method that populates Offer Details modal view:
        [HttpGet]
        public async Task<PartialViewResult> OfferDetails(Guid id)
        {
            OfferMaterialsViewModel offerMaterialsViewModel = new OfferMaterialsViewModel();
            offerMaterialsViewModel.Offer = await _context.Offer.Where(x => x.Id == id).FirstAsync();
            offerMaterialsViewModel.OfferMaterials = await GetAllOfferMaterials(id);
            return PartialView("_OfferDetails", offerMaterialsViewModel);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        #region Private Methods:

        // Populate Offer to be ready to be saved/updated in Db
        private static Offer PopulateOffer(Guid offerId)
        {
            var _offer = new Offer();

            _offer.Id = offerId;

            _offer.Time = "";

            return _offer;
        }

        // Method that calculates offer Price and Time
        private Offer CalculateOfferTimeAndPrice(List<OfferMaterial> offerMaterials)
        {
            int time = 0;
            Offer _offer = new Offer();

            foreach (var item in offerMaterials)
            {
                var _materialPrice = CalculateMaterialPrice(item);
                _offer.Price += _materialPrice;
                time += CalculateOfferTime(item.Quantity, _materialPrice);
            }

            _offer.Time = ConvertTime(time);

            return _offer;
        }

        // Method that calculates Material Price by Quantity added
        private decimal CalculateMaterialPrice(OfferMaterial offerMaterial)
        {
            decimal _materialPrice = 0;
            _materialPrice = offerMaterial.Quantity * offerMaterial.Material.Price;

            return _materialPrice;
        }
               
        // Method that calculates Offer time in days
        private int CalculateOfferTime(decimal quantity, decimal materialPrice)
        {
            var _timeOverPrice = CalculateTimeOverPrice(quantity, materialPrice);

            return _timeOverPrice;
        }

        //  The rules about the price are:
        //  • When the price is <= 10, the time is less than one day
        //  • When the price is > 10 and <=25 the time is one week
        //  • When the price is > 25 <= 100 the time is one month and fifteen days
        //  • When the price is > 100 the time is six months
        private int CalculateTimeOverPrice(decimal quantity, decimal _materialPrice)
        {
            var _time = CalculateTimeOverQuantity(quantity);

            if (_materialPrice <= 10)
            {
                _time += 0;
            }
            if (_materialPrice > 10 && _materialPrice <= 25)
            {
                _time += 7; // days
            }
            if (_materialPrice > 25 && _materialPrice <= 100)
            {
                _time += 45; // 1 month and 15 days
            }
            if (_materialPrice > 100)
            {
                _time += 180; // 6 months
            }

            return _time;
        }

        //  The rules about the quantity are:
        //  • When the quantity is <= 3 don’t add any time
        //  • When the quantity is > 3 and <=12 add 3 days
        //  • When the quantity is > 12 and <= 50 add three weeks
        //  • When the quantity is > 50 add two months
        private int CalculateTimeOverQuantity(decimal quantity)
        {
            var _time = 0;

            if (quantity <= 3)
            {
                _time = 0;
            }
            if (quantity > 3 && quantity <= 12)
            {
                _time = 3; // 3 days
            }
            if (quantity > 12 && quantity <= 50)
            {
                _time = 21; // 3 weeks
            }
            if (quantity > 50)
            {
                _time = 60; // 2 months
            }

            return _time;
        }

        // Convert Time to years, months and days
        private string ConvertTime(int calculatedTimeDays)
        {
            var _currentTime = DateTime.Now;
            var test = DateTime.Now.AddDays(calculatedTimeDays);

            TimeSpan time = test.Subtract(_currentTime);

            var test1 = string.Format(" {0} years {1} months {2} days",
                                                        (int)time.TotalDays / 365,
                                                        (int)(time.TotalDays % 365) / 30,
                                                        time.Days % 30,
                                                        time.Hours,
                                                        time.Minutes,
                                                        time.Seconds);

            return test1;
        }
                
        // Returns list from database with all Offer Materials 
        private async Task<List<OfferMaterial>> GetAllOfferMaterials(Guid offerId)
        {
            var _offerMaterials = await _context.OfferMaterial.Where(x => x.OfferId == offerId).ToListAsync();

            List<OfferMaterial> _populatedOfferMaterials = new List<OfferMaterial>();

            foreach (var _offerMaterial in _offerMaterials)
            {
                // Material object is populated
                _offerMaterial.Material = await GetMaterialAsync(_offerMaterial.MaterialId);
                _populatedOfferMaterials.Add(_offerMaterial);
            }

            return _populatedOfferMaterials;
        }

        // Method to populate Material object from Db by the specific Id
        private async Task<Material> GetMaterialAsync(Guid materialId)
        {
            Material _material = new Material();
            _material = await _context.Material.Where(x => x.Id == materialId).FirstAsync();
            return _material;
        }

        // Method that populates the Material dropdown
        private async Task<List<SelectListItem>> MaterialsSelectList()
        {
            List<SelectListItem> _materialsList = new List<SelectListItem>();
            var _materials = await GetAllMaterials();

            foreach (var item in _materials)
            {
                _materialsList.Add(new SelectListItem
                {
                    Text = item.Name + " - $" + item.Price,
                    Value = item.Id.ToString()
                });
            }

            return _materialsList;
        }

        // Method that returns List of all Materials from Db
        private async Task<List<Material>> GetAllMaterials()
        {
            var _materials = await _context.Material.ToListAsync();
            return _materials;
        }

    }
}
#endregion