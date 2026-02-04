public class EncryptionOptions
{
    public byte[] Key { get; set; } = Array.Empty<byte>();
    public string PathToWatch { get; set; } = string.Empty;
    public string OutputDir { get; set; } = string.Empty;
    public string AppPath { get; set; } = string.Empty;
    public int MaxParallelism { get; set; } = 4;
}
