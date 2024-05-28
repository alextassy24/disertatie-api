using backend.Contracts;
using backend.Data;
using backend.Models;
using backend.Services;
using backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/locations")]
    public class LocationController(
        ILogger<ProductController> logger,
        ApiDbContext context,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEmailService emailService,
        IUser userAccount,
        IConfiguration config
    ) : ControllerBase
    {
        private readonly ILogger<ProductController> _logger = logger;
        private readonly ApiDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IUser _userAccount = userAccount;
        private readonly IConfiguration _config = config;

        [HttpGet]
        [Route("/t")]
        public async Task<IActionResult> CheckConnection()
        {
            return Ok("Connection to the API successful!");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterLocation(
            [FromQuery] RegisterLocationViewModel model
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p =>
                p.DeviceID == model.DeviceGUID
            );

            if (product is null)
            {
                return NotFound();
            }

            var location = new Location
            {
                ProductID = product.DeviceID,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Time = TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm:ss"),
                Date = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLocations([FromQuery] int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p =>
                p.User == user && p.Id == productId
            );
            if (product is null)
            {
                return NotFound("Product not found");
            }

            var locations = _context.Locations.Where(l =>
                l.Product.Id == product.Id && l.Product.User == user
            );
            if (locations is null)
            {
                return NotFound("Locations not found");
            }

            List<Location> locationsData = [];
            foreach (var location in locations)
            {
                var newLocation = new Location
                {
                    Date = location.Date,
                    Time = location.Time,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                };
                locationsData.Add(newLocation);
            }

            var productData = new
            {
                Id = product.Id,
                SerialNumber = product.SerialNumber,
                DeviceID = product.DeviceID,
            };

            var responseData = new { LocationsData = locationsData, ProductData = productData };

            return Ok(responseData);
        }
    }
}
