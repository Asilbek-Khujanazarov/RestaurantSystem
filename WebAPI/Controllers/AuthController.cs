using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RestaurantManagementSystem.infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RestaurantDbContext _context;
    private readonly IConfiguration _configuration;
    public AuthController(RestaurantDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    // [Authorize(Roles = "GeniralSupperStaff")]
    public async Task<IActionResult> Register(Staff staff)
    {
        staff.CustomId = int.Parse(Generate4DigitId());

        _context.Staffs.Add(staff);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Foydalanuvchi muvaffaqiyatli ro'yxatdan o'tdi" });
    }

    private string Generate4DigitId()
    {
        Random random = new Random();
        int newId;
        do
        {
            newId = random.Next(1000, 10000);
        }
        while (_context.Staffs.Any(s => s.CustomId == newId));

        return newId.ToString();
    }

    [HttpPost("[Action]")]
    public async Task<IActionResult> AttendanceStaffs([FromBody] AttendanceRequest attendanceRequest)
    {
        var staffCutomId = _context.Staffs.FirstOrDefault(o => o.CustomId == attendanceRequest.AttendanceStaffId);
        if (staffCutomId == null)
        {
            return NotFound("Xotim Id-si topilmadi");
        }
        var present = new Attendance
        {
            CustomID = attendanceRequest.AttendanceStaffId,
            AttendanceTime = DateTime.Now
        };
        var today = DateTime.Today;
        var count = await _context.Attendances
            .Where(a => a.CustomID == attendanceRequest.AttendanceStaffId && a.AttendanceTime.Date == today)
            .CountAsync();

        if (count % 2 == 0)
        {
            _context.Attendances.Add(present);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xush kelibsiz ✅" });
        }
        else
        {
            _context.Attendances.Add(present);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xayir Ko'rishguncha ✅" });
        }
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var existingUser = await _context.Staffs
        .FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.PhoneNumber && u.Password == loginDto.Password);

        if (existingUser == null)
        {
            return Unauthorized(new { Message = "Foydalanuvchi topilmadi yoki parol noto'g'ri!" });
        }

        var token = GenerateJwtToken(existingUser);
        return Ok(new { Token = token, isSuperStaff = existingUser.IsSuperStaff });

    }
    [HttpPost("[Action]")]
    [Authorize(Roles = "GeniralSupperStaff")]
    public async Task<IActionResult> IsResultStaff(int id, bool? PutPresent)
    {
        var existingUser = _context.Staffs.FirstOrDefault(o => o.Id == id);
        if (existingUser == null)
        {
            return NotFound("Xotim topilmadi");
        }
        existingUser.IsSuperStaff = PutPresent;
        await _context.SaveChangesAsync();
        return Ok(existingUser);

    }
    [HttpPost("[Action]")]
    [Authorize(Roles = "GeniralSupperStaff")]
    public async Task<ActionResult<string>> GetWorkDuration([FromBody] GetWorkDurationRequest request)
    {

        var fromDate = request.FromTheDay.Date;
        var untilDate = request.UntilTheDay.Date.AddDays(1).AddSeconds(-1);

        var attendances = await _context.Attendances
            .Where(a => a.CustomID == request.CustomId &&
                        a.AttendanceTime >= fromDate &&
                        a.AttendanceTime <= untilDate)
            .OrderBy(a => a.AttendanceTime)
            .ToListAsync();

        if (attendances.Count == 0)
        {
            return NotFound("Xodimning ma'lumotlari topilmadi.");
        }

        int totalMinutes = 0;

        for (int i = 0; i < attendances.Count - 1; i += 2)
        {
            var entry = attendances[i];
            var exit = attendances[i + 1];

            totalMinutes += (int)(exit.AttendanceTime - entry.AttendanceTime).TotalMinutes;
        }

        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        return Ok($"{hours} soat, {minutes} daqiqa");
    }


    [HttpGet("[Action]")]
    [Authorize(Roles = "GeniralSupperStaff")]
    public IActionResult SeachStaff(int id)
    {
        var staff = _context.Staffs.FirstOrDefault(o => o.CustomId == id);
        if (staff == null)
        {
            return NotFound("Topilmadi");
        }

        return Ok(staff);
    }

    [HttpDelete("deleteStaff")]
    [Authorize(Roles = "GeniralSupperStaff")]
    public ActionResult DeleteStaff(int id)
    {
        var staffDel = _context.Staffs.FirstOrDefault(m => m.Id == id);

        if (staffDel == null)
        {
            return NotFound("Staff topilmadi");
        }

        _context.Staffs.Remove(staffDel);

        var result = _context.SaveChanges();

        return NoContent();
    }



    private string GenerateJwtToken(Staff staff)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, staff.FirstName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString())
    };

        if (staff.IsSuperStaff == null)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Staff"));
        }
        if (staff.IsSuperStaff == false)
        {
            claims.Add(new Claim(ClaimTypes.Role, "GeniralStaff"));
        }
        if (staff.IsSuperStaff == true)
        {
            claims.Add(new Claim(ClaimTypes.Role, "GeniralSupperStaff"));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}