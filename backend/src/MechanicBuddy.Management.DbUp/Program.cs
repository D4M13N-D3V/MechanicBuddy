using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Helpers;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Management")
    ?? throw new InvalidOperationException("ConnectionStrings__Management environment variable not set");

Console.WriteLine("Starting MechanicBuddy Management database migrations...");

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .WithVariablesDisabled()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Management database migrations completed successfully!");
Console.ResetColor();
return 0;
