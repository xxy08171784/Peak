using System.Reflection;

var asm = Assembly.LoadFile(@"D:\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var type = asm.GetType("MegaCrit.Sts2.Core.Models.CardModel");

var method = type.GetMethod("GetResultLocationForCardPlay", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

if (method != null)
{
    Console.WriteLine("FOUND: " + method.ToString());
    Console.WriteLine("IsVirtual: " + method.IsVirtual);
    Console.WriteLine("IsAbstract: " + method.IsAbstract);
    Console.WriteLine("IsFamily: " + method.IsFamily);
    Console.WriteLine("IsPublic: " + method.IsPublic);
}
else
{
    Console.WriteLine("METHOD NOT FOUND in sts2.dll");
    Console.WriteLine("Methods with 'Location' or 'Result' in name:");
    foreach (var m in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
    {
        if (m.Name.Contains("Location") || m.Name.Contains("Result"))
            Console.WriteLine("  " + m.Name + " | IsVirtual=" + m.IsVirtual + " | IsFamily=" + m.IsFamily);
    }
}
