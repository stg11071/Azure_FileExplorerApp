using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Azure_FileExplorerApp.Data;

public class FileExplorerDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        // Створюємо конфігурацію, щоб зчитати налаштування з appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Налаштовуємо DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        // Вказуємо рядок підключення з конфігурації
        var connectionString = configuration.GetConnectionString("MigrationConnection");

        optionsBuilder.UseSqlServer(connectionString);

        // Повертаємо новий екземпляр FileExplorerDbContext з налаштованим підключенням
        return new DataContext(optionsBuilder.Options);
    }
}

