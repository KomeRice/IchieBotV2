// Inspections disabled for legacy classes
// ReSharper disable All
#pragma warning disable CS8618
namespace IchieBotData.Legacy
{
	public class Ability
	{
		public string Description { get; set; }
		public Icon AbilityIcon { get; set; }

		public override string ToString()
		{
			return "{(IconPath :" + AbilityIcon.Path + "): " + Description + "}";
		}
	}
}