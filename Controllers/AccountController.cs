using AutoMapper;
using Azure;
using Engineering_Hub.DTO.LoginDTOs;
using Engineering_Hub.models;
using Engineering_Hub.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            GenericRepository<RefreshToken> repo,
            IMapper mapper,
    IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _repo = repo;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }
        [HttpPost]
        public async Task<IActionResult> Login(logindto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null) { return Unauthorized(); }

            var password = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!password) { return Unauthorized(); }

            var roles = await _userManager.GetRolesAsync(user);

            List<Claim> userdata = new List<Claim>();
            userdata.Add(new Claim(ClaimTypes.Name, dto.UserName));
            userdata.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
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
    }
}