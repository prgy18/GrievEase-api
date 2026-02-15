using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using GrievEase.API.Models.Entities;
using GrievEase.API.Constants;

namespace GrievEase.API.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _configuration;   

    public JwtHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Generate JWT token for a user
    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

        // Claims = data stored inside the token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.SignInType.ToString()),
            new Claim("tokenVersion", user.TokenVersion.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["ExpiryMinutes"]!)
            ),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Get UserId from token
    public Guid GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier
                              || c.Type == "nameid");

        if (userIdClaim == null)
            throw new Exception("Invalid token - UserId not found");

        return Guid.Parse(userIdClaim.Value);
    }

    // Get token expiry datetime
    public DateTime GetTokenExpiry(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        return jwtToken.ValidTo;
    }

    // Get TokenVersion from token
    public int GetTokenVersion(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var versionClaim = jwtToken.Claims
            .FirstOrDefault(c => c.Type == "tokenVersion");

        if (versionClaim == null)
            throw new Exception("Invalid token - TokenVersion not found");

        return int.Parse(versionClaim.Value);
    }
}