using System;
using System.IO;
using System.Security.Cryptography;

namespace PasswordManager
{
    static class Encryptor
    {
        // Encrypts and replaces source file using key and IV
        public static void Encrypt(String inputPath, byte[] key, byte[] IV)
        {
            // Creates a temp file for encrypting
            FileStream output = File.Create("$encrypted.tmp");
            output.Write(IV, 0, IV.Length);

            using (FileStream input = File.OpenRead(inputPath))
            using (var provider = new AesCryptoServiceProvider())
            using (var transform = provider.CreateEncryptor(key, IV))
            using (var cryptoStream = new CryptoStream(output, transform, CryptoStreamMode.Write))
            {
                // Skips IV
                input.Position = IV.Length;
                // Encrypts file
                input.CopyTo(cryptoStream);
            }
            // Replaces input file with encrypted file
            output.Close();
            File.Delete(inputPath);
            File.Move(output.Name, inputPath);
        }

        // Decrypts and replaces source file using key
        public static void Decrypt(String inputPath, byte[] key, byte[] IV)
        {
            // Creates a temp file for decrypting
            FileStream output = File.Create("$decrypted.tmp");
            // Re-adds IV to beginning of file
            output.Write(IV, 0, IV.Length);
            try
            {
                // Decrypts rest of file
                using (FileStream input = File.OpenRead(inputPath))
                using (var provider = new AesCryptoServiceProvider())
                using (var transform = provider.CreateDecryptor(key, IV))
                using (var cryptoStream = new CryptoStream(input, transform, CryptoStreamMode.Read))
                {
                    // Skips IV (which is unencrypted)
                    input.Position = IV.Length;
                    // Decrypts file
                    cryptoStream.CopyTo(output);
                }
            } catch (System.Security.Cryptography.CryptographicException)
            {
                // Error if the wrong key is used
                output.Close();
                File.Delete(output.Name);
                throw new IncorrectKeyException("The wrong key was used!");
            }
            
            // Replaces source file with decrypted file
            output.Close();
            File.Delete(inputPath);
            File.Move(output.Name, inputPath);
        }

        public class IncorrectKeyException : Exception
        {
            public IncorrectKeyException() { }

            public IncorrectKeyException(string s) : base(s) { }
        }
    }
}
