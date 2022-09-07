namespace NosTale_ClientLess_NET.Packet.Encrypt_Decrypt;

public interface IPacketDecryptor
{
    string DecryptLoginPacket(byte[] bytes, int size);

    List<string> DecryptWorldPacket(byte[] bytes, int size);
}