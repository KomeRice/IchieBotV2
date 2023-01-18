namespace IchieBotData.Effects;

public class DamageEffect : Effect
{
	public DamageEffect(int id, string name, string jpName, List<string> altNames, int iconId, string verbose, string extraVerbose, List<string> tags) 
		: base(id, name, jpName, altNames, iconId, verbose, extraVerbose, tags)
	{
	}
}