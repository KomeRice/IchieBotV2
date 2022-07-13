using IchieBotData.Effects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IchieBotData.Legacy;
using OldJsonFormatParser.LegacyClass;
using Icon = IchieBotData.Common.Icon;

namespace OldJsonFormatParser;

public class OldJsonParser
{
    private readonly string _filePath;

    public OldJsonParser(string path)
    {
        _filePath = path;
    }

    private List<StageGirl> SerializeJson()
    {
        var f = File.ReadAllText(_filePath);
        var dressList = JsonConvert.DeserializeObject<List<StageGirl>>(f);

        if (dressList is not null) return dressList;
        Console.WriteLine($"Failed to open file '{_filePath}'"); 
        return new List<StageGirl>();
    }

    public void ParseActDescriptions()
    {
        var dressList = SerializeJson();
        
        var descriptions = new HashSet<string>();
        var targets = new HashSet<string>();
        
        foreach(var dress in dressList)
        {
            foreach (var act in dress.Moves)
            {
                var effects = act.Description.Split("\n");
                foreach(var fullDescription in effects){
                    var effect = fullDescription.Split(":");
                    if (effect.Length != 2)
                    {
                        Console.WriteLine($"The following act has an unusual descrption: {act.Description}");
                        continue;
                    }

                    var description = effect[0].StartsWith("Mode ") ? effect[0][10..] : effect[0];
                    var target = effect[1];

                    descriptions.Add(description.ToLowerInvariant().TrimEnd());
                    targets.Add(target[1..].TrimEnd().TrimEnd(']').ToLowerInvariant());
                }
            }
        }
        
        Console.WriteLine($"Finished parsing acts.\n ----- Descriptions ----- \n{string.Join("\n", descriptions)}\n\n " +
                          $"----- Targets ----- \n{string.Join("\n", targets)}");
    }

    private static List<EffectInst> GetEffectsFromLine(string descriptor)
    {
        return new List<EffectInst>();
    }

    public static void MapNewIcons()
    {
        const string mappingJsonPath = "mappings.json";
        const string oldIconsJson = "jsonOld/icons.json";
        const string outPath = "jsonNew/icons.json";
        Directory.CreateDirectory("jsonNew");

        var fileMappings = File.ReadAllText(mappingJsonPath);
        var fileIcons = File.ReadAllText(oldIconsJson);

        var mappings = JObject.Parse(fileMappings);
        var icons = JsonConvert.DeserializeObject<IconCollection>(fileIcons);
        if(icons is null)
            return;

        var iconList = icons.Icons;
        var outList = new List<Icon>();

        foreach (var icon in iconList)
        {
            var val = mappings.GetValue(icon.Name);
            if (val is null)
            {
                Console.WriteLine($"Got null value on: {icon.Name}");
                continue;
            }

            var id = val.ToString();
            outList.Add(new Icon(id, icon.Name, icon.EmoteCode));
        }

        var json = JsonConvert.SerializeObject(outList, Formatting.Indented);
        
        File.WriteAllText(outPath, json);
        
        Console.WriteLine("Mapping new icons: Done");
    }
}