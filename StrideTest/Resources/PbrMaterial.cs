namespace StrideTest.Resources
{
	using Stride.Core.Mathematics;
	using Stride.Graphics;
	using Stride.Rendering;
	using Stride.Rendering.Materials;
	using Stride.Rendering.Materials.ComputeColors;
	using System;
	using System.Collections.Generic;

	public class PbrMaterial : Material, IDisposable
	{
		private readonly AssetManager assetManager;

		private PbrMaterial(AssetManager assetManager)
		{
			this.assetManager = assetManager;
		}

		public static PbrMaterial Load(AssetManager assetManager, Dictionary<string, YamlNode> yaml)
		{
			var material = new PbrMaterial(assetManager);

			var attributes = new MaterialAttributes
			{
				DiffuseModel = new MaterialDiffuseLambertModelFeature(), SpecularModel = new MaterialSpecularMicrofacetModelFeature()
			};

			if (PbrMaterial.GetColorMap(material, yaml, "DiffuseMap", out var diffuseMap))
				attributes.Diffuse = new MaterialDiffuseMapFeature { DiffuseMap = diffuseMap };
			else if (PbrMaterial.GetColorValue(yaml, "DiffuseColor", out var diffuseColor))
				attributes.Diffuse = new MaterialDiffuseMapFeature { DiffuseMap = diffuseColor };

			if (PbrMaterial.GetScalarMap(material, yaml, "MetalnessMap", out var metalnessMap))
				attributes.Specular = new MaterialMetalnessMapFeature { MetalnessMap = metalnessMap };
			else if (PbrMaterial.GetScalarValue(yaml, "Metalness", out var metalness))
				attributes.Specular = new MaterialMetalnessMapFeature { MetalnessMap = metalness };

			if (PbrMaterial.GetScalarMap(material, yaml, "DisplacementMap", out var displacementMap))
			{
				attributes.Displacement = new MaterialDisplacementMapFeature
				{
					DisplacementMap = displacementMap,
					Intensity = PbrMaterial.GetScalarValue(yaml, "DisplacementIntensity", out var displacementIntensity)
						? displacementIntensity
						: new ComputeFloat(1)
				};
			}

			if (PbrMaterial.GetColorMap(material, yaml, "NormalMap", out var normalMap))
				attributes.Surface = new MaterialNormalMapFeature { NormalMap = normalMap, ScaleAndBias = true, IsXYNormal = false };

			if (PbrMaterial.GetColorMap(material, yaml, "EmissiveMap", out var emissiveMap))
			{
				attributes.Emissive = new MaterialEmissiveMapFeature
				{
					EmissiveMap = emissiveMap,
					Intensity = PbrMaterial.GetScalarValue(yaml, "EmissiveIntensity", out var emissiveIntensity) ? emissiveIntensity : new ComputeFloat(1),
					UseAlpha = false
				};
			}
			else if (PbrMaterial.GetColorValue(yaml, "EmissiveColor", out var emissiveColor))
			{
				attributes.Emissive = new MaterialEmissiveMapFeature
				{
					EmissiveMap = emissiveColor,
					Intensity = PbrMaterial.GetScalarValue(yaml, "EmissiveIntensity", out var emissiveIntensity) ? emissiveIntensity : new ComputeFloat(1),
					UseAlpha = false
				};
			}

			if (PbrMaterial.GetScalarMap(material, yaml, "GlossinessMap", out var glossinessMap))
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = glossinessMap, Invert = false };
			else if (PbrMaterial.GetScalarMap(material, yaml, "RoughnessMap", out var roughnessMap))
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = roughnessMap, Invert = true };
			else if (PbrMaterial.GetScalarValue(yaml, "Glossiness", out var glossiness))
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = glossiness, Invert = false };
			else if (PbrMaterial.GetScalarValue(yaml, "Roughness", out var roughness))
				attributes.MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = roughness, Invert = true };

			var context = new MaterialGeneratorContext(material, assetManager.GraphicsDevice)
			{
				GraphicsProfile = assetManager.GraphicsDevice.Features.RequestedProfile
			};

			var descriptor = new MaterialDescriptor { Attributes = attributes };
			var result = MaterialGenerator.Generate(descriptor, context, $"{descriptor.MaterialId}:RuntimeMaterial");

			if (result.HasErrors)
				throw new InvalidOperationException($"Error when creating the material [{result.ToText()}]");

			return material;
		}

		private static bool GetColorMap(PbrMaterial material, IReadOnlyDictionary<string, YamlNode> yaml, string key, out IComputeColor? result)
		{
			if (!yaml.TryGetValue(key, out var node) || node.Value == null)
			{
				result = null;

				return false;
			}

			if (!AssetManager.Exists<Texture>(node.Value))
			{
				result = null;

				return false;
			}

			result = new ComputeTextureColor(material.assetManager.Load<Texture>(node.Value, material));

			return true;
		}

		private static bool GetColorValue(IReadOnlyDictionary<string, YamlNode> yaml, string key, out IComputeColor? result)
		{
			if (!yaml.TryGetValue(key, out var node) || node.Value == null || !Serializer.TryParse<Color>(node.Value, out var color))
			{
				result = null;

				return false;
			}

			result = new ComputeColor(color);

			return true;
		}

		private static bool GetScalarMap(PbrMaterial material, IReadOnlyDictionary<string, YamlNode> yaml, string key, out IComputeScalar? result)
		{
			if (!yaml.TryGetValue(key, out var node) || node.Value == null)
			{
				result = null;

				return false;
			}

			if (!AssetManager.Exists<Texture>(node.Value))
			{
				result = null;

				return false;
			}

			result = new ComputeTextureScalar(
				material.assetManager.Load<Texture>(node.Value, material),
				TextureCoordinate.Texcoord0,
				Vector2.One,
				Vector2.Zero
			);

			return true;
		}

		private static bool GetScalarValue(IReadOnlyDictionary<string, YamlNode> yaml, string key, out IComputeScalar? result)
		{
			if (!yaml.TryGetValue(key, out var node) || node.Value == null || !Serializer.TryParse<float>(node.Value, out var color))
			{
				result = null;

				return false;
			}

			result = new ComputeFloat(color);

			return true;
		}

		public void Dispose()
		{
			this.assetManager.Dispose(this);

			GC.SuppressFinalize(this);
		}
	}
}
