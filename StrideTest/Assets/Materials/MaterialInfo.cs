namespace StrideTest.Assets.Materials
{
	using JetBrains.Annotations;
	using Stride.Core.Mathematics;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class MaterialInfo
	{
		public readonly string? DiffuseMap;
		public readonly Color? DiffuseColor;
		public readonly string? MetalnessMap;
		public readonly float? Metalness;
		public readonly string? DisplacementMap;
		public readonly float? DisplacementIntensity;
		public readonly string? NormalMap;
		public readonly string? EmissiveMap;
		public readonly Color? EmissiveColor;
		public readonly float? EmissiveIntensity;
		public readonly string? GlossinessMap;
		public readonly string? RoughnessMap;
		public readonly float? Glossiness;
		public readonly float? Roughness;
		public readonly string? AmbientOcclusionMap;
	}
}
