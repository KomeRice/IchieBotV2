﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IchieBotData;
using IchieBotData.Common;
using IchieBotData.Effects;
using IchieBotV2.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IchieBotV2.Services;

public class DatabaseService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    public StatCalculator Calculator { get; set; }
    private const string DataRoot = @"Data/";

    private readonly Dictionary<string, DamageEffect> _damageEffects;
    private readonly Dictionary<string, NonDamageEffect> _nonDamageEffects;
    private readonly Dictionary<string, Target> _targets;
    private readonly Dictionary<string, Icon> _icons;
    private readonly Dictionary<string, JObject> _sequence;
    private readonly Dictionary<string, Act> _acts;
    public readonly Dictionary<string, Dress> Dresses;
    private readonly Dictionary<string, JObject> _chara;
    private readonly Dictionary<string, Skill> _entrySkills;
    private readonly Dictionary<string, Skill> _startSkills;
    private readonly Dictionary<string, Skill> _passiveSkills;
    private readonly Dictionary<string, Skill> _partySkills;
    private Dictionary<string, Skill> _equipSkills;

	public DatabaseService(DiscordSocketClient client, InteractionService commands, IServiceProvider services,
		StatCalculator calculator)
	{
		_client = client;
		_commands = commands;
		_services = services;
		Calculator = calculator;

		_damageEffects = LoadDamageEffects();
		_nonDamageEffects = LoadNonDamageEffects();
		_targets = LoadTargets();
		_icons = LoadIcons();

		_chara = (DeserializeJsonFile<Dictionary<string, JObject>>(DataRoot + "Raw/jp/chara.json") as Dictionary<string, JObject>)!;
		
		var sequence1 = DeserializeJsonFile<Dictionary<string, JObject>>(DataRoot + "Raw/jp/sequence.json") as Dictionary<string, JObject>;
		var sequence2 = DeserializeJsonFile<Dictionary<string, JObject>>(DataRoot + "Raw/jp/sequence2.json") as Dictionary<string, JObject>;
		_sequence = MergeDicts(sequence1!, sequence2!);
		
		var s1 = LoadActs(DataRoot + "Raw/jp/skill.json");
		var s2 = LoadActs(DataRoot + "Raw/jp/skill2.json");
		_acts = MergeDicts(s1, s2);
		
		_startSkills = LoadSkills(DataRoot + "Raw/jp/start_skill.json", false);
		_passiveSkills = LoadSkills(DataRoot + "Raw/jp/passive_skill.json", true);
		_partySkills = LoadSkills(DataRoot + "Raw/jp/party_skill.json", true);
		_entrySkills = LoadSkills(DataRoot + "Raw/jp/entry_skill.json", false, true);
		
		Dresses = LoadDresses(DataRoot + "Raw/jp/dress.json");
	}

	private Dictionary<string, NonDamageEffect> LoadNonDamageEffects()
	{
		var outDict = DeserializeJsonFile<Dictionary<string, NonDamageEffect>>(DataRoot + "NonDamageEffects.json");
		return outDict == null
			? new Dictionary<string, NonDamageEffect>()
			: (Dictionary<string, NonDamageEffect>)outDict;
	}
	
	private Dictionary<string, DamageEffect> LoadDamageEffects()
	{
		var outDict = DeserializeJsonFile<Dictionary<string, DamageEffect>>(DataRoot + "DamageEffects.json");
		return outDict == null
			? new Dictionary<string, DamageEffect>()
			: (Dictionary<string, DamageEffect>)outDict;
	}

	private Dictionary<string, Target> LoadTargets()
	{
		var outDict = DeserializeJsonFile<Dictionary<string, Target>>(DataRoot + "Targets.json");
		return outDict == null
			? new Dictionary<string, Target>()
			: (Dictionary<string, Target>)outDict;
	}

	private Dictionary<string, Icon> LoadIcons()
	{
		var iconList = (List<Icon>?) DeserializeJsonFile<List<Icon>>(DataRoot + "Icons.json");
		var outDict = new Dictionary<string, Icon>();
		if (iconList == null)
			return outDict;

		foreach (var icon in iconList)
		{
			outDict[icon.Id] = icon;
		}

		return outDict;
	}

	private static object? DeserializeJsonFile<T>(string path)
    {
        var f = File.ReadAllText(path);
        var jsonList = JsonConvert.DeserializeObject<T>(f);

        if (jsonList is not null) return jsonList;
        Console.WriteLine($"Failed to open file '{path}'");
        return null;
    }

    private Dictionary<string, Act> LoadActs(string path)
    {
	    var f = File.ReadAllText(path);
	    dynamic? json = JsonConvert.DeserializeObject(f);
	    var outDict = new Dictionary<string, Act>();

	    if (json == null)
		    return new Dictionary<string, Act>();

	    foreach (JProperty prop in json)
	    {
		    try
		    {
			    var id = prop.Name;
			    var jObj = prop.Value;
			    if (jObj == null)
				    throw new FormatException("Bad format");
			    var attribute = (Element)int.Parse(jObj["attribute_id"]!.ToString());
			    var cost = int.Parse(jObj["cost"]!.ToString());
			    var skillName = ((jObj["name"] as JObject)!)["ja"]!.ToString();
			    var skill = GetEffectsFromJsonSkill((jObj as JObject)!, attribute, id);

			    outDict[id] = new Act(skillName, skill, null, cost);
		    }
		    catch (Exception e)
		    {
			    //Program.LogAsync(new LogMessage(LogSeverity.Error, "loadActs",
				//    $"Failed to load act id {prop.Name}", e));
		    }
	    }

	    return outDict;
    }

    private Dictionary<string, Skill> LoadSkills(string path, bool isPassive, bool isEntry = false)
    {
	    var f = File.ReadAllText(path);
	    dynamic? json = JsonConvert.DeserializeObject(f);

	    var outDict = new Dictionary<string, Skill>();

	    if (json == null)
		    return new Dictionary<string, Skill>();

	    foreach (JProperty prop in json)
	    {
		    try
		    {
			    var id = prop.Name;

			    if (prop.Value is not JObject jObj)
			    {
				    throw new FormatException("Bad format");
			    }

			    var skill = GetEffectsFromJsonSkill(jObj, debugId:id, isPassive:isPassive, isEntry:isEntry);

			    outDict[id] = skill;
		    }
		    catch (Exception e)
		    {
			    //Program.LogAsync(new LogMessage(LogSeverity.Error, "loadSkills",
				//    $"Failed to load skill id {prop.Name}", e));
		    }
	    }
	    
	    return outDict;
    }

    private Skill GetEffectsFromJsonSkill(JObject jObj, Element attribute = Element.NoElem, string debugId = "",
	    bool isPassive = false, bool isEntry = false)
    {
		var effects = new List<EffectInst>();
	    var skillIconId = jObj["icon_id"]!.ToString();

		for (var i = 1; i < 6; i++)
		{
			var optionId = jObj[$"skill_option{i}_id"]!.ToString();
			if(optionId == "0")
				continue;

			var optionHitRate = isPassive ? 100 : int.Parse(jObj[$"skill_option{i}_hit_rate"]!.ToString());
			var optionTargetId = jObj[$"skill_option{i}_target_id"]!.ToString();
			var optionTarget = _targets[optionTargetId];

			List<int> optionTimes;
			List<int> optionValues;
			if (isEntry)
			{
				optionTimes = new List<int>()
				{
					int.Parse(jObj[$"skill_option{i}_time"]!.ToString())
				};
				
				optionValues = new List<int>()
				{
					int.Parse(jObj[$"skill_option{i}_value"]!.ToString())
				};
			}
			else
			{
				optionTimes = isPassive
					? new List<int>() { }
					: JObjectToList((jObj[$"skill_option{i}_times"] as JObject)!);
				optionValues = JObjectToList((jObj[$"skill_option{i}_values"] as JObject)!);
			}

			if (_nonDamageEffects.ContainsKey(optionId))
			{
				var inst = new NonDamageEffectInst(_nonDamageEffects[optionId], optionTarget.Description,
					optionHitRate, optionValues, optionTimes, attribute);
				effects.Add(inst);
			}
			else if (_damageEffects.ContainsKey(optionId))
			{
				var hitCount = jObj["sequence_id"] != null ? GetHitCountFromSequence(jObj["sequence_id"]!.ToString(), i) : 1;
				var inst = new DamageEffectInst(_damageEffects[optionId], optionTarget.Description,
					optionHitRate, optionTimes, optionValues, hitCount, attribute);
				effects.Add(inst);
			}
			else
			{
				//Program.LogAsync(new LogMessage(LogSeverity.Error, "loadActs",
				//	$"Ignored skill {optionId} in act {debugId}"));
			}
		}

		var skill = new Skill(effects, Condition.None, skillIconId);
		return skill;
    }
    
    private Dictionary<string, Dress> LoadDresses(string path)
    {
	    var f = File.ReadAllText(path);
	    dynamic? json = JsonConvert.DeserializeObject(f);
	    var outDict = new Dictionary<string, Dress>();

	    if (json == null)
		    return new Dictionary<string, Dress>();

	    foreach (JProperty prop in json)
	    {
		    try
		    {
			    var jObj = prop.Value;
			    var id = prop.Name;
			    var thumbUrl = $"https://cdn.karth.top/api/assets/jp/res/item_root/large/1_{id}.png";
			    var charId = jObj["chara_id"]!.ToString();

			    var charName = _chara[charId]["name_ruby"]!["ja"]!.ToString();
			    var dressName = $"{jObj["name"]!["ja"]!} {charName}";
			    var attribute = (Element)int.Parse(jObj["attribute_id"]!.ToString());
			    var type = (AttackType)int.Parse(jObj["attack_type"]!.ToString());
			    var stats = GetAllStats(id);
			    var rowIndex = int.Parse(jObj["role_index"]!.ToString());
			    var releasedJp = int.Parse(jObj["published_at"]!.ToString());
			    var hasRemake = Calculator.HasRemake(id);

			    var actIds = new List<string>();
			    for (var i = 1; i < 4; i++)
			    {
				    actIds.Add(jObj[$"command_skill{i}_id"]!.ToString());
			    }
			    
			    var basics = actIds.Select(actId => _acts[actId]).ToList();
			    var cx = _acts[jObj[$"command_unique_skill_id"]!.ToString()];

			    var autos = new List<Skill>();
			    for (var i = 1; i < 5; i++)
			    {
				    var autoId = jObj[$"auto_skill{i}_id"]!.ToString();
				    if(autoId == "0")
					    continue;
				    switch (jObj[$"auto_skill{i}_type"]!.ToString())
				    {
					    case "2":
						    autos.Add(_startSkills[autoId].CloneWithElement(attribute));
						    break;
					    case "1":
						    autos.Add(_passiveSkills[autoId].CloneWithElement(attribute));
						    break;
				    }
			    }
			    
			    
			    var partySkillId = jObj["party_skill_id"]!.ToString();
			    var unitSkill = partySkillId == "0" ? null : _partySkills[partySkillId];
			    
			    var entrySkillId = jObj["entry_skill_id"]!.ToString();
			    Skill? entrySkill;
			    if (entrySkillId == "0")
				    entrySkill = null;
			    else
			    {
				    entrySkill = _entrySkills[entrySkillId];
			    }
			    
			    var aliases = new List<string>();

			    var cost = int.Parse(jObj["cost"]!.ToString());
			    var rarity = int.Parse(jObj["base_rarity"]!.ToString());
			    var pool = Pool.Permanent;
			    var notes = "";

			    var newDress = new Dress(id, thumbUrl, dressName, attribute, stats, rowIndex, type, -1,
				    releasedJp, basics, autos, cx, unitSkill, entrySkill, aliases, cost, rarity,
				    pool, notes, hasRemake);
			    outDict[id] = newDress;
		    }
		    catch (Exception e)
		    {
			    Program.LogAsync(new LogMessage(LogSeverity.Error, "loadDresses",
				    $"Failed to load dress id {prop.Name}", e));
		    }
	    }

	    return outDict;
    }
    
    public List<int> GetDressStats(string dressId, int rb = 0)
    {
        if (rb != 0 && !Calculator.HasRemake(dressId))
            rb = 0;

        var parameters = Calculator.GetDressStats(dressId, level: 80 + 5 * rb, remake: rb, friendship: 30 + 5 * rb);
        return parameters.ToIntList();
    }

    public List<List<int>> GetAllStats(string dressId)
    {
	    if (!Calculator.HasRemake(dressId))
		    return new List<List<int>>{ GetDressStats(dressId) };

        var ret = new List<List<int>>();
        for (var i = 0; i < 5; i++)
        {
            ret.Add(GetDressStats(dressId, i));
        }

        return ret;
    }
    
    private static List<int> JObjectToList(JObject obj)
    {
	    var outList = new List<int>();
	    foreach (var i in obj)
	    {
		    outList.Add(int.Parse(i.Value!.ToString()));
	    }

	    return outList;
    }

    private static Dictionary<TKey, TValue> MergeDicts<TKey, TValue>(Dictionary<TKey, TValue> d1,
	    Dictionary<TKey, TValue> d2) where TKey : notnull
    {
	    var outDict = new Dictionary<TKey, TValue>();
		foreach (var key in d1.Keys)
		{
			outDict[key] = d1[key];
		}
		foreach (var key in d2.Keys)
		{
			outDict[key] = d2[key];
		}

		return outDict;
    }

    private int GetHitCountFromSequence(string sequenceId, int optionNumber)
    {
	    if (!_sequence.ContainsKey(sequenceId))
		    return 1;

	    var sequence = _sequence[sequenceId][$"option_{optionNumber}_damage_frame"] as JObject;

	    return sequence?.Count ?? 0;
    }

    public string GetIconEmote(string id)
    {
	    return _icons.TryGetValue(id, out var res) ? res.Emote : 
		    "<:please_ping_if_you_see_this:670781960800698378>";
    }
    
    public class Target
    {
	    public string DescriptionJp;
	    public string Description;

	    public Target(string descriptionJp, string description)
	    {
		    DescriptionJp = descriptionJp;
		    Description = description;
	    }
    }
}