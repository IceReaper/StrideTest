namespace StrideTest.Ecs.Components
{
	using Events;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using Stride.Graphics;
	using Stride.Rendering;
	using System.Linq;
	using Entity = Ecs.Entity;
	using Material = Assets.Materials.Material;

	public class GroundInfo : SingleComponentInfo<Ground>
	{
		public readonly string Material = "";
	}

	public class Ground : SingleComponent<GroundInfo>, IOnDespawn
	{
		public Ground(Entity entity, GroundInfo info)
			: base(entity, info)
		{
			// TODO move this to gltf.
			// TODO what do we need to dispose?

			var size = new Vector2(10, 10);

			var vertices = new VertexPositionNormalTexture[]
			{
				new(new(-.5f * size.X, 0, -.5f * size.Y), Vector3.UnitY, new(0, 0)),
				new(new(0.5f * size.X, 0, -.5f * size.Y), Vector3.UnitY, new(1, 0)),
				new(new(0.5f * size.X, 0, 0.5f * size.Y), Vector3.UnitY, new(1, 1)),
				new(new(-.5f * size.X, 0, 0.5f * size.Y), Vector3.UnitY, new(0, 1))
			};

			var indices = new[] { 0, 1, 2, 2, 3, 0 };

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
