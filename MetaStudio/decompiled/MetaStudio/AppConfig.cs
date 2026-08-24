using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace MetaStudio;

public class AppConfig
{
	private static string confilePath = Application.ExecutablePath.ToLower() + ".config";

	private static string file = Application.ExecutablePath;

	private static Configuration config = ConfigurationManager.OpenExeConfiguration(file);

	public static Dictionary<string, string> LoadAllConfig()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] allKeys = ConfigurationManager.AppSettings.AllKeys;
		foreach (string text in allKeys)
		{
			dictionary.Add(text, config.AppSettings.Settings[text].Value);
		}
		return dictionary;
	}

	public static void WriteConfig(string key, string value)
	{
		if (!config.AppSettings.Settings.AllKeys.Contains(key))
		{
			config.AppSettings.Settings.Add(key, value);
			config.Save((ConfigurationSaveMode)0);
		}
		else
		{
			config.AppSettings.Settings[key].Value = value;
			config.Save((ConfigurationSaveMode)0);
		}
	}

	public static string GetAppSetting(string key)
	{
		if (config.AppSettings.Settings.AllKeys.Contains(key))
		{
			return config.AppSettings.Settings[key].Value;
		}
		return string.Empty;
	}
}
