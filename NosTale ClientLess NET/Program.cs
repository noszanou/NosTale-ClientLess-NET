using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NosTale_ClientLess_NET.Packet.Bullshit;
using NosTale_ClientLess_NET.Packet.Encrypt_Decrypt;

namespace NosTale_ClientLess_NET;

public class Program
{
    public static TcpClientSession? _client;

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddSingleton<IPacketEncryptor, PacketEncryptor>()
            .AddSingleton<IPacketDecryptor, PacketDecryptor>()
            .AddSingleton<TcpClientSession>();
        return services.BuildServiceProvider();
    }

    public async Task MainAsync()
    {
        string id = Environment.GetEnvironmentVariable("ID") ?? string.Empty;
        string password = Environment.GetEnvironmentVariable("PASSWORD") ?? string.Empty;
        byte slot = Convert.ToByte(Environment.GetEnvironmentVariable("SLOT"));
        short loginPort = Convert.ToInt16(Environment.GetEnvironmentVariable("LOGIN_PORT"));
        short worldPort = Convert.ToInt16(Environment.GetEnvironmentVariable("WORLD_PORT"));
        string serverAdress = Environment.GetEnvironmentVariable("SERVER_IP") ?? "127.0.0.1";

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Specify an id or pw to login");
            Console.ReadLine();
            return;
        }

        if (loginPort == 0 || worldPort == 0)
        {
            Console.WriteLine("Specify Login and world port");
            Console.ReadLine();
            return;
        }

        await using (var services = ConfigureServices())
        {
            var user = services.GetRequiredService<TcpClientSession>();
            while (user.EncryptionKey == 0)
            {
                await user.ConnectToIpPortAndBeginStreamReader(serverAdress, loginPort);
                await user.SendLoginCredential(id, password);
                await Task.Delay(5000);
            }
            user.Disconnect();
            await user.ConnectToIpPortAndBeginStreamReader(serverAdress, worldPort, false);
            await user.WriteAndFlushPacket($"{user.EncryptionKey}", true, waitMicroSec: 250);
            await user.WriteAndFlushPacket($"{id} ORG 0", waitMicroSec: 250);
            await user.WriteAndFlushPacket($"{password}", waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateCClose(0), waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateFStashEnd(), waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateCClose(1), waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateSelect(slot), waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateGameStart(), waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateLbs(0), waitMicroSec: 250);
            // why there fiew c_close and f_stash_end in official when you start ??
            await user.WriteAndFlushPacket(Lmao.GenerateCClose(0), waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateBpClose(), waitMicroSec: 250);
            await user.WriteAndFlushPacket(Lmao.GenerateFStashEnd(), waitMicroSec: 50);
            await user.WriteAndFlushPacket(Lmao.GenerateCClose(1), waitMicroSec: 50);
            await user.WriteAndFlushPacket(Lmao.GenerateCClose(0), waitMicroSec: 50);
            await user.WriteAndFlushPacket(Lmao.GenerateFStashEnd(), waitMicroSec: 50);
            await user.WriteAndFlushPacket(Lmao.GenerateCClose(1), waitMicroSec: 50);
            await user.WriteAndFlushPacket($"glist 0 0", waitMicroSec: 50);
            await user.WriteAndFlushPacket(Lmao.GenerateNpInfo(0), waitMicroSec: 50);

            _client = user;
        }
    }

    public static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        await new Program().MainAsync();
    }
}