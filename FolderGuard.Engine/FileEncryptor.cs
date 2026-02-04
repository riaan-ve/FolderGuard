using System.Security.Cryptography;
public class FileEncryptor
{
	public static void EncryptFile(EncryptRequest req)
	{
		try
		{
			byte[] salt = RandomNumberGenerator.GetBytes(16);
			using (Aes aes = Aes.Create())
			{
				using var deriveBytes = new Rfc2898DeriveBytes(req.Password, salt, req.Iterations, HashAlgorithmName.SHA256);
				aes.Key = deriveBytes.GetBytes(32);
				byte[] hmacKey = deriveBytes.GetBytes(32);
				byte[] iv = aes.IV;

				// We'll store the hash here so we can write it at the very end
				byte[] computedHash;

				using (FileStream fsOutput = new FileStream(req.FileOut, FileMode.Create))
				{
					fsOutput.Write(salt, 0, salt.Length);
					fsOutput.Write(iv, 0, iv.Length);

					using (HMACSHA256 hmac = new HMACSHA256(hmacKey))
					{
						// Logic: Wrap the streams, but we extract the hash 
						// BEFORE the outer FileStream is closed.
						using (CryptoStream csHmac = new CryptoStream(fsOutput, hmac, CryptoStreamMode.Write, leaveOpen: true))
						{
							using (ICryptoTransform encryptor = aes.CreateEncryptor())
								using (CryptoStream csEncrypt = new CryptoStream(csHmac, encryptor, CryptoStreamMode.Write))
								using (FileStream fsInput = new FileStream(req.FileIn, FileMode.Open))
								{
									fsInput.CopyTo(csEncrypt);
									csEncrypt.FlushFinalBlock();
								}
						}
						computedHash = hmac.Hash ?? throw new Exception("Hash failed");
					}

					// Now the FileStream is still open, write the hash to the end
					fsOutput.Write(computedHash, 0, computedHash.Length);
				}
			}
		}
		finally
		{
			// Zero out the password byte array
			if (req.Password != null)
			{
				Array.Clear(req.Password, 0, req.Password.Length);
			}
		}
	}
}
