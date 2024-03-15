using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Contracts;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using backend.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/account")]
    public class AccountController(
        ILogger<AccountController> logger,
        ApiDbContext context,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEmailService emailService,
        IUser userAccount,
        IConfiguration config
    ) : ControllerBase
    {
        private readonly ILogger<AccountController> _logger = logger;
        private readonly ApiDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IUser _userAccount = userAccount;
        private readonly IConfiguration _config = config;

        [HttpPost("register")]
        /*
        0 - user already exists;
        1 - email failed;
        */
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new
                    {
                        Errors = ModelState.Values.SelectMany(v =>
                            v.Errors.Select(e => e.ErrorMessage)
                        )
                    }
                );
            }
            // System.Console.WriteLine("Model is valid!");
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null)
            {
                return BadRequest(new { Message = 0 });
            }
            // System.Console.WriteLine("User does not exists!");
            var newUser = new User
            {
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            // System.Console.WriteLine("User created!");
            // System.Console.WriteLine(newUser);
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                // System.Console.WriteLine("Result succeeded!");
                var userToken = GenerateEmailToken(newUser, Usage.EmailConfirmation);
                // System.Console.WriteLine($"Email token created");
                userToken.TokenValue = await _userManager.GenerateEmailConfirmationTokenAsync(
                    newUser
                );
                // System.Console.WriteLine($"Email token changed: {userToken.TokenValue}!");
                userToken.TokenValue = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(userToken.TokenValue)
                );
                // System.Console.WriteLine($"Email token changed again: {userToken.TokenValue}!");
                await _context.Tokens.AddAsync(userToken);
                // System.Console.WriteLine("User token added to the database!");
                await _context.SaveChangesAsync();
                // System.Console.WriteLine("User token saved!");
                var emailSent = await _emailService.SendConfirmationEmail(
                    newUser.Email,
                    userToken.TokenValue
                );
                if (!emailSent)
                {
                    _context.Tokens.Remove(userToken);
                    await _context.SaveChangesAsync();
                    await _userManager.DeleteAsync(newUser);
                    return BadRequest(new { Message = 1 });
                }
                // System.Console.WriteLine("Email sent!");
                return Ok();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            // System.Console.WriteLine("Something Failed!");
            return BadRequest(
                new
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                }
            );
        }

        [HttpGet("confirm-email")]
        /*
        0 - Invalid Token
        1 - Token not found
        2 - Token expired
        3 - User not found
        4 - Something Failed
        */
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound(new { Message = 0 });
            }

            var userToken = _context.Tokens.FirstOrDefault(t => t.TokenValue == token);
            if (userToken is null)
            {
                return NotFound(new { Message = 1 });
            }

            DateTime currentTime = DateTime.UtcNow;

            if (userToken.CreatedAt > currentTime || currentTime > userToken.ExpirationDate)
            {
                return BadRequest(new { Message = 2 });
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userToken.UserID);

            if (user is null)
            {
                return NotFound(new { Message = 3 });
            }

            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(userToken.TokenValue)
            );
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                _context.Tokens.Remove(userToken);
                await _context.SaveChangesAsync();

                return Ok();
            }
            return BadRequest(new { Message = 4 });
        }

        [HttpPost("resend-email-confirmation")]
        /*
        0 - token expired
        1 -
        */
        public async Task<IActionResult> ResendEmailConfirmation(
            [FromBody] ForgotPasswordViewModel model
        )
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || user.EmailConfirmed == true)
            {
                return NotFound();
            }

            var token = await _context.Tokens.FirstOrDefaultAsync(t =>
                t.UserID == user.Id && t.Usage == Usage.EmailConfirmation
            );
            if (token is not null)
            {
                System.Console.WriteLine($"Token = {token}");
                if (token.CreatedAt.AddMinutes(2) > DateTime.UtcNow)
                {
                    return BadRequest();
                }
                _context.Tokens.Remove(token);
                await _context.SaveChangesAsync();
            }

            var userToken = GenerateEmailToken(user, Usage.EmailConfirmation);
            userToken.TokenValue = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            userToken.TokenValue = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(userToken.TokenValue)
            );

            _context.Tokens.Add(userToken);
            await _context.SaveChangesAsync();
            await _emailService.SendConfirmationEmail(user.Email, userToken.TokenValue);

            return Ok();
        }

        [HttpPost("login")]
        /*
        0 - Login Failed;
        1 - Invalid email or password;
        */
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new
                    {
                        Message = 0,
                        Errors = ModelState.Values.SelectMany(v =>
                            v.Errors.Select(e => e.ErrorMessage)
                        )
                    }
                );
            }
            // System.Console.WriteLine(ConsoleColors.Green("Model is valid!"));
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !user.EmailConfirmed)
            {
                return NotFound();
            }
            // System.Console.WriteLine(ConsoleColors.Green("User exists and has email confirmed!"));
            bool checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!checkPassword)
            {
                // System.Console.WriteLine(ConsoleColors.Red("Password is wrong!"));
                return BadRequest(new { Message = 1 });
            }
            // System.Console.WriteLine(ConsoleColors.Green($"Password is {checkPassword}"));
            var userSession = new UserSession(user.Id, user.FirstName, user.LastName, user.Email);
            // System.Console.WriteLine(ConsoleColors.Green($"User session is {userSession}"));
            string token = GenerateJwtToken(userSession);
            // System.Console.WriteLine(ConsoleColors.Green($"User token is {token}"));
            return Ok(new { Token = token });
        }

        [HttpPost("change-password")]
        [Authorize]
        /*
        1 - Old password == new password;
        2 - Failed to change password;
        3 - Incorect password;
        4 - Failed;
        */
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new
                    {
                        Message = 4,
                        Errors = ModelState.Values.SelectMany(v =>
                            v.Errors.Select(e => e.ErrorMessage)
                        )
                    }
                );
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.CurrentPassword,
                false
            );
            if (result.Succeeded)
            {
                if (model.CurrentPassword == model.NewPassword)
                {
                    return BadRequest(new { Message = 1 });
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword
                );
                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok();
                }
                return BadRequest(
                    new
                    {
                        Message = 2,
                        Errors = changePasswordResult.Errors.Select(e => e.Description)
                    }
                );
            }
            return BadRequest(new { Message = 3 });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return NotFound();
            }
            // System.Console.WriteLine(ConsoleColors.Green("user exists!"));
            var token = await _context.Tokens.FirstOrDefaultAsync(t =>
                t.UserID == user.Id && t.Usage == Usage.PasswordReset
            );
            if (token is not null)
            {
                // System.Console.WriteLine(ConsoleColors.Green("token exists!"));
                if (token.CreatedAt.AddMinutes(2) > DateTime.UtcNow)
                {
                    return BadRequest();
                }
                _context.Tokens.Remove(token);
                await _context.SaveChangesAsync();
                // System.Console.WriteLine(ConsoleColors.Green("token removed!"));
            }

            var userToken = GenerateEmailToken(user, Usage.PasswordReset);
            // System.Console.WriteLine(ConsoleColors.Green("token generated!"));
            userToken.TokenValue = await _userManager.GeneratePasswordResetTokenAsync(user);
            // System.Console.WriteLine(ConsoleColors.Green($"token value: {userToken.TokenValue}!"));
            userToken.TokenValue = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(userToken.TokenValue)
            );
            // System.Console.WriteLine(ConsoleColors.Green($"token value encoded: {userToken.TokenValue}!"));
            _context.Tokens.Add(userToken);
            // System.Console.WriteLine(ConsoleColors.Green($"token added to db!"));
            await _context.SaveChangesAsync();
            // System.Console.WriteLine(ConsoleColors.Green($"token saved!"));
            await _emailService.SendPasswordResetEmail(user.Email, userToken.TokenValue);
            // System.Console.WriteLine(ConsoleColors.Green($"email sent!"));
            return Ok();
        }

        [HttpGet("check-token")]
        /*
        0 - Invalid Token
        1 - Token not found
        2 - Token expired
        3 - User not found
        */
        public async Task<IActionResult> CheckToken([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return NotFound(new { Message = 0 });
            }
            // System.Console.WriteLine(ConsoleColors.Green("Token is not null or empty!"));
            var userToken = _context.Tokens.FirstOrDefault(t => t.TokenValue == token);
            if (userToken is null)
            {
                // System.Console.WriteLine(ConsoleColors.Red("Token is null!"));
                return NotFound(new { Message = 1 });
            }
            // System.Console.WriteLine(ConsoleColors.Green($"Token exists, token = {userToken.TokenValue}!"));
            DateTime currentTime = DateTime.UtcNow;

            if (userToken.CreatedAt > currentTime || currentTime > userToken.ExpirationDate)
            {
                return BadRequest(new { Message = 2 });
            }
            // System.Console.WriteLine(ConsoleColors.Green("Token is not expired!"));
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userToken.UserID);
            if (user is null)
            {
                return NotFound(new { Message = 3 });
            }
            // System.Console.WriteLine(ConsoleColors.Green("User is not null!"));
            return Ok();
        }

        [HttpPost("recover-password")]
        /*
        0 - Model failed;
        1 - Token not found;
        2 - User not found;
        3 - Something failed;
        */
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new
                    {
                        Message = 0,
                        Errors = ModelState.Values.SelectMany(v =>
                            v.Errors.Select(e => e.ErrorMessage)
                        )
                    }
                );
            }
            System.Console.WriteLine(ConsoleColors.Green("Model is valid!"));
            var userToken = _context.Tokens.FirstOrDefault(t => t.TokenValue == model.Token);
            if (userToken is null)
            {
                System.Console.WriteLine(ConsoleColors.Red("Token is not valid!"));
                return NotFound(new { Message = 1 });
            }
            System.Console.WriteLine(ConsoleColors.Green("Token is valid!"));
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userToken.UserID);
            if (user is null)
            {
                System.Console.WriteLine(ConsoleColors.Red("User is not valid!"));
                return NotFound(new { Message = 2 });
            }
            System.Console.WriteLine(ConsoleColors.Green("User is valid!"));

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                model.NewPassword
            );
            if (result.Succeeded)
            {
                System.Console.WriteLine(ConsoleColors.Green("Password Changed!"));
                _context.Tokens.Remove(userToken);
                await _context.SaveChangesAsync();
                return Ok();
            }
            System.Console.WriteLine(ConsoleColors.Red("Something failed!"));
            foreach (var error in result.Errors)
            {
                System.Console.WriteLine(ConsoleColors.Red($"Error: {error.Description}"));
            }

            return BadRequest(new { Message = 3 });
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> Info()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                var products = _context.Products.Where(p => p.User == user);
                var wearers = _context.Wearers.Where(w => w.User == user);
                var userData = new
                {
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    products,
                    wearers
                };
                return Ok(new { user = userData });
            }
            return BadRequest();
        }

        private static Token GenerateEmailToken(User user, Usage usage)
        {
            var token = new Token
            {
                UserID = user.Id,
                Status = false,
                Usage = usage,
                CreatedAt = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddMinutes(60)
            };

            return token;
        }

        private string GenerateJwtToken(UserSession user)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }
                ),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }

        public static class ConsoleColors
        {
            public static string Red(string value) => $"\u001b[31m{value}\u001b[0m";

            public static string Green(string value) => $"\u001b[32m{value}\u001b[0m";

            public static string Yellow(string value) => $"\u001b[33m{value}\u001b[0m";
        }
    }
}
