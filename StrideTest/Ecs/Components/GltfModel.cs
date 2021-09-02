namespace StrideTest.Ecs.Components
{
	using Assets.Models;
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
			// TODO we need the code to be able to determine the absolute bone transform in every frame
			// TODO what do we need to dispose?

			var modelRoot = ModelRoot.ReadGLB(File.OpenRead("Base/Models/" + info.Model + ".glb"));

			// New approach - does not render the model.
			var model = GltfMeshParser.LoadFirstModel(modelRoot);
			model.Materials.Add(new(entity.World.AssetManager.Load<Material>(info.Material, this)));
			var animations = GltfMeshParser.ConvertAnimations(modelRoot, info.Model);
			
			var animationComponent = new AnimationComponent();
			this.Entity.StrideEntity.Add(new ModelComponent(model));
			this.Entity.StrideEntity.Add(animationComponent);
			
			foreach (var (name, animation) in animations)
				animationComponent.Animations.Add(name, animation);

			animationComponent.Play(animations.Keys.First());

			// Old approach - does not support animations.
			/*var verticesList = new List<VertexPositionNormalTexture>();

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
			);*/
		}

		void IOnDespawn.OnDespawn()
		{
			this.Entity.World.AssetManager.Dispose(this);
		}
	}
}
