using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

class Program {
    static void Main() {
        var gd = @"c:\Users\shiqian\Peak\bulid\game_loc_new";
        var md = @"c:\Users\shiqian\Peak\localization\zhs";
        var od = @"c:\Users\shiqian\Peak\peak\localization\zhs";
        var ff = new[]{"cards","powers","relics","ancients","characters","epochs","potions","card_library","rest_site_ui"};
        var ov = new Dictionary<string,HashSet<string>>{ {"ancients", new HashSet<string>{"NEOW.talk.firstVisitEver.0-0.ancient"} } };
        Directory.CreateDirectory(od);
        foreach (var f in ff) {
            var gp = Path.Combine(gd, f+".json");
            var mp = Path.Combine(md, f+".json");
            var op = Path.Combine(od, f+".json");
            if (!File.Exists(mp)) { Console.WriteLine($"{f}: mod missing"); continue; }
            var mt = File.ReadAllText(mp, Encoding.UTF8);
            var md_ = JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(mt);
            var gk = new HashSet<string>();
            if (File.Exists(gp)) {
                var gt = File.ReadAllText(gp, Encoding.UTF8);
                var gd_ = JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(gt);
                foreach (var k in gd_.Keys) gk.Add(k);
            }
            var res = new Dictionary<string,JsonElement>();
            var overrides = ov.ContainsKey(f) ? ov[f] : new HashSet<string>();
            foreach (var kv in md_)
                if (!gk.Contains(kv.Key) || overrides.Contains(kv.Key))
                    res[kv.Key] = kv.Value;
            // Merge with existing
            if (File.Exists(op)) {
                try {
                    var et = File.ReadAllText(op, Encoding.UTF8);
                    if (et.Trim().Length > 2) {
                        var ex = JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(et);
                        foreach (var kv in ex)
                            if (!res.ContainsKey(kv.Key)) res[kv.Key] = kv.Value;
                    }
                } catch {}
            }
            Console.WriteLine($"{f}: modOnly={res.Count}");
            var opt = new JsonSerializerOptions{ WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            File.WriteAllText(op, JsonSerializer.Serialize(res, opt) + "\n", new UTF8Encoding(false));
        }
    }
}
