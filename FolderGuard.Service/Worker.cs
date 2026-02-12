using System.Threading.Channels;
using FolderGuard.Service.Services;

namespace FolderGuard.Service;

public class Worker : BackgroundService
{
	private readonly ILogger<Worker> _logger;
	private readonly Channel<string> _fileQueue = Channel.CreateUnbounded<string>();
	private FileSystemWatcher? _watcher;
	private readonly EncryptionOptions _options;
	private readonly IEncryptionEngine _engine;

	public Worker(ILogger<Worker> logger, EncryptionOptions options, IEncryptionEngine engine)
	{
		_logger = logger;
		_options = options;
		_engine = engine;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var watchPath = _options.PathToWatch;
		var outputDir = _options.OutputDir;
		var maxParallelism = _options.MaxParallelism;

		Directory.CreateDirectory(watchPath);
		Directory.CreateDirectory(outputDir);

		// Create wacher to monitor recursively
		_watcher = new FileSystemWatcher(watchPath)
		{
			IncludeSubdirectories = true,
			EnableRaisingEvents = true
		};

		_watcher.Created += (s, e) => 
		{
			if (!Directory.Exists(e.FullPath))
			{
				// Add created file to be queued for encryption
				_fileQueue.Writer.TryWrite(e.FullPath);
			}
		};

		_logger.LogInformation("Watching: {in} | Outputting to: {out}", watchPath, outputDir);

		var consumers = new List<Task>();
		// Add task to list of tasks
		for (int i = 0; i < maxParallelism; i++)
		{
			consumers.Add(ConsumeQueueAsync(i, stoppingToken));
		}

		await Task.WhenAll(consumers);
	}

	private async Task ConsumeQueueAsync(int workerId, CancellationToken stoppingToken)
	{
		// Keep consuming until the channel is marked as complete
		await foreach (var filePath in _fileQueue.Reader.ReadAllAsync(stoppingToken))
		{
			try
			{
				if (await WaitForFileReadyAsync(filePath, stoppingToken))
				{
					await ProcessFileAsync(filePath, stoppingToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Worker {id} error processing {file}", workerId, filePath);
			}
		}
	}


	private async Task ProcessFileAsync(string inputPath, CancellationToken ct)
	{
		string relativePath = Path.GetRelativePath(_options.PathToWatch, inputPath);
		string outputPath = Path.Combine(_options.OutputDir, relativePath + ".enc");

		string? outputSubDir = Path.GetDirectoryName(outputPath);

		// Recreate the source subfolder structure in the output directory
		if (!string.IsNullOrEmpty(outputSubDir))
		{
			Directory.CreateDirectory(outputSubDir);
		}

		// Use the injected engine instead of local Process.Start logic
		bool success = await _engine.EncryptAsync(inputPath, outputPath, ct);

		if (success)
		{
			_logger.LogInformation("Processed: {relPath}", relativePath);
			// Remove source file only after successful encryption to prevent data loss
			File.Delete(inputPath);
			// TODO: If directory structure is empty, remove empty directories in source location
		}
		else
		{
			_logger.LogError("Encryption engine failed for {file}", inputPath);
		}
	}

	private static async Task<bool> WaitForFileReadyAsync(string path, CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			try
			{
				if (!File.Exists(path)) return false;
				using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
				return true;
			}
			catch (IOException)
			{
				// File might be locked by the OS during copy, retry until accessible
				await Task.Delay(1000, token);
			}
		}
		return false;
	}
}
