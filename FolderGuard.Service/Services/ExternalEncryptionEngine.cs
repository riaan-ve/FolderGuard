namespace FolderGuard.Service.Services;

using System.Diagnostics;
using System.Text.Json;

public class ExternalEncryptionEngine : IEncryptionEngine
{
    private readonly EncryptionOptions _options;
    private readonly ILogger<ExternalEncryptionEngine> _logger;

    public ExternalEncryptionEngine(EncryptionOptions options, ILogger<ExternalEncryptionEngine> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<bool> EncryptAsync(string inputPath, string outputPath, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _options.AppPath,
            UseShellExecute = false,
			// Redirect stdin to pass JSON without exposing data
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null) return false;

        try
        {
			// Define instructions for the external tool
            var payload = new
            {
                Action = "encrypt",
                Password = _options.Key, 
                FileIn = inputPath,
                FileOut = outputPath,
                Iterations = 150000
            };

            // Write the instructions to the sub-process
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(payload));
            process.StandardInput.Close();

            // Wait until the process finishes 
            await process.WaitForExitAsync(ct);
            
            return process.ExitCode == 0;
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill();
            _logger.LogWarning("Encryption cancelled for {file} due to service shutdown.", inputPath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during encryption of {file}", inputPath);
            return false;
        }
    }
}
