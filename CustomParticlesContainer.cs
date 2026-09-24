using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace peak;

/// <summary>
/// Scout 能量球粒子容器。
///
/// 基类 NEnergyCounter._Ready() 会 GetNode&lt;NParticlesContainer&gt;("%EnergyVfxBack" / "%EnergyVfxFront")，
/// 场景里缺这两个节点时每次进战斗都会刷 "Node not found" ERROR。
/// Scout 暂时没有能量球粒子特效，所以场景里挂这个空容器 + _particles = []：
/// NParticlesContainer.Restart() 会无条件访问 _particles（null 会 NRE），
/// 空数组让它在"能量增加时 Restart"变成空操作。
/// </summary>
public partial class CustomParticlesContainer : NParticlesContainer { }
