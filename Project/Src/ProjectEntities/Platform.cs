using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.UISystem;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.Utils;




namespace ProjectEntities
{
    public class PlatformType : DynamicType
    {

    }

    [LogicSystemBrowsable(true)]
    [Browsable(true)]
    public class Platform : Dynamic
    {
        PlatformType _type = null;
        public new PlatformType Type { get { return _type; } }


        // max elevation on the platform from the start position
        [FieldSerialize]
        float maxElevation = 1;

        public float MaxElevation
        {
            get { return maxElevation; }
            set { maxElevation = value; }
        }

        // time for reach the top
        [FieldSerialize]
        float velocity = 1f;

        public float Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        float minElevation;

        float direction = 1;
        //public static bool PlatformStart = false;
        [Description("Always Run")]
        [FieldSerialize]
        [LogicSystemBrowsable(true)]
        [Browsable(true)]
        public bool platformStart;

        [LogicSystemBrowsable(true)]
        [Browsable(true)]
        [DefaultValue(false)]
        public bool PlatformStart
        {
            get { return platformStart; }
            set { platformStart = value; }
        }

        [FieldSerialize]
        public bool cyclePlatform;


        [LogicSystemBrowsable(true)]
        [Browsable(true)]
        [DefaultValue(false)]
        public bool CyclePlatform
        {
            get { return cyclePlatform; }
            set { cyclePlatform = value; }
        }




        /* ///////////////////////////////////// works in logic editor for instances & not visible in the properties in Map editor (useful for controlling elevator at runtime)
          public bool test;

          [LogicSystemBrowsable(true)]
          [Browsable(false)]
          [DefaultValue(false)]
          public bool Test
          {
              get { return test; }
              set { test = value; }
          }
         //////////////////////////////////// */

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            minElevation = PhysicsModel.Bodies[0].Position.Z;

            SubscribeToTickEvent();//AddTimer();
        }

        protected override void OnTick()
        {
            base.OnTick();
            if (platformStart == true)
            {
                PhysicsModel.Bodies[0].LinearVelocity = new Vec3(0, 0, (velocity) * direction);//was  new Vec3(0, 0, (1/velocity)* maxElevation * direction)


                if ((PhysicsModel.Bodies[0].Position.Z >= (minElevation + maxElevation) && direction == 1) ||
                    (PhysicsModel.Bodies[0].Position.Z <= minElevation && direction == -1))
                {
                    if (cyclePlatform == false) platformStart = false;
                    PhysicsModel.Bodies[0].LinearVelocity = Vec3.Zero;
                    direction = -direction;
                }
            }
        }
    }
}
