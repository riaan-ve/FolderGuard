public class EncryptRequest
{
    public string Action { get; set; } = string.Empty;
    public byte[] Password { get; set; } = Array.Empty<byte>();
    public string FileIn { get; set; } = string.Empty;
    public string FileOut { get; set; } = string.Empty;
    public int Iterations { get; set; } = 150000;
}
