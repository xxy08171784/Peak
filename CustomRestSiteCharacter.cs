using Godot;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace peak;

/// <summary>
/// Scout 休息处（篝火）角色视觉脚本。
/// 继承游戏自带的 NRestSiteCharacter，以便 NRestSiteRoom 通过
/// NRestSiteCharacter.Create() 加载本场景时能正确实例化。
/// 本项目使用 AnimatedSprite2D 帧动画（无 Spine），基类的
/// GetChildSpineNodes() 只匹配 SpineSprite 节点，会安全跳过 AnimatedSprite2D。
/// </summary>
public partial class CustomRestSiteCharacter : NRestSiteCharacter
{
}
