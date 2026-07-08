using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

// ===== 手动补充导入游戏本体的药水和模型命名空间 =====
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace peak.Core.Models.PotionPools;

public sealed class ScoutPotionPool : PotionPoolModel
{
    // 对应童子军的能量颜色名称
    public override string EnergyColorName => "scout";

    // 图鉴界面中药水瓶的描边颜色（使用黄色/金色）
    public override Color LabOutlineColor => new Color("FFD700");

    protected override IEnumerable<PotionModel> GenerateAllPotions()
    {
        // 暂时还没有专属药水，在此直接返回空数组以保证编译通过。
        // 以后如果您设计了专属药水，可以像遗物池一样，使用以下数组形式返回：
        /*
        return new PotionModel[]
        {
            ModelDb.Potion<ScoutEnergyPotion>() // 假设的专属药水类
        };
        */
        return Array.Empty<PotionModel>();
    }

    public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
    {
        // 目前未添加专属药水，直接返回全部（即空集合）。
        // 以后如果您加入了时代（Epoch）系统来限制药水解锁，可以仿照原版在这里加入判断：
        /*
        if (!unlockState.IsEpochRevealed<Scout4Epoch>())
        {
            return Array.Empty<PotionModel>();
        }
        */
        return GenerateAllPotions();
    }
}