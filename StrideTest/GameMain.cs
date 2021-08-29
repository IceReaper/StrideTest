namespace StrideTest
{
	using SharpFont;
	using Stride.Animations;
	using Stride.Core.Collections;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using Stride.Engine.Processors;
	using Stride.Games;
	using Stride.Graphics;
	using Stride.Graphics.Font;
	using Stride.Graphics.GeometricPrimitives;
	using Stride.Particles;
	using Stride.Particles.BoundingShapes;
	using Stride.Particles.Components;
	using Stride.Particles.Initializers;
	using Stride.Particles.Materials;
	using Stride.Particles.ShapeBuilders;
	using Stride.Particles.Spawners;
	using Stride.Rendering;
	using Stride.Rendering.Colors;
	using Stride.Rendering.Lights;
	using Stride.Rendering.Materials;
	using Stride.Rendering.Materials.ComputeColors;
	using Stride.Rendering.ProceduralModels;
	using Stride.Rendering.UI;
	using Stride.UI;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	public class GameMain : Game
	{
		private Entity? movingPointLight;
		private UIRenderFeature? uiRenderFeature;
		private UIElement? uiElementUnderMouseCursor;

		// TODO this is a hack for custom font resolution
		private SpriteFont FontNewDynamic(
			float defaultSize,
			string fontName,
			FontStyle style,
			FontAntiAliasMode antiAliasMode = FontAntiAliasMode.Default,
			bool useKerning = false,
			float extraSpacing = 0f,
			float extraLineSpacing = 0f,
			char defaultCharacter = ' '
		)
		{
			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var fontManager = ((FontSystem)this.Font).GetType().GetProperty("FontManager", flags)?.GetValue(this.Font);

			if (fontManager?.GetType().GetField("cachedFontFaces", flags)?.GetValue(fontManager) is not Dictionary<string, Face> cachedFontFaces
				|| fontManager.GetType().GetField("freetypeLibrary", flags)?.GetValue(fontManager) is not Library freetypeLibrary)
				return this.Font.NewDynamic(defaultSize, fontName, style, antiAliasMode, useKerning, extraSpacing, extraLineSpacing, defaultCharacter);

			using var fontStream = File.OpenRead($"Assets/Fonts/{fontName}.ttf");
			var newFontData = new byte[fontStream.Length];
			fontStream.Read(newFontData, 0, newFontData.Length);

			lock (freetypeLibrary)
				cachedFontFaces[FontHelper.GetFontPath(fontName, style)] = freetypeLibrary.NewMemoryFace(newFontData, 0);

			return this.Font.NewDynamic(defaultSize, fontName, style, antiAliasMode, useKerning, extraSpacing, extraLineSpacing, defaultCharacter);
		}

		protected override void BeginRun()
		{
			this.Window.AllowUserResizing = true;

			var scene = new Scene();
			this.SceneSystem.GraphicsCompositor = GraphicsCompositorBuilder.Create();
			this.SceneSystem.SceneInstance = new SceneInstance(this.Services, scene);

			var texture = Texture.Load(this.GraphicsDevice, File.OpenRead("Assets/Textures/Dummy.png"));
			var font = this.FontNewDynamic(20, "Roboto", FontStyle.Regular);

			// Camera
			var cameraComponent = new CameraComponent
			{
				Projection = CameraProjectionMode.Orthographic, Slot = this.SceneSystem.GraphicsCompositor.Cameras[0].ToSlotId()
			};

			var cameraEntity = new Entity { cameraComponent };
			var cameraTransform = cameraEntity.Get<TransformComponent>();
			cameraTransform.Position = new Vector3(-6, 6, 6);
			cameraTransform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(-45), MathUtil.DegreesToRadians(-30), 0);
			scene.Entities.Add(cameraEntity);

			// Ground
			var groundModel = new Model();

			new PlaneProceduralModel
			{
				Size = new Vector2(10, 10),
				Normal = NormalDirection.UpY,
				MaterialInstance =
				{
					Material = Material.New(
						this.GraphicsDevice,
						new MaterialDescriptor
						{
							Attributes = new MaterialAttributes
							{
								MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = new ComputeFloat(0.65f) },
								Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(new Color4(0.5f, 0.5f, 0.5f))),
								DiffuseModel = new MaterialDiffuseLambertModelFeature(),
								Specular = new MaterialMetalnessMapFeature { MetalnessMap = new ComputeFloat(1.0f) },
								SpecularModel = new MaterialSpecularMicrofacetModelFeature
								{
									Fresnel = new MaterialSpecularMicrofacetFresnelSchlick(),
									Visibility = new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX(),
									NormalDistribution = new MaterialSpecularMicrofacetNormalDistributionGGX(),
									Environment = new MaterialSpecularMicrofacetEnvironmentGGXLUT()
								}
							}
						}
					)
				}
			}.Generate(this.Services, groundModel);

			var groundEntity = new Entity { new ModelComponent(groundModel) };
			scene.Entities.Add(groundEntity);

			// Teapot
			var teapotModel = new Model();

			new TeapotProceduralModel
			{
				MaterialInstance =
				{
					Material = Material.New(
						this.GraphicsDevice,
						new MaterialDescriptor
						{
							Attributes = new MaterialAttributes
							{
								MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = new ComputeFloat(0.65f) },
								Diffuse = new MaterialDiffuseMapFeature(new ComputeTextureColor(texture)),
								DiffuseModel = new MaterialDiffuseLambertModelFeature(),
								Specular = new MaterialMetalnessMapFeature { MetalnessMap = new ComputeFloat(1.0f) },
								SpecularModel = new MaterialSpecularMicrofacetModelFeature
								{
									Fresnel = new MaterialSpecularMicrofacetFresnelSchlick(),
									Visibility = new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX(),
									NormalDistribution = new MaterialSpecularMicrofacetNormalDistributionGGX(),
									Environment = new MaterialSpecularMicrofacetEnvironmentGGXLUT()
								}
							}
						}
					)
				}
			}.Generate(this.Services, teapotModel);

			var teapotEntity = new Entity { new ModelComponent(teapotModel) };
			teapotEntity.GetOrCreate<TransformComponent>().Position = new Vector3(0, 1, 0);
			scene.Entities.Add(teapotEntity);

			// Particles
			var particlesEntity = new Entity
			{
				new ParticleSystemComponent
				{
					ParticleSystem =
					{
						BoundingShape = new BoundingBoxStatic(),
						Emitters =
						{
							new ParticleEmitter
							{
								ParticleLifetime = new Vector2(1, 1),
								ShapeBuilder =
									new ShapeBuilderBillboard
									{
										SamplerSize = new ComputeCurveSamplerFloat
										{
											Curve = new ComputeAnimationCurveFloat
											{
												KeyFrames = new TrackingCollection<AnimationKeyFrame<float>>
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
									new InitialSizeSeed { DisplayParticleScaleUniform = true, RandomSize = new Vector2(0.1f, 0.1f) },
									new InitialVelocitySeed
									{
										DisplayParticleRotation = true,
										DisplayParticleScaleUniform = true,
										VelocityMin = new Vector3(-3, 3, -3),
										VelocityMax = new Vector3(3, 3, 3)
									}
								}
							}
						}
					}
				}
			};

			particlesEntity.GetOrCreate<TransformComponent>().Position = new Vector3(0, 2, 0);
			scene.Entities.Add(particlesEntity);

			// Lighting
			var sunEntity = new Entity
			{
				new LightComponent
				{
					Type = new LightDirectional
					{
						Color = new ColorRgbProvider(Color.White),
						Shadow =
						{
							Enabled = true,
							Size = LightShadowMapSize.XLarge,
							Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter7x7 }
						}
					},
					Intensity = .5f
				}
			};

			sunEntity.GetOrCreate<TransformComponent>().Rotation = Quaternion.RotationYawPitchRoll(0, MathUtil.DegreesToRadians(-90), 0);
			scene.Entities.Add(sunEntity);

			this.movingPointLight = new Entity
			{
				new LightComponent
				{
					Type = new LightPoint
					{
						Color = new ColorRgbProvider(Color.Red),
						Radius = 100,

						// TODO seems to not work
						Shadow =
						{
							Enabled = true,
							Size = LightShadowMapSize.XLarge,
							Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter7x7 }
						}
					},
					Intensity = 1
				},
				new UIComponent
				{
					Size = new Vector3(2, .5f, 1),
					Resolution = new Vector3(200, 50, 1),
					IsFullScreen = false,
					IsBillboard = true,
					Page = new UIPage { RootElement = new TestButton(font) }
				}
			};

			scene.Entities.Add(this.movingPointLight);

			// TODO seems to not work
			scene.Entities.Add(new Entity { new LightComponent { Type = new LightAmbient { Color = new ColorRgbProvider(Color.White) }, Intensity = 1 } });
		}

		protected override void Update(GameTime gameTime)
		{
			var time = (float)gameTime.Total.TotalSeconds;

			if (this.movingPointLight != null)
				this.movingPointLight.Transform.Position = (Matrix.Translation(0, 3, 3) * Matrix.RotationY(time)).TranslationVector;

			base.Update(gameTime);
		}

		// TODO this is a hack to get around the ui being unaware of the mouse leaving it 
		protected override bool BeginDraw()
		{
			this.uiRenderFeature ??= this.SceneSystem.GraphicsCompositor.RenderFeatures.OfType<UIRenderFeature>().FirstOrDefault();
			this.uiElementUnderMouseCursor = this.uiRenderFeature?.UIElementUnderMouseCursor;

			return base.BeginDraw();
		}

		protected override void EndDraw(bool present)
		{
			if (this.uiElementUnderMouseCursor != null && this.uiRenderFeature?.UIElementUnderMouseCursor == null)
				this.uiElementUnderMouseCursor?.GetType().GetProperty("MouseOverState")?.SetValue(this.uiElementUnderMouseCursor, MouseOverState.MouseOverNone);
			
			base.EndDraw(present);
		}
	}
}
