using Azure.Identity;
using Azure_FileExplorerApp;
using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Services;
using AzureTeacherStudentSystem;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

//SQL
//var sqlConnectionString = builder.Configuration["AzureFileExplorerSqlConnectionString"];
//builder.Services.AddDbContext<DataContext>(opt =>
//    opt.UseSqlServer(sqlConnectionString));

builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Local")));

// Blob Storage
var storageConnectionString = builder.Configuration["StorageAccountConnectionString07"];
builder.Services.AddSingleton<BlobService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<BlobService>>();
    return new BlobService(storageConnectionString, logger);
});

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageAccountConnectionString07"]!);
});

// сервіси
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IFolderService, FolderService>();

// кешування
builder.Services.AddSingleton(_ => ConnectionMultiplexer.Connect(builder.Configuration["Redis"]).GetDatabase());
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Identity для керування користувачами та ролями
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // тривалість сесії
    options.SlidingExpiration = false; // кукі не продовжуються автоматично
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = null; // кукі видаляється після закриття браузера

    options.LoginPath = "/Account/Login";   // шлях для сторінки входу
    options.LogoutPath = "/Account/Logout"; // шлях для сторінки виходу
    options.AccessDeniedPath = "/Account/AccessDenied"; // шлях для сторінки доступу відхилено
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Створення адміністратора і ролей під час запуску програми
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // отримання даних адміна з Key Vault
    var adminEmail = builder.Configuration["AdminEmail"];
    var adminPassword = builder.Configuration["AdminPassword"];

    await CreateRolesAndAdminAsync(roleManager, userManager, adminEmail, adminPassword);
}

app.Run();



// Метод для створення ролей та адміністратора
async Task CreateRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, string adminEmail, string adminPassword)
{
    string[] roleNames = { "Admin", "User", "Manager" };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail
        };

        var adminCreated = await userManager.CreateAsync(newAdmin, adminPassword);
        if (adminCreated.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}
