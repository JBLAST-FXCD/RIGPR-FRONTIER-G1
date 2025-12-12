using System.Collections;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using UnityEngine;

// jess @ 12/12/2025

//<summary>
// This script provides methods for encrypting and decrypting strings using AES encryption
// It uses a master passphrase and a salt to derive the encryption key and IV
// key components are hardcoded, encryption is not intended to be unbreakable but to prevent casual tampering with save files
//</summary>


public static class SaveEncryption
{
    // master phrase for key derivation, must b e kept secret and unchanged to maintain compatibility with existing save files
    private const string MASTER_PASSPHRASE = "MousechitectCityBuild_Codename_MouseGame_RIPMouseAirportGame2025<3";

    // number opf iterations for key generation, PBKDF2 uses this to make brute-force attacks more difficult at the expense of key generation speed
    private const int ITERATIONS = 100000;

    // non-secret salt for key derivation, increases key complexity
    private static readonly byte[] encryption_salt = Encoding.UTF8.GetBytes("MousechitectCityBuilder_Salt_2025");

    // <summary>
    // derives 256-bit encryption key and 128-bit IV from the master passphrase and salt using PBKDF2
    // </summary>
    private static void DeriveKeyAndIV(out byte[] encryption_key, out byte[] encryption_iv)
    {
        // Rfc2898DeriveBytes is c# implementation of PBKDF2
        using (var kdf = new Rfc2898DeriveBytes(MASTER_PASSPHRASE, encryption_salt, ITERATIONS, HashAlgorithmName.SHA256))
        {
            // aes requires 32 byte key and 16 byte iv
            encryption_key = kdf.GetBytes(32);
            encryption_iv = kdf.GetBytes(16);
        }
    }

    // <summary>
    // encrypts plaintext string into aes-256 base 64 string
    // </summary>
    public static string EncryptString(string plain_text)
    {
        // derive key and iv for every operation
        DeriveKeyAndIV(out byte[] encryption_key, out byte[] encryption_iv);

        using (AesManaged aes = new AesManaged())
        {
            aes.Key = encryption_key;
            aes.IV = encryption_iv;

            // cbc = cipher block chaining mode
            aes.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            // holds bytes in memory before encoding
            using (MemoryStream ms_encrypt = new MemoryStream())
            {
                // actual encryption operation as data is written
                using (CryptoStream cs_encrypt = new CryptoStream(ms_encrypt, encryptor, CryptoStreamMode.Write))
                {
                    // stream writer writes string data to crypto stream
                    using (StreamWriter sw_encrypt = new StreamWriter(cs_encrypt))
                    {
                        sw_encrypt.Write(plain_text);
                    }
                    // returns byte array as base64 string file
                    return Convert.ToBase64String(ms_encrypt.ToArray());
                }
            }
        }
    }

    // <summary>
    // decrypts aes-256 base64 string into plaintext string
    // </summary>
    public static string DecryptString(string cipher_text)
    {
        DeriveKeyAndIV(out byte[] encryption_key, out byte[] encryption_iv);

        try
        {
            // convert base64 string back to encrypted byte array
            byte[] cipher_bytes = Convert.FromBase64String(cipher_text);

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = encryption_key;
                aes.IV = encryption_iv;
                aes.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // reverse process of encryption
                using (MemoryStream ms_decrypt = new MemoryStream(cipher_bytes))
                {
                    using (CryptoStream cs_decrypt = new CryptoStream(ms_decrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr_decrypt = new StreamReader(cs_decrypt))
                        {
                            return sr_decrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        // required to catch exceptions from invalid data
        catch (Exception e)
        {
            Debug.LogError("Decryption failed" + e.Message);
            return null;
        }
    }
}
