using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using EventManagementApi.DTO;
using EventManagementApi.Entity;
using EventManagementApi.Database;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManagementApi.Controllers
{
    [Route("api/v1/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly BlobServiceClient _blobServiceClient;

        public AccountsController(
            IConfiguration configuration,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            BlobServiceClient blobServiceClient)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _blobServiceClient = blobServiceClient;
        }

        // Register a new user (Accessible by all, typically used for user self-registration)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] AccountCreateDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return Ok(new { Message = "User registered successfully" });
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountLoginDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid login attempt" });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            var tokenString = GenerateJwtToken(user);

            return Ok(new { Token = tokenString });
        }

        // General account management (Accessible by authenticated users)
        [HttpGet("profile")]
        [Authorize()]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new { user.UserName, user.Email, user.FullName });
        }

        // Update account details (Accessible by authenticated users)
        [HttpPut("update")]
        [Authorize()]
        public async Task<IActionResult> UpdateProfile([FromBody] AccountUpdateDto model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            user.Email = model.Email;
            user.FullName = model.FullName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User profile updated successfully" });
            }

            return BadRequest(result.Errors);
        }

        // Admin-related tasks (Accessible by Admins)
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            var users = _context.Users.Select(u => new { u.Id, u.UserName, u.Email, u.FullName }).ToList();
            return Ok(users);
        }

        [HttpDelete("/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { Message = $"User with ID {id} deleted successfully" });
            }

            return BadRequest(result.Errors);
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
                                {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName)
                        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
