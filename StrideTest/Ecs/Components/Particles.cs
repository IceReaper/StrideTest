namespace StrideTest.Ecs.Components
{
	using Stride.Animations;
	using Stride.Core.Mathematics;
	using Stride.Particles;
	using Stride.Particles.BoundingShapes;
	using Stride.Particles.Components;
	using Stride.Particles.Initializers;
	using Stride.Particles.Materials;
	using Stride.Particles.ShapeBuilders;
	using Stride.Particles.Spawners;
	using Stride.Rendering.Materials.ComputeColors;

	public class ParticlesInfo : MultipleComponentInfo<Particles>
	{
	}

	public class Particles : MultipleComponent<ParticlesInfo>
	{
		public Particles(Actor actor, ParticlesInfo info)
			: base(actor, info)
		{
			actor.Entity.Components.Add(
				new ParticleSystemComponent
				{
					ParticleSystem =
					{
						BoundingShape = new BoundingBoxStatic(),
						Emitters =
						{
							new ParticleEmitter
							{
								ParticleLifetime = new(1, 1),
								ShapeBuilder =
									new ShapeBuilderBillboard
									{
										SamplerSize = new ComputeCurveSamplerFloat
										{
											Curve = new ComputeAnimationCurveFloat
											{
												KeyFrames = new()
												{
													new() { Key = 0.0f, Value = 0.0f },
													new() { Key = 0.2f, Value = 1.0f },
													new() { Key = 1.0f, Value = 0.1f }
												}
											}
										}
									},
								Material = new ParticleMaterialComputeColor { ComputeColor = new ComputeColor { Value = Color4.White } },
								Spawners = { new SpawnerPerSecond { Duration = Vector2.One, SpawnCount = 100 } },
								Initializers =
								{
									new InitialSizeSeed { DisplayParticleScaleUniform = true, RandomSize = new(0.1f, 0.1f) },
									new InitialVelocitySeed
									{
										DisplayParticleRotation = true,
										DisplayParticleScaleUniform = true,
										VelocityMin = new(-3, 3, -3),
										VelocityMax = new(3, 3, 3)
									}
								}
							}
						}
					}
				}
			);
		}
	}
}
