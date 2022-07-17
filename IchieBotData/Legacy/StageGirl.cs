// Inspections disabled for legacy classes
// ReSharper disable All

namespace IchieBotData.Legacy;

#pragma warning disable CS8618
public class StageGirl
{
	public int Id { get; set; }
	public string DressId { get; set; }
	public string Name { get; set; }
	public string ThumbUrl { get; set; }
	public Icon Element { get; set; }
	public Icon Row { get; set; }
	public int Rarity { get; set; }
	public bool Special { get; set; }
	public Move Climax { get; set; }
	public Ability UnitSkill { get; set; }
	public List<string> Aliases { get; set; }
	public List<Move> Moves { get; set; }
	public Ability Entry { get; set; }
	public List<Ability> Abilities { get; set; }
	public List<int> MaxStats { get; set; }
	public string Pool { get; set; }
	public List<string> TagList { get; set; }
	public List<string> RealTagList { get; set; } = new List<string>();
	public string Notes = "";
	public string MoveType = "";
	public string AccName = "";
	public Move AccMove { get; set; }

	//TODO - Make this proper
	private static readonly List<string> Seishou = new List<string>{"Aijo Karen","Kagura Hikari","Tsuyuzaki Mahiru","Tendo Maya",
		"Saijo Claudine","Hoshimi Junna","Daiba Nana","Hanayagi Kaoruko","Isurugi Futaba"};
	private static readonly List<string> Rinmeikan = new List<string>{"Tomoe Tamao","Otonashi Ichie","Akikaze Rui",
		"Yumeoji Fumi","Tanaka Yuyuko"};
	private static readonly List<string> Frontier = new List<string>{"Otsuki Aruru","Kano Misora","Kocho Shizuha",
		"Nonomiya Lalafin","Ebisu Tsukasa"};
	private static readonly List<string> Siegfeld = new List<string>{"Yukishiro Akira","Otori Michiru","Tsuruhime Yachiyo",
		"Yumeoji Shiori","Liu Mei Fan"};
	private static readonly List<string> Seiran = new List<string>{"Yanagi Koharu", "Minase Suzu", "Honami Hisame"};
	public static readonly List<string> SchoolList = new List<string>{"seishou","siegfeld","rinmeikan","frontier", "rmk", "seiran"};
	
	public override string ToString()
	{
		var abilities = "{";
		var moves = "{";
		var aliases = "";
		foreach (var m in Moves)
		{
			moves += m + " ";
		}
		moves += "}";
		foreach (var a in Abilities)
		{
			abilities += a + " ";
		}
		abilities += "}";
		foreach (var n in Aliases)
		{
			aliases += n + " ";
		}

		var all = new[]{Id.ToString(), Name, Pool, ThumbUrl, Element.Name, Row.Name, Rarity.ToString(), 
			Special.ToString(), Climax.ToString(), UnitSkill.ToString(),
			"{" + aliases, moves, abilities, "{" + string.Join(",", MaxStats.ToArray()) + "}"};
		return "{" + string.Join(", ", all);
	}

	/// <summary>
	/// Returns the school a girl is from as an int.
	/// </summary>
	/// <param name="girl">Stage girl to check the school of</param>
	/// <returns>0 if Seishou, 1 if RMK, 2 if Frontier, 3 if Siegfeld, -1 if error.</returns>
	public static int GetSchool(StageGirl girl)
	{
		if (Seishou.Any(n => girl.Name.Contains(n))) return 0;
		if (Rinmeikan.Any(n => girl.Name.Contains(n))) return 1;
		if (Frontier.Any(n => girl.Name.Contains(n))) return 2;
		if (Siegfeld.Any(n => girl.Name.Contains(n))) return 3;
		if (Seiran.Any(n => girl.Name.Contains(n))) return 4;
		return -1;
	}
}