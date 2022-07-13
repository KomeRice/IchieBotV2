// Inspections disabled for legacy classes
// ReSharper disable All
#pragma warning disable CS8618
namespace IchieBotData.Legacy
{
	public class Icon
	{
		/// <summary>
		/// Icon name
		/// </summary>
		public string Name;
		
		/// <summary>
		/// Path to icon image from root
		/// </summary>
		public string Path;

		/// <summary>
		/// Code of emote to use on discord, sends error emote if not defined.
		/// </summary>
		public string EmoteCode;
	}

	public class IconCollection
	{
		public List<Icon> Icons;
	}
}