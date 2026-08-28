using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace peak.Patches;

/// <summary>
/// 修复第4幕存档丢失问题。
/// 
/// 根因：SerializableRoomSet 的 EventIds/NormalEncounterIds/EliteEncounterIds 标有
/// [JsonSerializeCondition(SaveIfNotCollectionEmptyOrNull)]，空列表不序列化。
/// Act4 的 AllEvents 为空数组，保存后 JSON 中缺少这些字段，读档时反序列化为 null，
/// RoomSet.FromSave 对 null 调用 .Select() 抛出 ArgumentNullException。
/// </summary>
[HarmonyPatch(typeof(RoomSet), "FromSave")]
public static class RoomSetFromSaveFixPatch
{
    [HarmonyPrefix]
    static void FixNullLists(SerializableRoomSet save)
    {
        if (save.EventIds == null)
            save.EventIds = new List<ModelId>();
        if (save.NormalEncounterIds == null)
            save.NormalEncounterIds = new List<ModelId>();
        if (save.EliteEncounterIds == null)
            save.EliteEncounterIds = new List<ModelId>();
    }
}