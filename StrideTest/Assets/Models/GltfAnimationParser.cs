// Copyright (c) Stride contributors (https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace StrideTest.Assets.Models
{
	using SharpGLTF.Schema2;
	using Stride.Animations;
	using Stride.Core.Mathematics;
	using Stride.Rendering;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AnimationChannel = SharpGLTF.Schema2.AnimationChannel;

	public static class GltfAnimationParser
	{
		/// <summary>
		/// Converts GLTF Skeleton into a Stride Skeleton
		/// </summary>
		/// <param name="root"></param>
		/// <returns>skeleton</returns>
		public static Skeleton ConvertSkeleton(ModelRoot root)
		{
			Skeleton result = new();
			var nodes = new List<ModelNodeDefinition>();
			var skins = root.LogicalSkins;

			//var skin = root.LogicalNodes.First(x => x.Mesh == root.LogicalMeshes.First()).Skin;
			// If there is no corresponding skins return a skeleton with 2 bones (an empty skeleton would make the editor crash)
			foreach (var skin in skins)
			{
				var jointList = Enumerable.Range(0, skin.JointsCount).Select(x => skin.GetJoint(x).Joint).ToList();

				nodes.AddRange(
					jointList.Select(
						x => new ModelNodeDefinition
						{
							Name = x.Name ?? "Joint_" + x.LogicalIndex,
							Flags = ModelNodeFlags.Default,
							ParentIndex = jointList.IndexOf(x.VisualParent) + 1,
							Transform = new()
							{
								Position = GltfUtils.ConvertNumerics(x.LocalTransform.Translation),
								Rotation = GltfUtils.ConvertNumerics(x.LocalTransform.Rotation),
								Scale = GltfUtils.ConvertNumerics(x.LocalTransform.Scale)
							}
						}
					)
				);

				// And insert a parent one not caught by the above function (GLTF does not consider the parent bone as a bone)
			}

			nodes.Insert(
				0,
				new()
				{
					Name = "Armature",
					Flags = ModelNodeFlags.EnableRender,
					ParentIndex = -1,
					Transform = new() { Position = Vector3.Zero, Rotation = Quaternion.Identity, Scale = Vector3.Zero }
				}
			);

			result.Nodes = nodes.ToArray();

			return result;
		}

		/// <summary>
		/// Helper function to create a keyframe from values
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="keyTime"></param>
		/// <param name="value"></param>
		/// <returns>keyframe</returns>
		public static KeyFrameData<T> CreateKeyFrame<T>(float keyTime, T value)
		{
			return new((CompressedTimeSpan)TimeSpan.FromSeconds(keyTime), value);
		}

		/// <summary>
		/// Convert a GLTF animation channel into a Stride AnimationCurve.
		/// If the model has no skin, root motion should be enabled
		/// </summary>
		/// <param name="channels"></param>
		/// <param name="root"></param>
		/// <returns>animationCurves</returns>
		public static Dictionary<string, AnimationCurve> ConvertCurves(IReadOnlyList<AnimationChannel> channels, ModelRoot root)
		{
			var result = new Dictionary<string, AnimationCurve>();

			if (root.LogicalAnimations.Count == 0)
				return result;

			var skins = root.LogicalSkins;
			var skNodes = GltfAnimationParser.ConvertSkeleton(root).Nodes.ToList();

			//var skin = root.LogicalNodes.First(x => x.Mesh == root.LogicalMeshes.First()).Skin;

			// In case there is no skin joints/bones, animate transform component
			if (skins.Count() == 0)
			{
				string basestring = "[TransformComponent].type";

				foreach (var chan in channels)
				{
					// TODO it is possible there are multiple entries here! So i changed "Add" to "TryAdd"
					switch (chan.TargetNodePath)
					{
						case PropertyPath.translation:
							result.TryAdd(basestring.Replace("type", "Position"), GltfAnimationParser.ConvertCurve(chan.GetTranslationSampler()));

							break;

						case PropertyPath.rotation:
							result.TryAdd(basestring.Replace("type", "Rotation"), GltfAnimationParser.ConvertCurve(chan.GetRotationSampler()));

							break;

						case PropertyPath.scale:
							result.TryAdd(basestring.Replace("type", "Scale"), GltfAnimationParser.ConvertCurve(chan.GetScaleSampler()));

							break;
					}
				}

				return result;
			}

			foreach (var skin in skins)
			{
				var jointList = Enumerable.Range(0, skin.JointsCount).Select(x => skin.GetJoint(x).Joint).ToList();

				foreach (var chan in channels)
				{
					//var index0 = jointList.IndexOf(chan.TargetNode) + 1;
					var index = skNodes.IndexOf(skNodes.First(x => x.Name == chan.TargetNode.Name));

					switch (chan.TargetNodePath)
					{
						case PropertyPath.translation:
							result.Add(
								$"[ModelComponent.Key].Skeleton.NodeTransformations[{index}].Transform.Position",
								GltfAnimationParser.ConvertCurve(chan.GetTranslationSampler())
							);

							break;

						case PropertyPath.rotation:
							result.Add(
								$"[ModelComponent.Key].Skeleton.NodeTransformations[{index}].Transform.Rotation",
								GltfAnimationParser.ConvertCurve(chan.GetRotationSampler())
							);

							break;

						case PropertyPath.scale:
							result.Add(
								$"[ModelComponent.Key].Skeleton.NodeTransformations[{index}].Transform.Scale",
								GltfAnimationParser.ConvertCurve(chan.GetScaleSampler())
							);

							break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Converts a GLTF AnimationSampler into a Stride AnimationCurve
		/// </summary>
		/// <param name="sampler"></param>
		/// <returns></returns>
		public static AnimationCurve<Quaternion> ConvertCurve(IAnimationSampler<System.Numerics.Quaternion> sampler)
		{
			var interpolationType = sampler.InterpolationMode switch
			{
				AnimationInterpolationMode.LINEAR => AnimationCurveInterpolationType.Linear,
				AnimationInterpolationMode.STEP => AnimationCurveInterpolationType.Constant,
				AnimationInterpolationMode.CUBICSPLINE => AnimationCurveInterpolationType.Cubic,
				_ => throw new NotImplementedException()
			};

			var keyframes = interpolationType switch
			{
				AnimationCurveInterpolationType.Constant => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),
				AnimationCurveInterpolationType.Linear => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),

				// Cubic might be broken
				AnimationCurveInterpolationType.Cubic => sampler.GetCubicKeys()
					.Select(x => GltfUtils.ConvertNumerics(x.Value).Select(y => GltfAnimationParser.CreateKeyFrame(x.Key, y)))
					.SelectMany(x => x),
				_ => throw new NotImplementedException()
			};

			return new() { InterpolationType = interpolationType, KeyFrames = new(keyframes) };
		}

		/// <summary>
		/// Converts a GLTF AnimationSampler into a Stride AnimationCurve
		/// </summary>
		/// <param name="sampler"></param>
		/// <returns></returns>
		public static AnimationCurve<Vector3> ConvertCurve(IAnimationSampler<System.Numerics.Vector3> sampler)
		{
			var interpolationType = sampler.InterpolationMode switch
			{
				AnimationInterpolationMode.LINEAR => AnimationCurveInterpolationType.Linear,
				AnimationInterpolationMode.STEP => AnimationCurveInterpolationType.Constant,
				AnimationInterpolationMode.CUBICSPLINE => AnimationCurveInterpolationType.Cubic,
				_ => throw new NotImplementedException()
			};

			var keyframes = interpolationType switch
			{
				AnimationCurveInterpolationType.Constant => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),
				AnimationCurveInterpolationType.Linear => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),

				// TODO : Cubic can be broken
				AnimationCurveInterpolationType.Cubic => sampler.GetCubicKeys()
					.Select(x => GltfUtils.ConvertNumerics(x.Value).Select(y => GltfAnimationParser.CreateKeyFrame(x.Key, y)))
					.SelectMany(x => x),
				_ => throw new NotImplementedException()
			};

			return new() { InterpolationType = interpolationType, KeyFrames = new(keyframes) };
		}

		/// <summary>
		/// Converts a GLTF AnimationSampler into a Stride AnimationCurve
		/// </summary>
		/// <param name="sampler"></param>
		/// <returns></returns>
		public static AnimationCurve<Vector2> ConvertCurve(IAnimationSampler<System.Numerics.Vector2> sampler)
		{
			var interpolationType = sampler.InterpolationMode switch
			{
				AnimationInterpolationMode.LINEAR => AnimationCurveInterpolationType.Linear,
				AnimationInterpolationMode.STEP => AnimationCurveInterpolationType.Constant,
				AnimationInterpolationMode.CUBICSPLINE => AnimationCurveInterpolationType.Cubic,
				_ => throw new NotImplementedException()
			};

			var keyframes = interpolationType switch
			{
				AnimationCurveInterpolationType.Constant => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),
				AnimationCurveInterpolationType.Linear => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),

				// TODO : Cubic can be broken
				AnimationCurveInterpolationType.Cubic => sampler.GetCubicKeys()
					.Select(x => GltfUtils.ConvertNumerics(x.Value).Select(y => GltfAnimationParser.CreateKeyFrame(x.Key, y)))
					.SelectMany(x => x),
				_ => throw new NotImplementedException()
			};

			return new() { InterpolationType = interpolationType, KeyFrames = new(keyframes) };
		}

		/// <summary>
		/// Converts a GLTF AnimationSampler into a Stride AnimationCurve
		/// </summary>
		/// <param name="sampler"></param>
		/// <returns></returns>
		public static AnimationCurve<Vector4> ConvertCurve(IAnimationSampler<System.Numerics.Vector4> sampler)
		{
			var interpolationType = sampler.InterpolationMode switch
			{
				AnimationInterpolationMode.LINEAR => AnimationCurveInterpolationType.Linear,
				AnimationInterpolationMode.STEP => AnimationCurveInterpolationType.Constant,
				AnimationInterpolationMode.CUBICSPLINE => AnimationCurveInterpolationType.Cubic,
				_ => throw new NotImplementedException()
			};

			var keyframes = interpolationType switch
			{
				AnimationCurveInterpolationType.Constant => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),
				AnimationCurveInterpolationType.Linear => sampler.GetLinearKeys()
					.Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, GltfUtils.ConvertNumerics(x.Value))),

				// TODO : Cubic can be broken
				AnimationCurveInterpolationType.Cubic => sampler.GetCubicKeys()
					.Select(x => GltfUtils.ConvertNumerics(x.Value).Select(y => GltfAnimationParser.CreateKeyFrame(x.Key, y)))
					.SelectMany(x => x),
				_ => throw new NotImplementedException()
			};

			return new() { InterpolationType = interpolationType, KeyFrames = new(keyframes) };
		}

		/// <summary>
		/// Converts a GLTF AnimationSampler into a Stride AnimationCurve
		/// </summary>
		/// <param name="sampler"></param>
		/// <returns></returns>
		public static AnimationCurve<float> ConvertCurve(IAnimationSampler<float> sampler)
		{
			var interpolationType = sampler.InterpolationMode switch
			{
				AnimationInterpolationMode.LINEAR => AnimationCurveInterpolationType.Linear,
				AnimationInterpolationMode.STEP => AnimationCurveInterpolationType.Constant,
				AnimationInterpolationMode.CUBICSPLINE => AnimationCurveInterpolationType.Cubic,
				_ => throw new NotImplementedException()
			};

			var keyframes = interpolationType switch
			{
				AnimationCurveInterpolationType.Constant => sampler.GetLinearKeys().Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, x.Value)),
				AnimationCurveInterpolationType.Linear => sampler.GetLinearKeys().Select(x => GltfAnimationParser.CreateKeyFrame(x.Key, x.Value)),
				_ => throw new NotImplementedException()
			};

			return new() { InterpolationType = interpolationType, KeyFrames = new(keyframes) };
		}
	}
}
