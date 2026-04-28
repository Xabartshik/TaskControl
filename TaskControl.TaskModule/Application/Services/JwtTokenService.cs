using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskControl.TaskModule.Domain; // Добавлен using для MobileUserRole

namespace TaskControl.TaskModule.Application.Services;

public interface IJwtTokenService
{
    // Теперь передаем все важные ID для Claims
    string CreateToken(int userId, MobileUserRole role, int? employeeId, int? customerId, int? branchId);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _cfg;

    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public string CreateToken(int userId, MobileUserRole role, int? employeeId, int? customerId, int? branchId)
    {
        var jwt = _cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // NameIdentifier теперь всегда ID записи в mobile_app_users (Account ID)
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            
            // Роль (Worker, Admin, Customer и т.д.)
            new Claim(ClaimTypes.Role, role.ToString())
        };

        // Добавляем ID сотрудника, если он есть
        if (employeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", employeeId.Value.ToString()));
        }

        // Добавляем ID клиента, если он есть
        if (customerId.HasValue)
        {
            claims.Add(new Claim("CustomerId", customerId.Value.ToString()));
        }

        // Филиал (для сотрудников)
        if (branchId.HasValue)
        {
            claims.Add(new Claim("BranchId", branchId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"] ?? "1440")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}