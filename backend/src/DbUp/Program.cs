// See https://aka.ms/new-console-template for more information
using System.Reflection;
using System.Text.RegularExpressions;
using MechanicBuddy.Core.Application.Configuration;
using DbUp;
using Microsoft.Extensions.Configuration;
 
var connectionString = AppConfiguration.PostgreSqlConnectionString;

var upgrader =
       DeployChanges.To
           .PostgresqlDatabase(connectionString)
               // Handle SQL scripts
               .WithScriptsAndCodeEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    scriptPath => scriptPath.EndsWith(".cs")) // Only process .cs files this way
                                                              // Use this for SQL scripts
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    script => script.EndsWith(".sql"))
            // Add custom script sorter that works for both types
            .WithScriptNameComparer(new CustomScriptComparer())
           .LogToConsole()
           .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
#if DEBUG
    Console.ReadLine();
#endif
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Success!");
Console.ResetColor();
return 0;

public class CustomScriptComparer : IComparer<string>
{
    
    public int Compare(string? x, string? y)
    {
        if (x == null) x = string.Empty;
        if (y == null) y = string.Empty;
        // Extract script numbers from both filenames
        var xMatch = Regex.Match(x, @"Script(\d+)_");
        var yMatch = Regex.Match(y, @"Script(\d+)_");

        if (xMatch.Success && yMatch.Success)
        {
            int xNum = int.Parse(xMatch.Groups[1].Value);
            int yNum = int.Parse(yMatch.Groups[1].Value);
            return xNum.CompareTo(yNum);
        }

        // Fall back to string comparison if pattern doesn't match
        return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }
}
public class AppConfiguration 
{
    static IConfigurationRoot Configuration { get; }
    static AppConfiguration()
    {
        Configuration = LoadConfiguration();
    }

    public static string PostgreSqlConnectionString 
    {
        get
        { 
            var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder();
            var options = new DbOptions(); Configuration.GetSection("DbOptions").Bind(options);
            connectionBuilder.Host = options.Host;
            connectionBuilder.Port = options.Port;
            connectionBuilder.Username = options.UserId;
            connectionBuilder.Password = options.Password;
            connectionBuilder.Database = options.Name;
            return connectionBuilder.ToString();
        }
    }
    private static IConfigurationRoot LoadConfiguration()
    { 

        var builder = new ConfigurationBuilder();
        builder.SetBasePath(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

#if DEBUG
        builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
#endif

#if RELEASE
builder.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);
#endif

        builder.AddJsonFile("appsettings.Secrets.json", optional: false, reloadOnChange: true);


        IConfigurationRoot configuration = builder.Build();
        return configuration;
    }
}

