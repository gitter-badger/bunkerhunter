﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Flakcore.Display.ParticleEngine.EmitterData;

namespace Flakcore.Display.ParticleEngine
{
    public class ParticleEngine : Node
    {
        public Vector2 EmitterPosition = Vector2.Zero;

        private ParticleEffect Effect;
        private BasicEmitter[] Emitters;
        private bool Started;

        public ParticleEngine(ParticleEffect Effect)
        {
            this.Effect = Effect;
            this.SetupEmitters();
            this.Started = false;
        }

        private void SetupEmitters()
        {
            // For each emitterData in our effect, get the Emitter and add it to our variable
            this.Emitters = new BasicEmitter[this.Effect.EmitterData.Length];

            for (int i = 0; i < this.Effect.EmitterData.Length; i++)
            {
                this.Emitters[i] = this.Effect.EmitterData[i].SetupEmitter();
                this.AddChild(this.Emitters[i]);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!this.Started)
                return;

            this.UpdateEmitterPositions();

        }

        public void Start()
        {
            this.Started = true;
            this.UpdateEmitterPositions();
            this.StartEmitters();
        }

        public void Stop()
        {
            this.Started = false;
            this.StopEmitters();
        }

        public void Explode()
        {
            this.UpdateEmitterPositions();
            this.ExplodeEmitters();
        }

        private void StartEmitters()
        {
            foreach (BasicEmitter emitter in this.Emitters)
                emitter.Start();
        }

        private void StopEmitters()
        {
            foreach (BasicEmitter emitter in this.Emitters)
                emitter.Stop();
        }

        private void ExplodeEmitters()
        {
            foreach (BasicEmitter emitter in this.Emitters)
                emitter.Explode();
        }

        private void UpdateEmitterPositions()
        {
            foreach (BasicEmitter emitter in this.Emitters)
                emitter.Position = this.Position;
        }
    }

}
