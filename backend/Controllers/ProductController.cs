using backend.Contracts;
using backend.Data;
using backend.Models;
using backend.Services;
using backend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/products")]
    public class ProductController(
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegisterProduct([FromBody] RegisterProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var wearer = await _context.Wearers.FirstOrDefaultAsync(w =>
                w.Id == model.WearerID && w.User == user
            );

            if (wearer is null)
            {
                return NotFound();
            }

            if (
                await _context.Products.FirstOrDefaultAsync(p => p.DeviceID == model.DeviceGUID)
                is not null
            )
            {
                return BadRequest();
            }

            var product = new Product
            {
                SerialNumber = model.DeviceSerialNumber,
                DeviceID = model.DeviceGUID,
                User = user,
                UserID = user.Id,
                Wearer = wearer,
                WearerID = wearer.Id
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var products = _context.Products.Where(p => p.User == user).Include(p => p.Wearer);
            if (products is null)
            {
                return NotFound();
            }

            List<Product> productsData = [];
            foreach (var product in products)
            {
                var newProduct = new Product
                {
                    Id = product.Id,
                    SerialNumber = product.SerialNumber,
                    DeviceID = product.DeviceID,
                    WearerID = product.WearerID,
                };
                productsData.Add(newProduct);
            }
            return Ok(new { products = productsData });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProduct(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var product = await _context
                .Products.Include(p => p.Locations)
                .FirstOrDefaultAsync(p => p.User == user && p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            List<Location> locationsData = [];
            foreach (var location in product.Locations)
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
                WearerID = product.WearerID,
            };

            return Ok(new { Product = productData, Locations = locationsData });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p =>
                p.User == user && p.Id == id
            );
            if (product is null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
