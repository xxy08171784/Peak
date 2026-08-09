using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace peak;

/// <summary>
/// 休息处/商店场景用的选择框（reticle）脚本。
/// 继承游戏自带的 NSelectionReticle，以便 NRestSiteCharacter._Ready()
/// 中 GetNode<NSelectionReticle>("%SelectionReticle") 能成功取到节点。
/// </summary>
public partial class CustomSelectionReticle : NSelectionReticle
{
}
