using System;
using System.Security.Cryptography;

namespace GodInfo.Helper.Crypto
{
    public class MozillaPBE
    {
        private byte[] cipherText { get; set; }
        private byte[] GlobalSalt { get; set; }
        private byte[] MasterPassword { get; set; }
        private byte[] EntrySalt { get; set; }
        public byte[] partIV { get; private set; }

        public MozillaPBE(byte[] cipherText, byte[] GlobalSalt, byte[] MasterPassword, byte[] EntrySalt, byte[] partIV)
        {
            this.cipherText = cipherText;
            this.GlobalSalt = GlobalSalt;
            this.MasterPassword = MasterPassword;
            this.EntrySalt = EntrySalt;
            this.partIV = partIV;
        }

        public byte[] Compute()
        {
            int iterations = 1;
            int keyLength = 32;

            // GLMP
            var GLMP = new byte[GlobalSalt.Length + MasterPassword.Length]; // GlobalSalt + MasterPassword
            Buffer.BlockCopy(GlobalSalt, 0, GLMP, 0, GlobalSalt.Length);
            Buffer.BlockCopy(MasterPassword, 0, GLMP, GlobalSalt.Length, MasterPassword.Length);

            // HP
            var HP = new SHA1Managed().ComputeHash(GLMP); // SHA1(GLMP)

            // IV
            byte[] ivPrefix = { 0x04, 0x0e };
            var IV = new byte[ivPrefix.Length + partIV.Length]; // ivPrefix + partIV
            Buffer.BlockCopy(ivPrefix, 0, IV, 0, ivPrefix.Length);
            Buffer.BlockCopy(partIV, 0, IV, ivPrefix.Length, partIV.Length);

            // 使用 PBKDF2
            var pbkdf2 = new Pbkdf2(new HMACSHA256(HP), EntrySalt, iterations);
            var key = pbkdf2.GetBytes(keyLength);

            // AES-CBC-256 settings
            Aes aes = new AesManaged
            {
                Mode = CipherMode.CBC,
                BlockSize = 128,
                KeySize = 256,
                Padding = PaddingMode.Zeros,
            };

            // Decrypt AES cipher text
            ICryptoTransform AESDecrypt = aes.CreateDecryptor(key, IV);
            var clearText = AESDecrypt.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return clearText;
        }
    }
} 