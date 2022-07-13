// Inspections disabled for legacy classes
// ReSharper disable All
#pragma warning disable CS8618
namespace IchieBotData.Legacy
{
	public class Move
	{
		public int Cost { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public Icon AttackIcon { get; set; }

		public override string ToString()
		{
			return "{[AP" + Cost + "]" + Name + "(IconPath :" + AttackIcon.Path + "): " + Description + "}" ;
		}
	}
}