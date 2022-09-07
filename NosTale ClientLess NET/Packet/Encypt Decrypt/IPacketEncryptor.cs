namespace NosTale_ClientLess_NET.Packet.Encrypt_Decrypt;

public interface IPacketEncryptor
{
    int EncryptionKey { get; set; }

    byte[] EncryptLoginPacket(string value);

    byte[] EncryptWorldPacket(string value, bool session = false);
}