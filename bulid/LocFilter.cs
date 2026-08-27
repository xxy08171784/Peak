using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

class LocFilter
{
    static void Main()
    {
        var gameDir = @"c:\Users\shiqian\Peak\bulid\game_loc_new";
        var modDir = @"c:\Users\shiqian\Peak\localization\zhs";
        var outDir = @"c:\Users\shiqian\Peak\peak\localization\zhs";
        var files = "cards,powers,relics,ancients,characters,epochs,potions,card_library,rest_site_ui".Split(',');

        Directory.CreateDirectory(outDir);

        foreach (var f in files)
        {
            var gp = Path.Combine(gameDir, f + ".json");
            var mp = Path.Combine(modDir, f + ".json");
            var op = Path.Combine(outDir, f + ".json");

            if (!File.Exists(mp))
            {
                Console.WriteLine($"{f}: mod source missing");
                continue;
            }

            var modText = File.ReadAllText(mp, Encoding.UTF8);
            var modDoc = JsonDocument.Parse(modText);

            var gameKeys = new HashSet<string>();
            if (File.Exists(gp))
            {
                var gameText = File.ReadAllText(gp, Encoding.UTF8);
                var gameDoc = JsonDocument.Parse(gameText);
                foreach (var p in gameDoc.RootElement.EnumerateObject())
                    gameKeys.Add(p.Name);
            }

            var result = new Dictionary<string, string>();
            foreach (var p in modDoc.RootElement.EnumerateObject())
            {
                if (!gameKeys.Contains(p.Name))
                    result[p.Name] = p.Value.GetRawText();
            }

            // Merge with existing peak file if any
            if (File.Exists(op))
            {
                var existingText = File.ReadAllText(op, Encoding.UTF8);
                if (existingText.Trim().Length > 2) // not empty {}
                {
                    var existingDoc = JsonDocument.Parse(existingText);
                    foreach (var p in existingDoc.RootElement.EnumerateObject())
                    {
                        if (!result.ContainsKey(p.Name))
                            result[p.Name] = p.Value.GetRawText();
                    }
                }
            }

            Console.WriteLine($"{f}: modOnly={result.Count}");

            using var sw = new StreamWriter(op, false, new UTF8Encoding(false));
            sw.WriteLine("{");
            bool first = true;
            foreach (var kv in result)
            {
                if (!first) sw.WriteLine(",");
                sw.Write($"  {JsonSerializer.Serialize(kv.Key)}: {kv.Value}");
                first = false;
            }
            sw.WriteLine();
            sw.WriteLine("}");
        }
    }
}