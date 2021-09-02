// Copyright (c) Stride contributors (https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace StrideTest.Assets.Models
{
	using SharpGLTF.Schema2;
	using Stride.Animations;
	using Stride.Extensions;
	using Stride.Graphics;
	using Stride.Graphics.Data;
	using Stride.Rendering;
	using Stride.Rendering.Materials;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Mesh = Stride.Rendering.Mesh;

	public static class GltfMeshParser
	{
		/// <summary>
		/// Converts the first mesh in the GLTF file into a stride Model 
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static Model LoadFirstModel(ModelRoot root)
		{
			// We load every primitives of the first mesh
			var sk = GltfAnimationParser.ConvertSkeleton(root);

			var draws = root.LogicalMeshes.Select(
					x => (x.Primitives.Select(x => GltfMeshParser.LoadMesh(x, sk)).ToList(), GltfUtils.ConvertNumerics(x.VisualParents.First().WorldMatrix))
				)
				.ToList();

			for (var i = 0; i < draws.Count; i++)
			{
				var mat = draws[i].Item2;

				for (var j = 0; j < draws[i].Item1.Count; j++)
				{
					for (var k = 0; k < draws[i].Item1[j].Draw.VertexBuffers.Count(); k++)
						draws[i].Item1[j].Draw.VertexBuffers[k].TransformBuffer(ref mat);
				}
			}

			var result = new Model() { Meshes = draws.SelectMany(x => x.Item1).ToList() };
			result.Skeleton = sk;

			return result;
		}

		/// <summary>
		/// Gets the sum of all animation duration
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static TimeSpan GetAnimationDuration(ModelRoot root)
		{
			var time = root.LogicalAnimations.Select(x => x.Duration).Sum();

			return TimeSpan.FromSeconds(time);
		}

		/// <summary>
		/// Converts GLTF joints into MeshSkinningDefinition, defining the Mesh to World matrix useful for skinning and animations.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static MeshSkinningDefinition ConvertInverseBindMatrices(ModelRoot root, Skeleton sk)
		{
			var skin = root.LogicalNodes.First(x => x.Mesh == root.LogicalMeshes[0]).Skin;

			if (skin == null)
				return null;

			var jointList = Enumerable.Range(0, skin.JointsCount).Select(skin.GetJoint);
			var nodeList = sk.Nodes.ToList();

			var mnt = new MeshSkinningDefinition
			{
				Bones = jointList.Select(
						x => new MeshBoneDefinition
						{
							NodeIndex = nodeList.IndexOf(nodeList.First(n => n.Name == x.Joint.Name)),
							LinkToMeshMatrix = GltfUtils.ConvertNumerics(x.InverseBindMatrix)
						}
					)
					.ToArray()
			};

			return mnt;
		}

		/// <summary>
		/// Converts GLTF animations into Stride AnimationClips.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static Dictionary<string, AnimationClip> ConvertAnimations(ModelRoot root, string filename)
		{
			var animations = root.LogicalAnimations;
			var meshName = filename;

			var clips = animations.Select(
					x =>
					{
						//Create animation clip with 
						var clip = new AnimationClip { Duration = TimeSpan.FromSeconds(x.Duration) };
						clip.RepeatMode = AnimationRepeatMode.LoopInfinite;

						// Add Curve
						GltfAnimationParser.ConvertCurves(x.Channels, root).ToList().ForEach(v => clip.AddCurve(v.Key, v.Value));
						string name = x.Name == null ? meshName + "_Animation_" + x.LogicalIndex : meshName + "_" + x.Name;

						if (clip.Curves.Count > 1)
							clip.Optimize();

						return (name, clip);
					}
				)
				.ToList()
				.ToDictionary(x => x.name, x => x.clip);

			return clips;
		}

		/// <summary>
		/// Convert a GLTF Primitive into a Stride Mesh
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public static Mesh LoadMesh(MeshPrimitive mesh, Skeleton sk)
		{
			var draw = new MeshDraw
			{
				PrimitiveType = GltfUtils.ConvertPrimitiveType(mesh.DrawPrimitiveType),
				IndexBuffer = GltfMeshParser.ConvertSerializedIndexBufferBinding(mesh),
				VertexBuffers = GltfMeshParser.ConvertSerializedVertexBufferBinding(mesh),
				DrawCount = GltfMeshParser.GetDrawCount(mesh)
			};

			var result = new Mesh(draw, new())
			{
				Skinning = GltfMeshParser.ConvertInverseBindMatrices(mesh.LogicalParent.LogicalParent, sk),
				Name = mesh.LogicalParent.Name,
				MaterialIndex = mesh.LogicalParent.LogicalParent.LogicalMaterials.ToList().IndexOf(mesh.Material)
			};

			// TODO : Add parameter collection only after checking if it has
			result.Parameters.Set(MaterialKeys.HasSkinningPosition, true);
			result.Parameters.Set(MaterialKeys.HasSkinningNormal, true);

			return result;
		}

		/// <summary>
		/// Gets the number of triangle indices.
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		private static int GetDrawCount(MeshPrimitive mesh)
		{
			// TODO : Check if every meshes has triangle indices
			return mesh.GetTriangleIndices().Select(x => new int[] { x.A, x.B, x.C }).SelectMany(x => x).Select(x => (uint)x).ToArray().Length;
		}

		/// <summary>
		/// Converts an index buffer into a serialized index buffer binding for asset creation.
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public static IndexBufferBinding ConvertSerializedIndexBufferBinding(MeshPrimitive mesh)
		{
			var indices = mesh.GetTriangleIndices()
				.Select(x => new int[] { x.A, x.C, x.B })
				.SelectMany(x => x)
				.Select(x => (uint)x)
				.Select(BitConverter.GetBytes)
				.SelectMany(x => x)
				.ToArray();

			var buf = GraphicsSerializerExtensions.ToSerializableVersion(new BufferData(BufferFlags.IndexBuffer, indices));

			return new(buf, true, indices.Length);
		}

		/// <summary>
		/// Converts a vertex buffer into a serialized vertex buffer for asset creation.
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public static VertexBufferBinding[] ConvertSerializedVertexBufferBinding(MeshPrimitive mesh)
		{
			var offset = 0;

			var vertElem = mesh.VertexAccessors.Select(
					x =>
					{
						var y = GltfUtils.ConvertVertexElement(x, offset);
						offset += y.Item2;

						return y.Item1;
					}
				)
				.ToList();

			var declaration = new VertexDeclaration(vertElem.ToArray());

			var size = mesh.VertexAccessors.First().Value.Count;

			List<byte> bytelst = new();

			for (var i = 0; i < size; i++)
			{
				foreach (var e in mesh.VertexAccessors.Keys)
				{
					var bytes = mesh.GetVertexAccessor(e).TryGetVertexBytes(i).ToArray();
					bytelst.AddRange(bytes);
				}
			}

			var byteBuffer = bytelst.ToArray();

			var buffer = GraphicsSerializerExtensions.ToSerializableVersion(new BufferData(BufferFlags.VertexBuffer, byteBuffer));
			var binding = new VertexBufferBinding(buffer, declaration, size);

			return new List<VertexBufferBinding>() { binding }.ToArray();
		}
	}
}
