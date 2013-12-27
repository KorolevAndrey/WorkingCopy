// Copyright (C) 2006-2007 NeoAxis Group
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.Utils;
//using GameCommon;
using ProjectEntities;

namespace GameEntities
{    /// <summary>
    /// Defines the <see cref="SpawnPoint"/> entity type.
    /// </summary>
    public class SelectiveSpawnPointType : MapObjectType
    {
    }

    public class SelectiveSpawnPoint : MapObject
    {
        [FieldSerialize]
        AIType aiType;
        [FieldSerialize]
        FactionType faction;
        [FieldSerialize]
        UnitType spawnedUnit;
        [FieldSerialize]
        float spawnTime;
        [FieldSerialize]
        float spawnRadius;
        [FieldSerialize]
        int popNumber;
        [FieldSerialize]
        float closeToEvent;
        //counter for remaining time
        float spawnCounter;
        //the amount of entities left to spawn
        int popAmount;

        SelectiveSpawnPointType _type = null; public new SelectiveSpawnPointType Type { get { return _type; } }
        [Description("Initial Faction or null for neutral")]
        public FactionType Faction
        {
            get { return faction; }
            set { faction = value; }
        }
        [Description("Time in seconds between spawns")]
        [DefaultValue(20.0f)]
        public float SpawnTime
        {
            get { return spawnTime; }
            set { spawnTime = value; }
        }
        [Description("Spawn radius that has to be free of other units")]
        [DefaultValue(10.0f)]
        public float SpawnRadius
        {
            get { return spawnRadius; }
            set { spawnRadius = value; }
        }
        [Description("UnitType that will be spawned")]

        public UnitType SpawnedUnit
        {
            get { return spawnedUnit; }

            set { spawnedUnit = value; }
        }
        [Description("Default AI for this unit or none for empty unit")]
        public AIType AIType
        {
            get { return aiType; }
            set { aiType = value; }
        }
        [Description("amount of entities that will be created")]
        public int PopNumber
        {
            get { return popNumber; }
            set { popNumber = value; }

        }
        [Description("when player enters this radius the entities will spawn")]
        [DefaultValueAttribute(10.0f)]
        public float CloseToEvent
        {
            get { return closeToEvent; }
            set { closeToEvent = value; }
        }
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            spawnCounter = 0.0f;
            //AddTimer();
            SubscribeToTickEvent();
        }
        protected override void OnTick()
        {
            base.OnTick();
            spawnCounter += TickDelta;
            if (spawnCounter >= SpawnTime) //time to do it
            {
                spawnCounter = 0.0f;
                if (!isSpawnPositionFree()) return;
                if (!isCloseToPoint()) return;
                popAmount++;
                if (popAmount <= PopNumber)
                {
                    Unit i = (Unit)Entities.Instance.Create(SpawnedUnit, Parent);
                    if (AIType != null)
                    {
                        i.InitialAI = AIType;
                    }
                    if (i == null) return;
                    i.Position = FindFreePositionForUnit(i, Position);
                    i.Rotation = Rotation;
                    if (Faction != null)
                    {
                        i.InitialFaction = Faction;
                    }
                    i.PostCreate();
                }
            }
        }
        Vec3 FindFreePositionForUnit(Unit unit, Vec3 center)
        {
            Vec3 volumeSize = unit.MapBounds.GetSize() + new Vec3(2, 2, 0);
            for (float zOffset = 0; ; zOffset += .3f)
            {
                for (float radius = 3; radius < 8; radius += .6f)
                {
                    for (float angle = 0; angle < MathFunctions.PI * 2; angle += MathFunctions.PI / 32)
                    {
                        Vec3 pos = center + new Vec3(MathFunctions.Cos(angle),
                             MathFunctions.Sin(angle), 0) * radius + new Vec3(0, 0, zOffset);
                        Bounds volume = new Bounds(pos);
                        volume.Expand(volumeSize * .5f);
                        Body[] bodies = PhysicsWorld.Instance.VolumeCast(
                            volume, (int)ContactGroup.CastOnlyContact);
                        if (bodies.Length == 0)
                            return pos;
                    }
                }
            }
        }
        bool isCloseToPoint()
        {
            bool returnValue = false;
            if (CloseToEvent <= 0)
            {
                returnValue = true;
            }
            else
            {
                Map.Instance.GetObjects(new Sphere(Position, CloseToEvent),
                  MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)  //GameFilterGroups.UnitFilterGroup, delegate(MapObject mapObject)
                  {
                      PlayerCharacter pchar = mapObject as PlayerCharacter;
                      if (pchar != null)
                      {
                          returnValue = true;
                      }
                  });

            }
            return returnValue;
        }
        bool isSpawnPositionFree()
        {
            bool returnValue = true;
            Map.Instance.GetObjects(new Sphere(Position, SpawnRadius),
           MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject) //GameFilterGroups.UnitFilterGroup, delegate(MapObject mapObject)
           {
               Unit unit = mapObject as Unit;
               //if there is atleast one then we won't spawn
               if (unit != null)
               {
                   returnValue = false;
               }
           });

            /*
            foreach (Entity obj in Entities.Instance.EntitiesCollection)
            {
                Unit un = obj as Unit;
                if (un != null)
                {
                    if ((un.Position - Position).LengthFast() < SpawnRadius)
                    {
                        returnValue =  false;
                    }
                                    }
            }
            */
            return returnValue;
        }
    }
}
