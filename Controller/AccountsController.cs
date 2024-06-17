using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using EventManagementApi.DTO;
using EventManagementApi.Entity;
using EventManagementApi.Database;

namespace EventManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BlobServiceClient _blobServiceClient;

        public AccountsController(
            IConfiguration configuration,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            BlobServiceClient blobServiceClient)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid Id)
        {
            var user = await _userManager.FindByIdAsync(id);

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
    }
}
