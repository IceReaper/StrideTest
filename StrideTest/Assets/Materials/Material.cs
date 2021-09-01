namespace StrideTest.Assets.Materials
{
	using Stride.Core.Mathematics;
	using Stride.Graphics;
	using Stride.Rendering.Materials;
	using Stride.Rendering.Materials.ComputeColors;
	using System;

	public class Material : Stride.Rendering.Material, IDisposable
	{
		private readonly AssetManager assetManager;
		public readonly string Id;

		private Material(AssetManager assetManager, string id)
		{
			this.assetManager = assetManager;
			this.Id = id;
		}

		public static Material? Load(AssetManager assetManager, string id, MaterialInfo materialInfo)
		{
			var material = new Material(assetManager, id);

			var attributes = new MaterialAttributes
			{
				DiffuseModel = new MaterialDiffuseLambertModelFeature(), SpecularModel = new MaterialSpecularMicrofacetModelFeature()
			};

			if (material.GetColorMap(materialInfo.DiffuseMap, out var diffuseMap))
				attributes.Diffuse = new MaterialDiffuseMapFeature { DiffuseMap = diffuseMap };
			else if (materialInfo.DiffuseColor != null)
				attributes.Diffuse = new MaterialDiffuseMapFeature { DiffuseMap = new ComputeColor(materialInfo.DiffuseColor.Value) };

			if (material.GetScalarMap(materialInfo.MetalnessMap, out var metalnessMap))
				attributes.Specular = new MaterialMetalnessMapFeature { MetalnessMap = metalnessMap };
			else if (materialInfo.Metalness != null)
				attributes.Specular = new MaterialMetalnessMapFeature { MetalnessMap = new ComputeFloat(materialInfo.Metalness.Value) };

			if (material.GetScalarMap(materialInfo.DisplacementMap, out var displacementMap))
			{
				attributes.Displacement = new MaterialDisplacementMapFeature
				{
					DisplacementMap = displacementMap, Intensity = new ComputeFloat(materialInfo.DisplacementIntensity ?? 1)
				};
			}

			if (material.GetColorMap(materialInfo.NormalMap, out var normalMap))
				attributes.Surface = new MaterialNormalMapFeature { NormalMap = normalMap, ScaleAndBias = true, IsXYNormal = false };

			if (material.GetColorMap(materialInfo.EmissiveMap, out var emissiveMap))
			{
				attributes.Emissive = new MaterialEmissiveMapFeature
				{
					EmissiveMap = emissiveMap, Intensity = new ComputeFloat(materialInfo.EmissiveIntensity ?? 1), UseAlpha = false
				};
			}
			else if (materialInfo.EmissiveColor != null)
			{
				attributes.Emissive = new MaterialEmissiveMapFeature
				{
					EmissiveMap = new ComputeColor(materialInfo.EmissiveColor.Value),
					Intensity = new ComputeFloat(materialInfo.EmissiveIntensity ?? 1),
					UseAlpha = false
				};
			}

			if (material.GetScalarMap(materialInfo.GlossinessMap, out var glossinessMap))
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = glossinessMap, Invert = false };
			else if (material.GetScalarMap(materialInfo.RoughnessMap, out var roughnessMap))
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = roughnessMap, Invert = true };
			else if (materialInfo.Glossiness != null)
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = new ComputeFloat(materialInfo.Glossiness.Value), Invert = false };
			else if (materialInfo.Roughness != null)
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = new ComputeFloat(materialInfo.Roughness.Value), Invert = true };

			if (material.GetScalarMap(materialInfo.AmbientOcclusionMap, out var ambientOcclusionMap))
				attributes.Occlusion = new MaterialOcclusionMapFeature { AmbientOcclusionMap = ambientOcclusionMap };

			var context = new MaterialGeneratorContext(material, assetManager.GraphicsDevice)
			{
				GraphicsProfile = assetManager.GraphicsDevice.Features.RequestedProfile
			};

			var descriptor = new MaterialDescriptor { Attributes = attributes };
			var result = MaterialGenerator.Generate(descriptor, context, $"{descriptor.MaterialId}:RuntimeMaterial");

			if (!result.HasErrors)
				return material;

			material.Dispose();

			return null;
		}

		private bool GetColorMap(string? textureId, out IComputeColor? result)
		{
			result = null;

			if (textureId == null)
				return false;

			var texture = this.assetManager.Load<Texture>(textureId, this);

			if (texture != null)
				result = new ComputeTextureColor(texture);

			return result != null;
		}

		private bool GetScalarMap(string? textureId, out IComputeScalar? result)
		{
			result = null;

			if (textureId == null)
				return false;

			var texture = this.assetManager.Load<Texture>(textureId, this);

			if (texture != null)
				result = new ComputeTextureScalar(texture, TextureCoordinate.Texcoord0, Vector2.One, Vector2.Zero);

			return result != null;
		}

		public void Dispose()
		{
			this.assetManager.Dispose(this);

			GC.SuppressFinalize(this);
		}
	}
}
