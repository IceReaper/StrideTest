namespace StrideTest.Ecs.Components
{
	using Events;
	using SharpGLTF.Geometry;
	using SharpGLTF.Schema2;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using Stride.Graphics;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Buffer = Stride.Graphics.Buffer;
	using Entity = Ecs.Entity;
	using Material = Assets.Materials.Material;
	using Mesh = Stride.Rendering.Mesh;
	using PrimitiveType = Stride.Graphics.PrimitiveType;

	public class GltfModelInfo : SingleComponentInfo<GltfModel>
	{
		public readonly string Material = "";
		public readonly string Model = "";
	}

	public class GltfModel : SingleComponent<GltfModelInfo>, IOnDespawn
	{
		public GltfModel(Entity entity, GltfModelInfo info)
			: base(entity, info)
		{
			// TODO material from gltf ... ?
			// TODO animations
			// TODO bones
			// TODO what do we need to dispose?

			var modelRoot = ModelRoot.ReadGLB(File.OpenRead("Assets/Models/" + info.Model + ".glb"));

			var verticesList = new List<VertexPositionNormalTexture>();

			var addVertex = new Action<IVertexBuilder>(
				vertex =>
				{
					var geometry = vertex.GetGeometry();
					var material = vertex.GetMaterial();

					var position = geometry.GetPosition();
					geometry.TryGetNormal(out var normal);
					var uv = material.GetTexCoord(0);

					verticesList.Add(new(new(position.X, position.Y, position.Z), new(normal.X, normal.Y, normal.Z), new(uv.X, uv.Y)));
				}
			);

			foreach (var triangle in modelRoot.DefaultScene.EvaluateTriangles())
			{
				addVertex(triangle.A);
				addVertex(triangle.C);
				addVertex(triangle.B);
			}

			var vertices = verticesList.ToArray();
			var indices = Enumerable.Range(0, vertices.Length).ToArray();

			var boundingBox = BoundingBox.FromPoints(vertices.Select(v => v.Position).ToArray());
			var boundingSphere = BoundingSphere.FromBox(boundingBox);

			var vertexTransformResult = VertexHelper.GenerateMultiTextureCoordinates(
				VertexHelper.GenerateTangentBinormal(VertexPositionNormalTexture.Layout, vertices, indices)
			);

			entity.StrideEntity.Add(
				new ModelComponent(
					new()
					{
						BoundingBox = boundingBox,
						BoundingSphere = boundingSphere,
						Meshes =
						{
							new Mesh
							{
								Draw = new()
								{
									IndexBuffer =
										new(Buffer.Index.New(entity.World.AssetManager.GraphicsDevice, indices), true, indices.Length),
									VertexBuffers = new[]
									{
										new VertexBufferBinding(
											Buffer.New(
												entity.World.AssetManager.GraphicsDevice,
												vertexTransformResult.VertexBuffer,
												BufferFlags.VertexBuffer
											),
											vertexTransformResult.Layout,
											vertices.Length
										)
									},
									DrawCount = indices.Length,
									PrimitiveType = PrimitiveType.TriangleList
								},
								BoundingBox = boundingBox,
								BoundingSphere = boundingSphere
							}
						},
						Materials = { new(entity.World.AssetManager.Load<Material>(info.Material, this)) }
					}
				)
			);
		}

		void IOnDespawn.OnDespawn()
		{
			this.Entity.World.AssetManager.Dispose(this);
		}
	}
}
