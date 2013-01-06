﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bunker_Hunter.GameObjects
{
    public class BulletType
    {
        public string TextureName { get; private set; }
        public Vector2 Speed { get; private set; }
        public Vector2 SpeedChange { get; private set; }
        public float Mass { get; private set; }

        public BulletType()
        {
            this.TextureName = "bullet";
            this.Speed = new Vector2(1000, -100);
            this.SpeedChange = new Vector2(1.00f, 0f);
            this.Mass = 0.0f;
        }
    }
}
