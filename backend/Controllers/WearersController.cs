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
    [Route("/api/wearers")]
    public class WearerController(
        ILogger<WearerController> logger,
        ApiDbContext context,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEmailService emailService,
        IUser userAccount,
        IConfiguration config
    ) : ControllerBase
    {
        private readonly ILogger<WearerController> _logger = logger;
        private readonly ApiDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IUser _userAccount = userAccount;
        private readonly IConfiguration _config = config;

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetWearers()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var wearers = _context.Wearers.Where(p => p.User == user);
            if (wearers is null)
            {
                return NotFound();
            }

            List<Wearer> wearersData = [];

            foreach (var wearer in wearers)
            {
                var newWearer = new Wearer
                {
                    Id = wearer.Id,
                    Name = wearer.Name,
                    Age = wearer.Age
                };
                wearersData.Add(newWearer);
            }
            return Ok(new { wearers = wearersData });
        }

        [HttpGet("{id}")]
        [Authorize]
        /*
        0 - wearer not found;
        1 - wearer has no product/(s)
        */
        public async Task<IActionResult> GetWearer(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var wearer = await _context.Wearers.FirstOrDefaultAsync(w =>
                w.User == user && w.Id == id
            );
            if (wearer is null)
            {
                return NotFound(new { Message = 0 });
            }

            var products = await _context
                .Products.Where(p => p.User == user && p.Wearer == wearer)
                .Select(p => new Product
                {
                    Id = p.Id,
                    SerialNumber = p.SerialNumber,
                    DeviceID = p.DeviceID
                })
                .ToListAsync();

            if (products.Count == 0)
            {
                products = [];
            }

            var wearerData = new
            {
                Id = wearer.Id,
                Age = wearer.Age,
                Name = wearer.Name,
            };
            Console.WriteLine(wearerData.Name);

            var responseData = new { Wearer = wearerData, Products = products };

            return Ok(responseData);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegisterWearer([FromBody] RegisterWearerViewModel model)
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

            var wearer = new Wearer
            {
                Name = model.WearerName,
                Age = model.WearerAge,
                User = user,
                UserID = user.Id
            };

            _context.Wearers.Add(wearer);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteWearer(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var wearer = await _context.Wearers.FirstOrDefaultAsync(p =>
                p.User == user && p.Id == id
            );
            if (wearer is null)
            {
                return NotFound();
            }

            _context.Wearers.Remove(wearer);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
