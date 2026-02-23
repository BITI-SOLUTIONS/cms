// ================================================================================
// ARCHIVO: CMS.Shared/Security/EncryptionService.cs
// PROPÓSITO: Servicio de encriptación y desencriptación AES-256
// DESCRIPCIÓN: Encripta datos sensibles para almacenamiento seguro en BD
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-13
// ================================================================================

using System.Security.Cryptography;
using System.Text;

namespace CMS.Shared.Security
{
    /// <summary>
    /// Servicio para encriptar y desencriptar datos usando AES-256.
    /// La clave debe ser de 32 bytes (256 bits).
    /// </summary>
    public class EncryptionService
    {
        private readonly byte[] _key;
        private const int IV_SIZE = 16; // 128 bits para AES

        /// <summary>
        /// Constructor con clave de encriptación desde configuración
        /// </summary>
        /// <param name="encryptionKey">Clave de 32 caracteres (256 bits)</param>
        public EncryptionService(string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentException("La clave de encriptación no puede estar vacía", nameof(encryptionKey));

            // Asegurar que la clave tenga exactamente 32 bytes (256 bits)
            _key = DeriveKey(encryptionKey, 32);
        }

        /// <summary>
        /// Encripta un texto plano usando AES-256-CBC
        /// </summary>
        /// <param name="plainText">Texto a encriptar</param>
        /// <returns>Texto encriptado en Base64 (IV + CipherText)</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Generar IV aleatorio
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Combinar IV + CipherText
            var result = new byte[IV_SIZE + cipherBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, IV_SIZE);
            Array.Copy(cipherBytes, 0, result, IV_SIZE, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Desencripta un texto encriptado usando AES-256-CBC
        /// </summary>
        /// <param name="cipherText">Texto encriptado en Base64</param>
        /// <returns>Texto plano desencriptado</returns>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                if (fullCipher.Length < IV_SIZE)
                    throw new CryptographicException("Datos encriptados inválidos");

                // Extraer IV y CipherText
                var iv = new byte[IV_SIZE];
                var cipher = new byte[fullCipher.Length - IV_SIZE];

                Array.Copy(fullCipher, 0, iv, 0, IV_SIZE);
                Array.Copy(fullCipher, IV_SIZE, cipher, 0, cipher.Length);

                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex) when (ex is FormatException or CryptographicException)
            {
                throw new CryptographicException("Error al desencriptar: datos inválidos o clave incorrecta", ex);
            }
        }

        /// <summary>
        /// Deriva una clave de longitud fija usando PBKDF2
        /// </summary>
        private static byte[] DeriveKey(string password, int keyLength)
        {
            // Salt fijo para derivación de clave (en producción considerar uno configurable)
            var salt = Encoding.UTF8.GetBytes("CMS_BITI_SOLUTIONS_2026");
            
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password, 
                salt, 
                iterations: 100000, 
                HashAlgorithmName.SHA256);
            
            return pbkdf2.GetBytes(keyLength);
        }

        /// <summary>
        /// Verifica si un texto parece estar encriptado (es Base64 válido)
        /// </summary>
        public static bool IsEncrypted(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            try
            {
                var bytes = Convert.FromBase64String(text);
                // Debe tener al menos IV (16 bytes) + algo de contenido
                return bytes.Length > IV_SIZE;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Servicio para hash de contraseñas usando PBKDF2
    /// </summary>
    public static class PasswordHasher
    {
        private const int SALT_SIZE = 16; // 128 bits
        private const int HASH_SIZE = 32; // 256 bits
        private const int ITERATIONS = 100000;

        /// <summary>
        /// Genera un hash seguro de una contraseña
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash en formato: Salt$Hash (ambos en Base64)</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // Generar salt aleatorio
            var salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generar hash con PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                ITERATIONS,
                HashAlgorithmName.SHA256);

            var hash = pbkdf2.GetBytes(HASH_SIZE);

            // Combinar salt + hash
            var hashBytes = new byte[SALT_SIZE + HASH_SIZE];
            Array.Copy(salt, 0, hashBytes, 0, SALT_SIZE);
            Array.Copy(hash, 0, hashBytes, SALT_SIZE, HASH_SIZE);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con su hash
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="hashedPassword">Hash almacenado</param>
        /// <returns>True si coinciden</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                var hashBytes = Convert.FromBase64String(hashedPassword);

                if (hashBytes.Length != SALT_SIZE + HASH_SIZE)
                    return false;

                // Extraer salt
                var salt = new byte[SALT_SIZE];
                Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);

                // Extraer hash almacenado
                var storedHash = new byte[HASH_SIZE];
                Array.Copy(hashBytes, SALT_SIZE, storedHash, 0, HASH_SIZE);

                // Calcular hash de la contraseña proporcionada
                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    ITERATIONS,
                    HashAlgorithmName.SHA256);

                var computedHash = pbkdf2.GetBytes(HASH_SIZE);

                // Comparación de tiempo constante para evitar timing attacks
                return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genera un token aleatorio seguro (para reset de contraseña, etc.)
        /// </summary>
        /// <param name="length">Longitud en bytes (default 32)</param>
        /// <returns>Token en Base64 URL-safe</returns>
        public static string GenerateSecureToken(int length = 32)
        {
            var tokenBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }

            // Convertir a Base64 URL-safe
            return Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
