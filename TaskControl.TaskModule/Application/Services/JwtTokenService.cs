using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskControl.TaskModule.Domain; // Добавлен using для MobileUserRole

namespace TaskControl.TaskModule.Application.Services;

public interface IJwtTokenService
{
    // Изменили тип role на MobileUserRole и название employeeId на profileId
    string CreateToken(int profileId, MobileUserRole role, int? branchId);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _cfg;

    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public string CreateToken(int profileId, MobileUserRole role, int? branchId)
    {
        var jwt = _cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Записываем ID профиля (Сотрудника или Клиента)
            new Claim(ClaimTypes.NameIdentifier, profileId.ToString()),
            
            // Преобразуем Enum в строку специально для Claims, так как ASP.NET Core 
            // ожидает строковые роли в атрибутах [Authorize(Roles = "Admin")]
            new Claim(ClaimTypes.Role, role.ToString())
        };

        if (branchId.HasValue)
        {
            claims.Add(new Claim("BranchId", branchId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["LifetimeMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}