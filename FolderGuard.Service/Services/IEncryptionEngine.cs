namespace FolderGuard.Service.Services;

public interface IEncryptionEngine
{
    Task<bool> EncryptAsync(string inputPath, string outputPath, CancellationToken ct);
}
