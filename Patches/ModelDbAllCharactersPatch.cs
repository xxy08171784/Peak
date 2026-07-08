using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using peak.Core.Models.Characters; // 导入您的 Scout 命名空间

namespace peak.Patches;

[HarmonyPatch(typeof(ModelDb), "get_AllCharacters")]
public static class ModelDbAllCharactersPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<CharacterModel> __result)
    {
        if (__result == null)
        {
            return;
        }

        // 1. 将原本硬编码返回的 5 人只读数组转为可操作的 List
        List<CharacterModel> characterList = __result.ToList();

        // 2. 从 ModelDb 数据库中获取我们已经成功初始化的 Scout 实例
        var scout = ModelDb.Character<Scout>();

        // 3. 如果列表中还没有童子军，就强行追加进去
        if (scout != null && !characterList.Contains(scout))
        {
            characterList.Add(scout);
        }

        // 4. 将新的列表赋值回结果，传给游戏选人界面
        __result = characterList;
    }
}