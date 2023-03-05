using Microsoft.Extensions.Configuration;

namespace EtherScanDemo;

public class Program
{
    public static void Main(string[] args)
    {
        SetEnvironmentVariables();
        var blkchi = new BlockChainInfo();
        Task.Run(async () =>
        {
            await blkchi.ScanBlockHandling(12100001, 12100500);
        })
        .Wait();
    }

    static void SetEnvironmentVariables()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        foreach (var child in config.GetChildren())
        {
            Environment.SetEnvironmentVariable(child.Key, child.Value);
        }
    }
}