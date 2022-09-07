using NosTale_ClientLess_NET.Packet;
using NosTale_ClientLess_NET.Packet.Encrypt_Decrypt;
using System.Net.Sockets;
using System.Text;

namespace NosTale_ClientLess_NET;

public class TcpClientSession
{
    // Make it really random XD this was just for test
    private int _random = 53535;

    private TcpClient _client;
    private NetworkStream _serverStream;
    private readonly IPacketEncryptor _packetEncryptor;
    private readonly IPacketDecryptor _packetDecryptor;

    public TcpClientSession(IPacketEncryptor packetEncryptor, IPacketDecryptor packetDecryptor)
    {
        _packetEncryptor = packetEncryptor;
        _packetDecryptor = packetDecryptor;
    }

    public int EncryptionKey { get; set; }

    public async Task SendLoginCredential(string username, string password)
    {
        string passwordHash = Cryptography.ToSha512(password);
        var packet =
            $"NoS0575 7040266 " +
            $"{username} {passwordHash} " +
            $"NONE 008B4FB4 0{(char)0xB}0.9.3.3181 0 880F67ADFB0DC538FE20F80F5E46BA4F";
        await WriteAndFlushPacket(packet, isLogin: true);
    }

    public void Disconnect()
    {
        _serverStream?.Close();
        _client?.Close();
    }

    public async Task WriteAndFlushPacket(string packet, bool session = false, bool isLogin = false, int waitMicroSec = 0)
    {
        byte[] sendPacket;
        if (isLogin)
        {
            sendPacket = _packetEncryptor.EncryptLoginPacket(packet);
        }
        else
        {
            sendPacket = _packetEncryptor.EncryptWorldPacket($"{_random++} {packet}", session);
        }
        _serverStream?.Write(sendPacket, 0, sendPacket.Length);
        _serverStream?.Flush();
        if (waitMicroSec != 0)
            await Task.Delay(waitMicroSec);
    }

    public async Task ConnectToIpPortAndBeginStreamReader(string ip, int port, bool isLogin = true)
    {
        Disconnect();
        _client = new TcpClient { NoDelay = true };
        await _client.ConnectAsync(ip, port);
        _serverStream = _client.GetStream();
        if (isLogin)
        {
            new Thread(() => ReadLoginPacket()).Start();
            return;
        }
        new Thread(() => ReadWorldPacket()).Start();
    }

    private List<string> Decrypt(bool isWorld = true)
    {
        var list = new List<string>();
        try
        {
            byte[] recvpacket = new byte[(int)_client.ReceiveBufferSize + 1];
            var readDataLenght = _serverStream.Read(recvpacket, 0, (int)_client.ReceiveBufferSize);
            if (!isWorld)
            {
                list.Add(_packetDecryptor.DecryptLoginPacket(recvpacket, readDataLenght));
            }
            {
                list.AddRange(_packetDecryptor.DecryptWorldPacket(recvpacket, readDataLenght));
            }
        }
        catch { }
        return list;
    }

    private void ReadLoginPacket()
    {
        try
        {
            while (true)
            {
                StringBuilder Msg = new();
                do
                {
                    var packet = Decrypt(false).First();

                    var splitedArg = packet.Split(" ");
                    if (splitedArg[0] == "NsTeST")
                    {
                        EncryptionKey = Convert.ToInt32(splitedArg[76]);
                        _packetEncryptor.EncryptionKey = EncryptionKey;
                    }
                    if (!string.IsNullOrEmpty(packet))
                    {
                        Msg.AppendFormat("{0}", packet);
                    }
                }
                while (_serverStream.DataAvailable);

                if (Msg.Length > 0)
                {
                    Console.WriteLine("[LOGIN] Packet received from client : " +
                                                 Msg);
                }
            }
        }
        catch { }
    }

    private void ReadWorldPacket()
    {
        try
        {
            while (true)
            {
                var list = new List<string>();

                do
                {
                    var packet = Decrypt();
                    list = packet;
                }
                while (_serverStream.DataAvailable);

                foreach (var i in list)
                {
                    var splitedArg = i.Split(" ");
                    // Ntm
                    if (splitedArg[0] == "mv")
                    {
                        continue;
                    }
                    Console.WriteLine("[WORLD] Packet received from client : " + i);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}