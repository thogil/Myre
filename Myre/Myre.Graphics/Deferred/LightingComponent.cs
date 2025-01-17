﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Myre.Extensions;
using Myre.Graphics.Deferred.LightManagers;
using System.Collections.ObjectModel;

namespace Myre.Graphics.Deferred
{
    public class LightingComponent
       : RendererComponent
    {
        Quad quad;
        Material restoreDepth;
        Material copyTexture;
        ReadOnlyCollection<IDirectLight> directLights;
        ReadOnlyCollection<IIndirectLight> indirectLights;

        RenderTarget2D directLightBuffer;
        RenderTarget2D indirectLightBuffer;

        public LightingComponent(GraphicsDevice device)
        {
            quad = new Quad(device);
            quad.SetPosition(depth: 0.99999f);

            restoreDepth = new Material(Content.Load<Effect>("RestoreDepth"));
            copyTexture = new Material(Content.Load<Effect>("CopyTexture"));
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // create debug settings
            var settings = renderer.Settings;

            // define inputs
            context.DefineInput("gbuffer_depth");
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");
            //context.DefineInput("gbuffer_depth_downsample");

            if (context.AvailableResources.Any(r => r.Name == "ssao"))
                context.DefineInput("ssao");

            // define outputs
            context.DefineOutput("lightbuffer", isLeftSet:true, surfaceFormat:SurfaceFormat.HdrBlendable, depthFormat:DepthFormat.Depth24Stencil8);
            context.DefineOutput("directlighting", isLeftSet:false, surfaceFormat: SurfaceFormat.HdrBlendable, depthFormat: DepthFormat.Depth24Stencil8);

            // define default light managers
            var scene = renderer.Scene;
            scene.GetManager<DeferredAmbientLightManager>();
            scene.GetManager<DeferredPointLightManager>();
            scene.GetManager<DeferredSkyboxManager>();
            scene.GetManager<DeferredSpotLightManager>();
            scene.GetManager<DeferredSunLightManager>();

            // get lights
            directLights = scene.FindManagers<IDirectLight>();
            indirectLights = scene.FindManagers<IIndirectLight>();

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.Get<Vector2>("resolution").Value;
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            // prepare direct lights
            for (int i = 0; i < directLights.Count; i++)
                directLights[i].Prepare(renderer);

            // set and clear direct light buffer
            directLightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            device.SetRenderTarget(directLightBuffer);
            device.Clear(Color.Transparent);

            // work around for a bug in xna 4.0
            renderer.Device.SamplerStates[0] = SamplerState.LinearClamp;
            renderer.Device.SamplerStates[0] = SamplerState.PointClamp;

            // set render states to draw opaque geometry
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            // restore depth
            quad.Draw(restoreDepth, metadata);
            
            // set render states to additive blend
            device.BlendState = BlendState.Additive;

            // draw direct lights
            for (int i = 0; i < directLights.Count; i++)
                directLights[i].Draw(renderer);

            // output direct lighting
            Output("directlighting", directLightBuffer);

            // prepare indirect lights
            for (int i = 0; i < indirectLights.Count; i++)
                indirectLights[i].Prepare(renderer);

            // set and clear indirect light buffer
            indirectLightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            device.SetRenderTarget(indirectLightBuffer);
            device.Clear(Color.Transparent);

            //draw indirect lights
            for (int i = 0; i < indirectLights.Count; i++)
                indirectLights[i].Draw(renderer);

            // blend direct lighting into the indirect light buffer
            copyTexture.Parameters["Texture"].SetValue(directLightBuffer);
            quad.Draw(copyTexture, metadata);

            // output resulting light buffer
            Output("lightbuffer", indirectLightBuffer);
        }
    }
}
