using Azure.Identity;
using Azure_FileExplorerApp;
using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Services;
using AzureTeacherStudentSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

var sqlConnectionString = builder.Configuration["AzureFileExplorerSqlConnectionString"];
builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseSqlServer(sqlConnectionString));

//builder.Services.AddDbContext<DataContext>(opt =>
//    opt.UseSqlServer(builder.Configuration.GetConnectionString("Local")));


var storageConnectionString = builder.Configuration["ConnectionStrings:Storage"];
builder.Services.AddSingleton<BlobService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<BlobService>>();
    return new BlobService(storageConnectionString, logger);
});

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["ConnectionStrings:Storage"]!);
    clientBuilder.AddQueueServiceClient(builder.Configuration["ConnectionStrings:Storage"]!);
});


builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IFolderService, FolderService>();

builder.Services.AddSingleton(_ => ConnectionMultiplexer.Connect(builder.Configuration["Redis"]).GetDatabase());
builder.Services.AddScoped<ICacheService, RedisCacheService>();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
