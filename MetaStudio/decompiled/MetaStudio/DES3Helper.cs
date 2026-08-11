using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MetaStudio;

public static class DES3Helper
{
	private static readonly byte[] Key = Encoding.UTF8.GetBytes("my3deskey123456789012345");

	private static readonly byte[] IV = Encoding.UTF8.GetBytes("my3desiv");

	public static string Encrypt(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = Key;
		tripleDESCryptoServiceProvider.IV = IV;
		tripleDESCryptoServiceProvider.Mode = CipherMode.CBC;
		tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static string Decrypt(string text)
	{
		byte[] array = Convert.FromBase64String(text);
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = Key;
		tripleDESCryptoServiceProvider.IV = IV;
		tripleDESCryptoServiceProvider.Mode = CipherMode.CBC;
		tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDESCryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
		cryptoStream.Write(array, 0, array.Length);
		cryptoStream.FlushFinalBlock();
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}
