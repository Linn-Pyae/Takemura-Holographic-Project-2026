using System.Collections.Generic;

namespace MetaStudio;

public class ConfigData
{
	public int id;

	public int speed = -1;

	public int motodire = -1;

	public int brightness = -1;

	public int angle = -1;

	public int centerX = -1;

	public int centerY = -1;

	public int scale = -1;

	public int startX = -1;

	public int startY = -1;

	public Dictionary<int, ConnectedScreen> DicConnScreen = new Dictionary<int, ConnectedScreen>();
}
