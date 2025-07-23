using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonsterSirenDownloader.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test()
    {
        Assert.Pass();
    }

    [Test]
    public void TestPaths()
    {
        Console.WriteLine("=== Workdir ===");
        var pwd = Directory.GetCurrentDirectory();
        Console.WriteLine($"Current Directory: {pwd}");
        Console.WriteLine($"Combined: {Path.GetFullPath(Path.Combine(pwd, "./monster"))}");
    }

    [Test]
    public void DisplayEnvironments()
    {
        Console.WriteLine("=== .NET Runtime Information ===");
        Console.WriteLine($"Framework Description: {RuntimeInformation.FrameworkDescription}"); // .NET Runtime version
        Console.WriteLine($"Environment Version: {Environment.Version}"); // Environment version
        Console.WriteLine($"Runtime Identifier: {RuntimeInformation.RuntimeIdentifier}"); // Runtime identifier (platform info)
        Console.WriteLine($"OS Description: {RuntimeInformation.OSDescription}"); // OS information

        Console.WriteLine("\n=== Assembly Version Information ===");
        var assembly = Assembly.GetExecutingAssembly(); // Get current assembly
        Console.WriteLine($"Assembly Version: {assembly.GetName().Version}"); // Assembly version
        Console.WriteLine(
            $"Product Version: {assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Not specified"}"); // Product version (from AssemblyInformationalVersion)
        Console.WriteLine($"File Version: {assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Not specified"}"); // File version
        Console.WriteLine($"Product Name: {assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Not specified"}"); // Product name
        Console.WriteLine($"Company: {assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Not specified"}"); // Company

        Console.WriteLine("\n=== Additional Information ===");
        Console.WriteLine($"Debug Build: {assembly.GetCustomAttributes(typeof(DebuggableAttribute), false).Length > 0}"); // Check if running in debug mode
        Console.WriteLine($"Assembly Location: {assembly.Location}"); // Get location
    }
}