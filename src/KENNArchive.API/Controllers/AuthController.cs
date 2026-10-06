using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KENNArchive.Application.DTOs.Auth;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using KENNArchive.Infrastructure.Data;

namespace KENNArchive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly KennArchiveDbContext _dbContext;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IEmailService emailService,
        KennArchiveDbContext dbContext)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest("Bu email artıq qeydiyyatdan keçib.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsEmailVerified = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        var code = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var verificationCode = new EmailVerificationCode
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        _dbContext.EmailVerificationCodes.Add(verificationCode);

        await _dbContext.SaveChangesAsync();

        await _emailService.SendVerificationCodeAsync(
            user.Email!,
            code);

        return Ok(
            "Qeydiyyat yaradıldı. Email ünvanınıza təsdiq kodu göndərildi.");
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        VerifyEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return NotFound("İstifadəçi tapılmadı.");
        }

        if (user.IsEmailVerified)
        {
            return BadRequest("Email artıq təsdiqlənib.");
        }

        var verificationCode = await _dbContext.EmailVerificationCodes
            .Where(x =>
                x.UserId == user.Id &&
                x.Code == request.Code &&
                !x.IsUsed)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (verificationCode == null)
        {
            return BadRequest("Təsdiq kodu yanlışdır.");
        }

        if (verificationCode.ExpiresAt < DateTime.UtcNow)
        {
            return BadRequest("Təsdiq kodunun vaxtı bitib.");
        }

        verificationCode.IsUsed = true;
        user.IsEmailVerified = true;
        user.EmailConfirmed = true;

        await _dbContext.SaveChangesAsync();

        return Ok("Email uğurla təsdiqləndi.");
    }

    [HttpPost("resend-verification-code")]
    public async Task<IActionResult> ResendVerificationCode(
        ResendVerificationCodeRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return NotFound("İstifadəçi tapılmadı.");
        }

        if (user.IsEmailVerified)
        {
            return BadRequest("Email artıq təsdiqlənib.");
        }

        var code = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var verificationCode = new EmailVerificationCode
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        _dbContext.EmailVerificationCodes.Add(verificationCode);

        await _dbContext.SaveChangesAsync();

        await _emailService.SendVerificationCodeAsync(
            user.Email!,
            code);

        return Ok(
            "Yeni təsdiq kodu email ünvanınıza göndərildi.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized("Email və ya şifrə yanlışdır.");
        }

        if (!user.IsEmailVerified)
        {
            return Unauthorized(
                "Login üçün əvvəlcə email ünvanınızı təsdiqləyin.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordValid)
        {
            return Unauthorized("Email və ya şifrə yanlışdır.");
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Email,
                user.Email!),

            new Claim(
                ClaimTypes.Name,
                user.UserName!)
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(
                    _configuration["Jwt:ExpireMinutes"]!)),
            signingCredentials: credentials);

        var tokenString =
            new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            message = "Login uğurludur.",
            token = tokenString
        });
    }
}