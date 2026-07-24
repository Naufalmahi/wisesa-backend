using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpDesk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthApiController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "Email atau password salah." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Email atau password salah." });

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    role = user.Role.ToString(), 
                    lastLoginAt = DateTime.UtcNow
                },
                expiresIn = 28800 // 8 jam
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest(new { message = "Email sudah terdaftar." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Role = Models.Enums.UserRole.User,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { message = "Registrasi berhasil.", userId = user.Id });
        }

        [HttpGet("me/{userId?}")]
        [AllowAnonymous] 
        public async Task<IActionResult> Me(string? userId = null)
        {
            ApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(userId) && userId != "{userId}")
            {
                if (Guid.TryParse(userId, out Guid parsedUserId))
                {
                    user = await _userManager.FindByIdAsync(parsedUserId.ToString());
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Format userId yang dikirimkan tidak valid. Pastikan Anda memasukkan string GUID user yang benar."
                    });
                }
            }

            if (user == null)
            {
                var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(tokenUserId))
                {
                    user = await _userManager.FindByIdAsync(tokenUserId);
                }
            }

            if (user == null)
            {
                user = await _userManager.Users.FirstOrDefaultAsync();
            }

            if (user == null)
                return NotFound(new { message = "Belum ada data user di dalam database." });

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                password = user.PasswordHash,
                role = user.Role.ToString(), 
                user.IsActive,
                user.CreatedAt,
                lastLoginAt = DateTime.UtcNow
            });
        }

        [HttpPut("update-profile/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateProfile(string userId, [FromBody] UpdateProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId) || userId == "{userId}")
            {
                return BadRequest(new
                {
                    message = "Gagal memproses request. Kolom parameter 'userId' di bagian atas/URL Scalar wajib diisi terlebih dahulu dengan GUID user Anda!"
                });
            }

            if (!Guid.TryParse(userId, out Guid parsedUserId))
            {
                return BadRequest(new
                {
                    message = "Format userId yang dikirimkan tidak valid. Pastikan Anda memasukkan string GUID user yang benar."
                });
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(parsedUserId.ToString());
            if (user == null)
                return NotFound(new { message = $"User dengan ID {parsedUserId} tidak ditemukan." });

            if (user.Email != dto.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(dto.Email);
                if (emailExists != null)
                {
                    return BadRequest(new { message = "Email sudah digunakan oleh pengguna lain." });
                }

                user.Email = dto.Email;
                user.UserName = dto.Email;
            }

            user.Name = dto.Name;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);

                if (removePasswordResult.Succeeded || removePasswordResult.Errors.Any(e => e.Code != "UserLoginAlreadyHasPassword"))
                {
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
                    if (!addPasswordResult.Succeeded)
                    {
                        return BadRequest(new
                        {
                            message = "Profil dasar berhasil diperbarui, namun password gagal diubah karena tidak memenuhi standar keamanan.",
                            errors = addPasswordResult.Errors.Select(e => e.Description)
                        });
                    }
                }
            }

            return Ok(new
            {
                message = "Profil dan password berhasil diperbarui tanpa token keamanan.",
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    role = user.Role.ToString()
                }
            });
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:Key"] ?? "HelpDeskSuperSecretKey2026!@#$%^&*()DefaultKey123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, user.Name),
                new("role", user.Role.ToString())
            };
            foreach (var role in roles) claims.Add(new(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "HelpDeskAPI",
                audience: _config["Jwt:Audience"] ?? "HelpDeskClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Nama wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format penulisan email salah.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Password minimal {2} karakter.", MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}