### Asp.NET Core 3.X WebApi JWT/Token Authentication with Identity

In this tutorial I configured **Identity** with **.net core WebApi** and expose **JWT token** for authentication, here I also manage **roles** in the token.

Here is the step by step process to configure it, you can follow these steps for clear understanding or you can run my GitHub project.

#### Step-1

Create a blank WebApi application with no authentication. We will add authentication manually.

![Create Project](https://i.imgur.com/6JtNpcN.png)

#### Step-2

Add required packages using nuget package manager.

```bash
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.IdentityModel.Tokens
System.IdentityModel.Tokens.Jwt
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.EntityFrameworkCore.Tools
```

#### Step-3

Prepare project structure and add some configuration

> Create directory **Data**, **Dtos** & **Utility** into your project.

Add this configuration in your `appsettings.json`

```bash
  "JwtKey": "nB4I_WnyEXLdsTXExXvaBhWGibOSsYT8bHqaUj_N7kD_XgLM5f2", //SOME RANDOM KEY DO NOT SHARE
  "JwtIssuer": "https://arif2009.github.io", //Your domain
  "JwtExpireDays": 1,
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=DbApi;Trusted_Connection=True;"
  },
```

#### Step-4

Add `Enums.cs` to Utility folder and `SeedData.cs` & `DataContext.cs` to your Data folder.

> Enums.cs

```bash
public static class Enums
{
    public enum Roles
    {
        SuperAdmin = 1,
        Admin = 2,
        User = 3
    }
}
```

> SeedData.cs

```bash
public static class SeedData
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = ((long)Roles.SuperAdmin).ToString(), Name = Roles.SuperAdmin.ToString(), NormalizedName = Roles.SuperAdmin.ToString().ToUpper(), ConcurrencyStamp = "88f0dec2-5364-4881-9817-1f2a135a8649" },
            new IdentityRole { Id = ((long)Roles.Admin).ToString(), Name = Roles.Admin.ToString(), NormalizedName = Roles.Admin.ToString().ToUpper(), ConcurrencyStamp = "5719c2b8-22fd-4eee-9c21-4bfbd2ce18d7" },
            new IdentityRole { Id = ((long)Roles.User).ToString(), Name = Roles.User.ToString(), NormalizedName = Roles.User.ToString().ToUpper(), ConcurrencyStamp = "eccc7115-422c-487d-95b0-58cfa8e66a94" }
        );
    }
}
```

> DataContext.cs

```bash
public class DataContext: IdentityDbContext<IdentityUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Seed();
    }
}
```

This basically extends **IdentityDbContext**, so that the identity table will add our database when we add migration.

> Configure DataContext in `Startup.cs` file

```bash
// ===== Add our DbContext ========
services.AddDbContext<DataContext>(x => x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

// ===== Add Identity ========
services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders();

// ===== Add Identity Options========
services.Configure<IdentityOptions>(opt => {
    opt.Password.RequiredLength = 6;
    opt.Password.RequireDigit = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;

    opt.User.RequireUniqueEmail = true;
});
```

#### Step-5

Now execute command `Add-Migration` and `Update-Database` in package manager console
![Package manager console](https://i.imgur.com/ariBhmG.png)
After execute successfully you can see your table structure like this
![identity table](https://i.imgur.com/gzloFF0.png)

#### Step-6

Now we have done our Identity setup. Let’s configure JWT authentication.

> In the `Startup.cs` file add this code add this code in `ConfigureServices()`

```bash
// ===== Add Jwt Authentication ========
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(cfg =>
{
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidIssuer = Configuration["JwtIssuer"],
        ValidAudience = Configuration["JwtIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
    };
});
```

> In the `Configure()` methode add this code

```bash
app.UseAuthentication();
app.UseAuthorization();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
```

#### Step-7

Add `LoginDto.cs` & `RegisterDto.cs` in Dtos folder and `AuthController.cs` in Controller

> LoginDto.cs

```bash
public class LoginDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
```

> RegisterDto.cs

```bash
public class RegisterDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
    public string Password { get; set; }
}
```

> AuthController.cs

```bash
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

        if (!result.Succeeded)
            return Unauthorized();

        var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);

        var token = await GenerateJwtToken(appUser);

        return Ok(new { token, appUser.UserName });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Roles.Admin.ToString());

            return Ok( new { user.Email, user.UserName });
        }

        return BadRequest("Username already exists");
    }

    private async Task<string> GenerateJwtToken(IdentityUser user)
    {

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach(var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }



        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

        var token = new JwtSecurityToken(
            _configuration["JwtIssuer"],
            _configuration["JwtIssuer"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### Step-8
