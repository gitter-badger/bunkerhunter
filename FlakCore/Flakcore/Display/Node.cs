using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Flakcore.Utils;
using Flakcore.Physics;

namespace Flakcore.Display
{
    public class Node : ICloneable
    {
        private static float DrawDepth = 0;

        public List<Node> Children { get; protected set; }

        public Node Parent;

        public Vector2 Position = Vector2.Zero;
        public Vector2 PreviousPosition { get; protected set; }
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 PreviousVelocity { get; protected set; }
        public Vector2 Acceleration = Vector2.Zero;
        public float Alpha = 1;
        public float Depth = 0;

        public float Mass = 0;

        public int Width;
        public int Height;

        public Vector2 Origin = Vector2.Zero;
        public Vector2 Scale = Vector2.One;

        public float Rotation = 0;
        public float RotationVelocity = 0;

        public Vector2 ScrollFactor = Vector2.One;

        public Sides Touching;
        public Sides WasTouching;

        public Vector2 MaxVelocity = Vector2.Zero;
        public float Elasticity = 0f;
        public bool Visable = true;
        public bool Immovable = false;
        public bool Collidable = true;
        public bool UpdateChildren = true;
        public Sides CollidableSides;
        public bool Active { get; protected set; }

        private List<string> CollisionGroup;
        private Matrix LocalTransform;

        private List<Activity> Activities;

        private static Quaternion RotationQuaternoin;
        private static Vector3 position3, scale3;

        public Node()
        {
            Children = new List<Node>(1000);
            CollisionGroup = new List<string>(10);
            this.Active = true;
            this.Collidable = false;
            this.Touching = new Sides();
            this.WasTouching = new Sides();
            this.CollidableSides = new Sides();
            this.CollidableSides.SetAllTrue();
            this.Activities = new List<Activity>();
            
            if(Node.RotationQuaternoin == null)
                Node.RotationQuaternoin = new Quaternion();
        }

        public static float GetDrawDepth(float depth)
        {
            Node.DrawDepth += 0.00001f;
            return depth + Node.DrawDepth;
        }

        public void AddChild(Node child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public void RemoveChild(Node child)
        {
            if (!Children.Remove(child))
                throw new Exception("Tried to remove child but gave an error");
        }

        public void RemoveFromParent()
        {
            if (this.Parent != null)
                this.Parent.RemoveChild(this);
        }

        public void AddActivity(Activity activity, bool startImmediately)
        {
            this.Activities.Add(activity);

            if (startImmediately)
                activity.Start();
        }

        public void RemoveActivity(Activity activity)
        {
            this.Activities.Remove(activity);

            activity = null;
        }

        public virtual void Update(GameTime gameTime)
        {
            GameManager.UpdateCalls++;

            if (!this.Active)
                return;

            if (this.UpdateChildren)
            {
                int childrenCount = this.Children.Count;
                for (int i = 0; i < childrenCount; i++)
                {
                    this.Children[i].Update(gameTime);
                    this.Children[i].PreCollisionUpdate(gameTime);
                }
            }

            for (int i = 0; i < this.Activities.Count; i++)
                this.Activities[i].Update(gameTime);
            

            this.Velocity.Y += this.Mass * GameManager.Gravity;

            WasTouching = Touching;
            Touching = new Sides();
        }


        public virtual void PreCollisionUpdate(GameTime gameTime)
        {
        }

        public virtual void PostUpdate(GameTime gameTime)
        {
            if (!this.Active)
                return;

            for (int i = 0; i < this.Children.Count; i++)
                this.Children[i].PostUpdate(gameTime);
                
            if (!Immovable)
            {
                this.PreviousPosition = this.Position;
                this.PreviousVelocity = this.Velocity;

                this.Rotation += RotationVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                this.Position += this.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ParentNode parentNode = new ParentNode();
            parentNode.Position = this.Position;
            parentNode.Alpha = this.Alpha;

            this.Draw(spriteBatch, parentNode);
        }

        public virtual void Draw(SpriteBatch spriteBatch, ParentNode parentNode)
        {
            if (!Visable || !Active)
                return;

            parentNode.Position += this.Position;
            parentNode.Alpha = Math.Min(this.Alpha, parentNode.Alpha);

            int childrenCount = this.Children.Count;
            for (int i = 0; i < childrenCount; i++)
                this.Children[i].Draw(spriteBatch, parentNode);
        }

        public void RemoveAllChildren()
        {
            Children = null;
            Children = new List<Node>();
        }

        public virtual BoundingRectangle GetBoundingBox()
        {
            return new BoundingRectangle(this.WorldPosition.X, this.WorldPosition.Y, Width, Height);
        }

        public virtual Rectangle GetBoundingBox(Vector2 position)
        {
            return new Rectangle((int)position.X, (int)position.Y, Width, Height);
        }

        public virtual void Deactivate()
        {
            this.Active = false;
            this.Visable = false;

            if (this is IPoolable)
            {
                IPoolable node = this as IPoolable;
                if(node.ReportDeadToPool != null)
                    node.ReportDeadToPool(node.PoolIndex);
            }
        }

        public virtual void Activate()
        {
            this.Active = true;
            this.Visable = true;
        }

        public Matrix GetLocalTransform()
        {
            this.LocalTransform = Matrix.CreateTranslation(-Origin.X, -Origin.Y, 0f) *
                //Matrix.CreateScale(Scale.X, Scale.Y, 1f) *
                //Matrix.CreateRotationZ(Rotation) *   
                Matrix.CreateTranslation(Position.X, Position.Y, 0f);

            return this.LocalTransform;
        }

        public static void decomposeMatrix(ref Matrix matrix, out Vector2 position, out Vector2 scale)
        {
            matrix.Decompose(out scale3, out Node.RotationQuaternoin, out position3);
            position.X = position3.X;
            position.Y = position3.Y;
            scale.X = scale3.X;
            scale.Y = scale3.Y;
        }

        public virtual List<Node> GetAllChildren(List<Node> nodes)
        {
            nodes.Add(this);

            if (Children.Count == 0)
                return nodes;
            else
            {
                foreach (Node child in Children)
                {
                    child.GetAllChildren(nodes);
                }

                return nodes;
            }
        }

        public virtual List<Node> GetAllCollidableChildren(List<Node> nodes)
        {
            nodes.Add(this);

            if (Children.Count == 0)
                return nodes;
            else
            {
                foreach (Node child in Children)
                {
                    if(child.Collidable && child.Active)
                        child.GetAllCollidableChildren(nodes);
                }

                return nodes;
            }
        }

        public void RoundPosition()
        {
            this.Position.X = (float)Math.Round(this.Position.X);
            this.Position.Y = (float)Math.Round(this.Position.Y);
        }

        public void RoundVelocity()
        {
            this.Velocity.X = (float)Math.Round(this.Velocity.X);
            this.Velocity.Y = (float)Math.Round(this.Velocity.Y);
        }

        public void AddCollisionGroup(string groupName)
        {
            this.CollisionGroup.Add(groupName);
        }

        public void RemoveCollisionGroup(string groupName)
        {
            this.CollisionGroup.Remove(groupName);
        }

        public bool IsMemberOfCollisionGroup(string groupName)
        {
            return this.CollisionGroup.Contains(groupName);
        }

        public bool HasCollisionGroups()
        {
            return this.CollisionGroup.Count > 0;
        }

        public Vector2 ScreenPosition
        {
            get
            {
                return GameManager.currentDrawCamera.TransformPosition(this.WorldPosition);
            }
        }

        public Vector2 WorldPosition
        {
            get
            {
                if (Parent == null)
                    return this.Position;
                else
                    return Parent.WorldPosition + Position;
            }
        }


        public float GetParentDepth()
        {
            if (this.Parent != null)
                return this.Parent.GetParentDepth() + this.Depth;
            else
                return this.Depth;
        }

        public object Clone()
        {
            Node clone = new Node();
            clone.Position = new Vector2(Position.X, Position.Y);
            clone.Velocity = new Vector2(Velocity.X, Velocity.Y);
            clone.Width = Width;
            clone.Height = Height;
            clone.Parent = Parent;

            return clone;
        }

        public void Clone(Node node)
        {
            node.Position = this.Position;
            node.Velocity = this.Velocity;
            node.Width = this.Width;
            node.Height = this.Height;
            node.Parent = this.Parent;
        }

        internal static void ResetDrawDepth()
        {
            Node.DrawDepth = 0;
        }
    }

    public struct ParentNode
    {
        public Vector2 Position;
        public float Alpha;
    }
}
