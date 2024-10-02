using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;


namespace Azure_FileExplorerApp.Data;

public class FileExplorerDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        // створюємо конфігурацію, щоб зчитати налаштування з appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // налаштовуємо DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        // вказуємо рядок підключення з конфігурації
        var connectionString = configuration.GetConnectionString("Local");

        optionsBuilder.UseSqlServer(connectionString);

        // повертаємо новий екземпляр DataContext з налаштованим підключенням
        return new DataContext(optionsBuilder.Options);
    }
}

