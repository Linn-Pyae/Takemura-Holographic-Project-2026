using System.Collections.Generic;

namespace MetaStudio;

public class GridData
{
	public string Guid { get; set; }

	public string ID { get; set; }

	public string Name { get; set; }

	public string Size { get; set; }

	public bool IsBottom { get; set; }

	public Dictionary<string, string> Dic { get; set; }

	public Dictionary<string, ConfigData> DicConfig { get; set; }
}
