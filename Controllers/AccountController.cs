using AutoMapper;
using Azure;
using Engineering_Hub.DTO.AccountDTOs;
using Engineering_Hub.DTO.LoginDTOs;
using Engineering_Hub.models;
using Engineering_Hub.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly GenericRepository<RefreshToken> _repo;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IWebHostEnvironment _environment;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            GenericRepository<RefreshToken> repo,
            IMapper mapper, IWebHostEnvironment environment,
    IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _repo = repo;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _environment = environment;
        }
        [HttpPost]
        public async Task<IActionResult> Login(logindto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null) { return Unauthorized(); }

            var password = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!password) { return Unauthorized(); }
            if (!user.IsActive)
            {
                return Unauthorized("Your account is inactive.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            List<Claim> userdata = new List<Claim>();
            userdata.Add(new Claim(ClaimTypes.Name, dto.UserName));
            userdata.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));


            foreach (var role in roles)
            {
                userdata.Add(new Claim(ClaimTypes.Role, role));
            }
            ;

            string secertkey = _configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secertkey));

            var sigcer = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(15);

            var Token = new JwtSecurityToken(
                claims: userdata,
                expires: accessTokenExpiration,
                signingCredentials: sigcer
            );

            var stringtoken =
                new JwtSecurityTokenHandler().WriteToken(Token);


            var refreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
            );


            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            _repo.add(refreshTokenEntity);

            await _repo.SaveAsync();


            Response.Cookies.Append("jwt", stringtoken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = new DateTimeOffset(accessTokenExpiration)
            });


            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                message = "Login successful"
            });

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(Registerdto dto)
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
            {
                return BadRequest("Role does not exist.");
            }


            var user = _mapper.Map<ApplicationUser>(dto);

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok("User registered successfully.");
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("Refresh token not found.");
            }

            var storedToken = _repo
                .GetByCondition(x => x.Token == refreshToken)
                .FirstOrDefault();

            if (storedToken == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            if (storedToken.IsRevoked)
            {
                return Unauthorized("Refresh token has been revoked.");
            }

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized("Refresh token has expired.");
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (!user.IsActive)
            {
                return Unauthorized("Your account is inactive.");
            }

            // Revoke the old refresh token
            storedToken.IsRevoked = true;
            _repo.Edit(storedToken);

            // Create a new refresh token
            var newRefreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
            );

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            _repo.add(newRefreshTokenEntity);

            // Create a new access token
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.Id)
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _configuration["Jwt:Key"];

            var key = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(secretKey)
            );

            var signingCredentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(15);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: accessTokenExpiration,
                signingCredentials: signingCredentials
            );

            var newAccessToken =
                new JwtSecurityTokenHandler().WriteToken(token);

            // Save both the revoked old token and new token
            await _repo.SaveAsync();

            // Replace access-token cookie
            Response.Cookies.Append("jwt", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = new DateTimeOffset(accessTokenExpiration)
            });

            // Replace refresh-token cookie
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                message = "Access token refreshed successfully."
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var storedToken = _repo
                    .GetByCondition(x => x.Token == refreshToken)
                    .FirstOrDefault();

                if (storedToken != null && !storedToken.IsRevoked)
                {
                    storedToken.IsRevoked = true;
                    _repo.Edit(storedToken);

                    await _repo.SaveAsync();
                }
            }

            Response.Cookies.Delete("jwt", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return Ok(new
            {
                message = "Logout successful."
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                fullName = user.FullName,
                roles = roles,
                profileImageUrl = user.ProfileImageUrl,
            });
        }

        [HttpPut("profile")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile(
     [FromForm] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.UserName))
                return BadRequest(new { message = "Username is required." });

            var trimmedUserName = dto.UserName.Trim();

            var existingUser = await _userManager.FindByNameAsync(trimmedUserName);

            if (existingUser != null && existingUser.Id != user.Id)
                return BadRequest(new { message = "This username is already taken." });

            string? newImagePath = null;

            if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024;

                if (dto.ProfileImage.Length > maxFileSize)
                    return BadRequest(new { message = "The image must be 5 MB or smaller." });

                var extension = Path.GetExtension(dto.ProfileImage.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new
                    {
                        message = "Only JPG, PNG, and WebP images are allowed."
                    });

                var webRoot = _environment.WebRootPath
                    ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

                var uploadsFolder = Path.Combine(webRoot, "uploads", "profiles");

                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfileImage.CopyToAsync(stream);
                }

                newImagePath = $"/uploads/profiles/{fileName}";
            }

            var usernameResult = await _userManager.SetUserNameAsync(
                user, trimmedUserName);

            if (!usernameResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(" ",
                        usernameResult.Errors.Select(e => e.Description))
                });
            }

            if (newImagePath != null)
                user.ProfileImageUrl = newImagePath;
            if (string.IsNullOrWhiteSpace(dto.FullName))
            {
                return BadRequest(new { message = "Full name is required." });
            }

            user.FullName = dto.FullName.Trim();
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(" ",
                        updateResult.Errors.Select(e => e.Description))
                });
            }

            return Ok(new
            {
                userName = user.UserName,
                fullName = user.FullName,
                profileImageUrl = user.ProfileImageUrl
            });
        }

    }
}