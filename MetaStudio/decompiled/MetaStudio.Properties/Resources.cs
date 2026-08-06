using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace MetaStudio.Properties;

[CompilerGenerated]
[DebuggerNonUserCode]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = new ResourceManager("MetaStudio.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap _2023_09_06_135731
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("2023-09-06_135731", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap _2023_09_06_135750
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("2023-09-06_135750", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap _2023_09_06_141641
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("2023-09-06_141641", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap _2023_09_06_141700
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("2023-09-06_141700", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
