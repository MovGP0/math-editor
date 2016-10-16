using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Security.Cryptography;

namespace Editor
{
    public static class ConfigManager
    {
        private static readonly string ExePath = Assembly.GetEntryAssembly().Location;
        private static AppSettingsSection _appSection;
        private static Configuration _config;        

        static ConfigManager()
        {
            try
            {
                if (!Directory.Exists(PublicFolderPath))
                {
                    Directory.CreateDirectory(PublicFolderPath);
                }
                var existed = true;
                if (!File.Exists(PublicConfigFilePath))
                {
                    CopyConfigFile();
                    existed = false;
                }
                var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = PublicConfigFilePath };
                _config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                _appSection = _config.AppSettings;
                if (!existed)
                {
                    SetConfigurationValue(KeyName.Version, Assembly.GetEntryAssembly().GetName().Version.ToString());
                }
            }
            catch { }
        }
        
        public static string GetConfigurationValue(KeyName key)
        {
            try
            {
                return _appSection.Settings[key.ToString()].Value;
            }
            catch
            {
                return "";
            }
        }

        public static int GetNumber(KeyName key, int defaultNum)
        {
            try
            {
                var numString = GetConfigurationValue(key);                
                var num = int.Parse(numString);
                return num;
            }
            catch
            {
                return defaultNum;
            }
        }

        public static int GetEditorMode(int defaultMode){
            return GetNumber(KeyName.DefaultMode, defaultMode);
        }

        public static bool SetConfigurationValue(KeyName key, string value)
        {
            try
            {
                if (!File.Exists(PublicConfigFilePath))
                {
                    CopyConfigFile();
                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = PublicConfigFilePath };
                    _config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    _appSection = _config.AppSettings;
                    SetConfigurationValue(KeyName.Version, Assembly.GetEntryAssembly().GetName().Version.ToString());
                }
                if (_appSection.Settings.AllKeys.Contains(key.ToString()))
                {
                    _appSection.Settings[key.ToString()].Value = value;
                }
                else
                {
                    _appSection.Settings.Add(key.ToString(), value);
                }
                _config.Save();
                return true;
            }
            catch { }
            return false;
        }

        public static bool SetConfigurationValues(Dictionary<KeyName, string> configItems)
        {
            try
            {
                if (!File.Exists(PublicConfigFilePath))
                {
                    CopyConfigFile();
                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = PublicConfigFilePath };
                    _config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    _appSection = _config.AppSettings;
                    SetConfigurationValue(KeyName.Version, Assembly.GetEntryAssembly().GetName().Version.ToString());
                }
                foreach (var item in configItems)
                {
                    if (_appSection.Settings.AllKeys.Contains(item.Key.ToString()))
                    {
                        _appSection.Settings[item.Key.ToString()].Value = item.Value;
                    }
                    else
                    {
                        _appSection.Settings.Add(item.Key.ToString(), item.Value);
                    }
                }                
                _config.Save();
                return true;
            }
            catch { }
            return false;
        }


        public static string PublicConfigFilePath => 
            Path.Combine(PublicFolderPath, Path.GetFileName(Assembly.GetEntryAssembly().Location) + ".config");

        public static string PublicFolderPath => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Math_Editor_MV");

        private static void CopyConfigFile()
        {
            try
            {
                File.Copy(ExePath + ".config", PublicConfigFilePath);
            }
            catch { }
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static bool SetConfigurationValue_AES(KeyName key, string value)
        {
            try
            {
                return SetConfigurationValue(key, EncryptText(value,  "abcd_o82349834jefhdfer8&"));//GetConfigurationValue(KeyName.s01)));
            }
            catch { }
            return false;
        }

        public static string GetConfigurationValue_AES(KeyName key)
        {
            try
            {
                var base64String = GetConfigurationValue(key);
                return DecryptText(base64String, "abcd_o82349834jefhdfer8&");//GetConfigurationValue(KeyName.s01));
            }
            catch
            {
                return "";
            }
        }
        
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (var ms = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    aes.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (var ms = new MemoryStream())
            {
                using (var aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    aes.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public static string EncryptText(string input, string password)
        {
            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            var result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        public static string DecryptText(string input, string password)
        {
            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(input);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            var result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }
    }
}
