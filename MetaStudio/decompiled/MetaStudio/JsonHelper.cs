using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MetaStudio;

public class JsonHelper
{
	public static void WriteJson(List<GridData> lstData, string filename)
	{
		try
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			StringWriter stringWriter = new StringWriter();
			jsonSerializer.Serialize(new JsonTextWriter(stringWriter), lstData);
			string contents = stringWriter.GetStringBuilder().ToString();
			File.WriteAllText(filename, contents);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public static List<GridData> ReadJson(string filename)
	{
		try
		{
			string value = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<List<GridData>>(value);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
		return null;
	}

	public static void WriteConfigJson(Dictionary<string, ConfigData> configData, string filename)
	{
		try
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			StringWriter stringWriter = new StringWriter();
			jsonSerializer.Serialize(new JsonTextWriter(stringWriter), configData);
			string contents = stringWriter.GetStringBuilder().ToString();
			File.WriteAllText(filename, contents);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
		}
	}

	public static void WriteConfigJson2(List<RegInfo> configData, string filename)
	{
		try
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			StringWriter stringWriter = new StringWriter();
			jsonSerializer.Serialize(new JsonTextWriter(stringWriter), configData);
			string contents = stringWriter.GetStringBuilder().ToString();
			File.WriteAllText(filename, contents);
		}
		catch (Exception ex)
		{
			ConstData.Importing = false;
			LogerHelper.Error(ex.Message);
		}
	}

	public static Dictionary<string, ConfigData> ReadConfigJson(string filename)
	{
		try
		{
			string value = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<Dictionary<string, ConfigData>>(value);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
			return null;
		}
	}

	public static List<RegInfo> ReadConfigJson2(string filename)
	{
		try
		{
			string value = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<List<RegInfo>>(value);
		}
		catch (Exception ex)
		{
			LogerHelper.Error(ex.Message);
			return null;
		}
	}
}
