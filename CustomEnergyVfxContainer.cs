using System;
using System.Reflection;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace peak;

/// <summary>
/// Provides the typed particle-container contract required by <c>NEnergyCounter</c>.
/// The scene supplies a non-null particle array, while Scout-specific particles remain optional.
/// </summary>
public partial class CustomEnergyVfxContainer : NParticlesContainer
{
	private static readonly FieldInfo ParticlesField =
		typeof(NParticlesContainer).GetField(
			"_particles",
			BindingFlags.Instance | BindingFlags.NonPublic
		) ?? throw new MissingFieldException(
			typeof(NParticlesContainer).FullName,
			"_particles"
		);

	/// <summary>
	/// Initializes the base container with an empty, non-null particle list.
	/// </summary>
	public CustomEnergyVfxContainer()
	{
		ParticlesField.SetValue(this, new Array<GpuParticles2D>());
	}
}
