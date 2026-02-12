using System.Text.Json;

class Program
{
	static async Task<int> Main()
	{
		try
		{
			// Read the entire JSON payload piped from the parent process
			string inputJson = await Console.In.ReadToEndAsync();
			if (string.IsNullOrWhiteSpace(inputJson)) return 1;

			var request = JsonSerializer.Deserialize<EncryptRequest>(inputJson);
			if (request == null) return 1;

			switch (request.Action.ToLower())
			{
				case "encrypt":
				case "-e":
					Console.WriteLine($"Encrypting {request.FileIn}...");
					FileEncryptor.EncryptFile(request);
					Console.WriteLine("Success.");
					break;

				case "decrypt":
				case "-d":
					Console.WriteLine($"Decrypting {request.FileIn}...");
					// TODO: Implement File Decryptor
					//FileDecryptor.DecryptFile(request);
					//Console.WriteLine("Success.");
					break;

				case "help":
				case "-h":
					Console.WriteLine("JSON Format: { \"Action\": \"encrypt\", \"Password\": \"...\", \"FileIn\": \"...\", \"FileOut\": \"...\" }");
					break;

				default:
					Console.WriteLine("Unknown action.");
					return 1;
			}

			return 0;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"ERROR: {ex.Message}");
			return 10;
		}
	}
}
