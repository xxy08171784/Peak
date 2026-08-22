#nullable enable
using System.Collections.Generic;
using System.IO;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace peak.Core.CardTypes;

internal static class PeakFrames
{
	private const string FrameDirectory = "res://images/cards";

	private static readonly Dictionary<CardType, Texture2D> FrameCache = new();
	private static readonly Dictionary<CardType, Texture2D> BorderCache = new();
	private static ShaderMaterial? _identityMaterial;

	internal static Texture2D? GetFrame(CardType type)
	{
		if (!PeakCardTypes.TryGetVisuals(type, out PeakCardTypes.TypeVisuals? visuals))
		{
			return null;
		}

		if (!FrameCache.TryGetValue(type, out Texture2D? texture))
		{
			texture = LoadRequiredTexture($"{FrameDirectory}/{visuals.FrameFileName}", "frame");
			FrameCache[type] = texture;
		}

		return texture;
	}

	internal static Texture2D? GetPortraitBorder(CardType type)
	{
		if (!PeakCardTypes.TryGetVisuals(type, out PeakCardTypes.TypeVisuals? visuals))
		{
			return null;
		}

		if (!BorderCache.TryGetValue(type, out Texture2D? texture))
		{
			texture = LoadRequiredTexture($"{FrameDirectory}/{visuals.BorderFileName}", "portrait border");
			BorderCache[type] = texture;
		}

		return texture;
	}

	internal static Material? GetFrameMaterial(CardType type)
	{
		if (!PeakCardTypes.IsPeakType(type))
		{
			return null;
		}

		_ = GetFrame(type);
		return _identityMaterial ??= ShaderUtils.GenerateHsv(1f, 1f, 1f);
	}

	private static Texture2D LoadRequiredTexture(string path, string role)
	{
		if (!ResourceLoader.Exists(path))
		{
			throw new FileNotFoundException($"Peak custom card {role} is missing: {path}", path);
		}

		Texture2D texture = ResourceLoader.Load<Texture2D>(path)
			?? throw new InvalidDataException($"Peak custom card {role} is not a Texture2D: {path}");
		return texture;
	}
}
