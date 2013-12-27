// Copyright (C) 2006-2007 NeoAxis Group
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.Renderer;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;

namespace GameEntities
{

    /// <summary>
    /// Defines the <see cref="Beam"/> entity type.
    /// </summary>
    public class BeamType : MapObjectType
    {
        public const float torad = (1.0f / 360.0f) * 2.0f * (float)Math.PI;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class AnimatedFloat
        {
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public abstract class BaseFunction
            {

                [FieldSerialize]
                bool islooped;
                public bool IsLooped
                {
                    get { return islooped; }
                    set { islooped = value; }
                }

                protected float localval;
                public float Val
                {
                    get { return localval; }
                }

                public abstract void Tick(float deltatime);

                static protected void CheckLoop(ref float valtockeck, ref float speed, float min, float max)
                {
                    if (valtockeck > max)
                    {
                        speed *= (-1);
                        valtockeck = max;
                    }
                    else
                        if (valtockeck < min)
                        {
                            speed *= (-1);
                            valtockeck = min;
                        }
                }

                static protected void CheckClamp(ref  float valtockeck, float min, float max)
                {
                    if (valtockeck > max)
                    {
                        valtockeck = max;
                    }
                    else
                        if (valtockeck < min)
                        {
                            valtockeck = min;
                        }
                }

                static protected void CheckReset(ref  float valtockeck, float min, float max)
                {
                    if (valtockeck > max)
                    {
                        valtockeck = min;
                    }
                    else
                        if (valtockeck < min)
                        {
                            valtockeck = max;
                        }
                }

                public override string ToString()
                {
                    return String.Format("Evolution Fonction");
                }
            }

            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class CirularFunction : BaseFunction
            {
                [FieldSerialize]
                float anglemin = 0;
                public float AngleMin
                {
                    get { return anglemin; }
                    set
                    {
                        if (value <= anglemax)
                            anglemin = value;

                    }
                }

                [FieldSerialize]
                float anglemax = 0;
                public float AngleMax
                {
                    get { return anglemax; }
                    set
                    {
                        if (value >= AngleMin)
                            anglemax = value;
                    }
                }

                [FieldSerialize]
                float originalanglespeed = 0;
                [Description("Speed of Variation of the angle, in degree per second")]
                public float AngleSpeedVariation
                {
                    get { return originalanglespeed; }
                    set
                    {
                        originalanglespeed = value;
                        localanglespeed = value;
                    }
                }

                // phasis is used for cosine et sinus variation
                [FieldSerialize]
                float phasis = 0;
                public float Phasis
                {
                    get { return phasis; }
                    set { phasis = value; }
                }

                [FieldSerialize]
                float radius = 0.0f;
                public float Radius
                {
                    get { return radius; }
                    set { radius = value; }
                }

                [FieldSerialize]
                float center = 0.0f;
                public float Center
                {
                    get { return center; }
                    set { center = value; }
                }

                [FieldSerialize]
                float anglestartvalue = 0;
                public float AngleStartValue
                {
                    get { return anglestartvalue; }
                    set { anglestartvalue = value; }
                }

                float localangle;
                float localanglespeed;

                public CirularFunction(float argstartangle, float argphasis, float argradius, float argcenter,
                                       float arganglemin, float arganglemax, float arganglespeed, bool looped)
                {
                    phasis = argphasis; radius = argradius; center = argcenter;
                    anglemin = Math.Min(arganglemin, arganglemax);
                    anglemax = Math.Max(anglemin, arganglemax);
                    anglestartvalue = Math.Max(argstartangle, anglemin);
                    anglestartvalue = Math.Min(anglestartvalue, anglemax);
                    originalanglespeed = arganglespeed;
                    localanglespeed = arganglespeed;
                    localangle = anglestartvalue;
                    this.IsLooped = looped;

                }

                public override void Tick(float deltatime)
                {
                    if (IsLooped)
                    {
                        localangle += localanglespeed * deltatime;
                        CheckLoop(ref  localangle, ref localanglespeed, anglemin, anglemax);
                        localval = center + radius * (float)Math.Cos((localangle + phasis) * torad);
                    }
                    else
                    {
                        localangle += localanglespeed * deltatime;
                        CheckReset(ref  localangle, anglemin, anglemax);
                        localval = center + radius * (float)Math.Cos((localangle + phasis) * torad);
                    }
                }
            }

            [TypeConverter(typeof(ExpandableObjectConverter))]
            public class LinearFunction : BaseFunction
            {
                [FieldSerialize]
                float min;
                public float Min
                {
                    get { return min; }
                    set
                    {
                        if (value <= max)
                            min = value;

                    }
                }

                [FieldSerialize]
                float max;
                public float Max
                {
                    get { return max; }
                    set
                    {
                        if (value >= min)
                            max = value;
                    }
                }

                [FieldSerialize]
                float originalspeed;
                public float Speed
                {
                    get { return originalspeed; }
                    set
                    {
                        originalspeed = value;
                        localspeed = value;
                    }

                }

                [FieldSerialize]
                float startval = 0.0f;
                public float StartValue
                {
                    get { return startval; }
                    set
                    {
                        startval = value;
                        startval = Math.Max(startval, min);
                        startval = Math.Min(startval, max);
                        localval = startval;
                    }
                }

                float localspeed;

                /*
                public LinearFunction(float argmin, float argmax, float argstartval,float speed, bool looped)
                {
                    min = Math.Min(argmin, argmax);
                    max = Math.Max(argmin, argmax);
                    startval = Math.Max(argstartval, min);
                    startval = Math.Min(startval, max);
                    originalspeed = speed;
                    localspeed = originalspeed;
                    localval = startval;
                    IsLooped = looped;
                }
                */
                public override void Tick(float deltatime)
                {
                    if (IsLooped)
                    {
                        localval += localspeed * deltatime;
                        CheckLoop(ref  localval, ref localspeed, min, max);
                    }
                    else
                    {
                        localval += localspeed * deltatime;
                        CheckReset(ref  localval, min, max);
                    }
                }

                public void Init()
                {
                    localval = StartValue;
                    localspeed = originalspeed;
                }
            }

            [Browsable(false)]
            float totalval;
            public float Val
            {
                get
                {
                    totalval = /*circularcomponent.Val +*/linearcomponent.Val;
                    return totalval;
                }
            }

            /*
            [FieldSerialize]
            CirularFunction circularcomponent = new CirularFunction(0,0,0,0,0,0,0,true);
            public CirularFunction CircularComponent
            {
                get { return circularcomponent; }
                set { circularcomponent = value; }
            }
             */

            [FieldSerialize]
            LinearFunction linearcomponent = new LinearFunction(); //0,0,0,0,true);
            public LinearFunction LinearComponent
            {
                get { return linearcomponent; }
                set { linearcomponent = value; }
            }

            public void Tick(float deltatime)
            {
                //circularcomponent.Tick(deltatime);
                linearcomponent.Tick(deltatime);
            }

            public override string ToString()
            {
                return string.Format("Variable Parameters");
            }

            public void Init()
            {
                linearcomponent.Init();
                totalval = linearcomponent.Val;
            }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CylindricCoord
        {
            [FieldSerialize]
            AnimatedFloat r;
            [Description("Radius, distance from the axis")]
            public AnimatedFloat R
            {
                get { return r; }
                set { r = value; }
            }

            [FieldSerialize]
            AnimatedFloat teta;
            [Description("Angular position, around X axis")]
            public AnimatedFloat Teta
            {
                get { return teta; }
                set { teta = value; }
            }

            [FieldSerialize]
            AnimatedFloat z;
            [Description("Position along the axis of the cylindric space, ie the X axis in carthesian position. 0 is the position of the object, 1 is the extremity (length)")]
            public AnimatedFloat Z
            {
                get { return z; }
                set { z = value; }
            }

            public Vec3 ToXYZ()
            {
                return new Vec3(z.Val,
                                r.Val * (float)Math.Cos(teta.Val * torad),
                                r.Val * (float)Math.Sin(teta.Val * torad));
            }

            public CylindricCoord()
            {
                r = new AnimatedFloat();
                Teta = new AnimatedFloat();
                z = new AnimatedFloat();
            }
            public void Tick(float deltatime)
            {
                r.Tick(deltatime);
                z.Tick(deltatime);
                teta.Tick(deltatime);
            }

            public override string ToString()
            {
                return string.Format("Cylindric Coordinate");
            }

            public void Init()
            {
                r.Init();
                teta.Init();
                z.Init();
            }
        }

        [FieldSerialize]
        bool useatachedcontrolpoint = false;
        [DefaultValue(false)]
        public bool UseAttachedObjectAsControlPoint
        {
            get { return useatachedcontrolpoint; }
            set { useatachedcontrolpoint = value; }
        }

        [FieldSerialize]
        float duration = 0;
        [Category("Settings : Time and Spawning")]
        [DefaultValue(0)]
        public float Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        [FieldSerialize]
        int numrepeat = 0;
        [Category("Settings : Time and Spawning")]
        [DefaultValue(0)]
        public int CycleRepetitions
        {
            get { return numrepeat; }
            set { numrepeat = value; }
        }


        [FieldSerialize]
        float updatetime = 0.1f;
        [Category("Settings : Time and Spawning")]
        [DefaultValue(0.1f)]
        public float UpDateTime
        {
            get { return updatetime; }
            set { updatetime = value; }
        }

        [FieldSerialize]
        MapObjectType mainpointobjtype;
        [Category("Settings : Time and Spawning")]
        public MapObjectType MainPointObjectType
        {
            get { return mainpointobjtype; }
            set { mainpointobjtype = value; }
        }

        [FieldSerialize]
        float jointobjectprobability;
        [Category("Settings : Time and Spawning")]
        [DefaultValue(0.1f)]
        public float JointObjectSpawnProbability
        {
            get { return jointobjectprobability; }
            set { jointobjectprobability = value; }
        }

        [FieldSerialize]
        float pitchspawningdeviation;
        [Category("Settings : Time and Spawning")]
        public float PitchSpawningDeviation
        {
            get { return pitchspawningdeviation; }
            set { pitchspawningdeviation = value; }
        }

        [FieldSerialize]
        float yawspawningdeviation;
        [Category("Settings : Time and Spawning")]
        public float YawSpawningDeviation
        {
            get { return yawspawningdeviation; }
            set { yawspawningdeviation = value; }
        }

        [FieldSerialize]
        float rollspawningdeviation;
        [Category("Settings : Time and Spawning")]
        public float RollSpawningDeviation
        {
            get { return rollspawningdeviation; }
            set { rollspawningdeviation = value; }
        }

        # region Settings : Shape
        [FieldSerialize]
        int numberofpoints = 10;
        [Category("Settings : Shape")]
        [DefaultValue(10)]
        public int NumberOfPoints
        {
            get { return numberofpoints; }
            set { numberofpoints = value; }
        }

        [FieldSerialize]
        float length = 1.0f;
        [Category("Settings : Shape")]
        [DefaultValue(1.0f)]
        public float Length
        {
            get { return length; }
            set { length = value; }
        }

        [FieldSerialize]
        bool raycast;
        [Category("Settings : Shape")]
        public bool UseRayCast
        {
            get { return raycast; }
            set { raycast = value; }
        }

        [FieldSerialize]
        AnimatedFloat startwidth = new AnimatedFloat();
        [Category("Settings : Shape")]
        public AnimatedFloat WidthStart
        {
            get { return startwidth; }
            set { startwidth = value; }
        }

        [FieldSerialize]
        AnimatedFloat endwidth = new AnimatedFloat();
        [Category("Settings : Shape")]
        public AnimatedFloat WidthEnd
        {
            get { return endwidth; }
            set { endwidth = value; }
        }

        [FieldSerialize]
        Vec3 noisealongcurve = Vec3.Zero;
        [Category("Settings : Shape")]
        public Vec3 NoiseAlongCurve
        {
            get { return noisealongcurve; }
            set { noisealongcurve = value; }
        }

        [FieldSerialize]
        bool usefastbeam = false;
        [Category("Settings : Shape")]
        public bool UseFastBeam
        {
            get { return usefastbeam; }
            set { usefastbeam = value; }
        }

        [FieldSerialize]
        Vec3 controlpointnoisefactor = Vec3.Zero;
        [Category("Settings : Shape")]
        public Vec3 ControlPointNoiseFactor
        {
            get { return controlpointnoisefactor; }
            set { controlpointnoisefactor = value; }
        }

        [FieldSerialize]
        CylindricCoord Bcontrolpoint = new CylindricCoord();
        [Category("Settings : Shape")]
        public CylindricCoord BControlPoint
        {
            get { return Bcontrolpoint; }
            set { Bcontrolpoint = value; }
        }

        [FieldSerialize]
        CylindricCoord Ccontrolpoint = new CylindricCoord();
        [Category("Settings : Shape")]
        public CylindricCoord CControlPoint
        {
            get { return Ccontrolpoint; }
            set { Ccontrolpoint = value; }
        }
        #endregion

        [FieldSerialize]
        bool nomeshmode = false;
        [Description("Use this option to disable mesh rendering. Useful for the spawning of object along a curve.")]
        [Category("Settings : Rendering")]
        [DefaultValue(false)]
        public bool NoMeshMode
        {
            get { return nomeshmode; }
            set { nomeshmode = value; }
        }

        [FieldSerialize]
        bool permainpointcamface = false;
        [Description("Set to True to enable a per main point segment facing. CPU consuming.")]
        [Category("Settings : Rendering")]
        [DefaultValue(false)]
        public bool PerMainPointCameraFace
        {
            get { return permainpointcamface; }
            set { permainpointcamface = value; }
        }

        [FieldSerialize]
        bool doubleshape = false;
        [Description("Set to True to enable double shape : 2 orthogonal ribbons instead of one.")]
        [Category("Settings : Rendering")]
        [DefaultValue(false)]
        public bool DoubleShape
        {
            get { return doubleshape; }
            set { doubleshape = value; }
        }

        [FieldSerialize]
        bool disablemipmapfiltering = false;
        [Description("This Option disable all filtering parameters for the material, to avoid visual artifact due to mipmaping for example.")]
        [Category("Settings : Rendering")]
        public bool DisableMipMapFiltering
        {
            get { return disablemipmapfiltering; }
            set { disablemipmapfiltering = value; }
        }

        [FieldSerialize]
        float textureperunit = 1.0f;
        [Category("Settings : Rendering")]
        public float TexturePerWorldUnit
        {
            get { return textureperunit; }
            set { textureperunit = value; }
        }

        [FieldSerialize]
        string materialName = "";
        [Category("Settings : Rendering")]
        [Editor(typeof(EditorMaterialUITypeEditor), typeof(UITypeEditor))]
        public string MaterialName
        {
            get { return materialName; }
            set { materialName = value; }
        }


        #region Settings : color
        [FieldSerialize]
        ColorValue startcolorvalue = new ColorValue();
        [Category("Settings : Color")]
        public ColorValue StartColor
        {
            get { return startcolorvalue; }
            set { startcolorvalue = value; }
        }

        [FieldSerialize]
        ColorValue middlecolorvalue = new ColorValue();
        [Category("Settings : Color")]
        public ColorValue MiddleColor
        {
            get { return middlecolorvalue; }
            set { middlecolorvalue = value; }
        }

        [FieldSerialize]
        float middlecolorpos = 0.5f;
        [Category("Settings : Color")]
        public float MiddleColorPosition
        {
            get { return middlecolorpos; }
            set { middlecolorpos = value; }
        }

        [FieldSerialize]
        ColorValue endcolorvalue = new ColorValue();
        [Category("Settings : Color")]
        public ColorValue EndColor
        {
            get { return endcolorvalue; }
            set { endcolorvalue = value; }
        }

        [FieldSerialize]
        float fadeinend = 0.0f;
        [Category("Settings : Color")]
        [EditorLimitsRange(0.0f, 1.0f)]
        public float FadeInEnd
        {
            get { return fadeinend; }
            set
            {
                fadeinend = value;
                if (fadeinend > fadeoutstart)
                    fadeinend = fadeoutstart;
            }
        }

        [FieldSerialize]
        float fadeoutstart = 1.0f;
        [Category("Settings : Color")]
        [EditorLimitsRange(0.0f, 1.0f)]
        public float FadeOutStart
        {
            get { return fadeoutstart; }
            set
            {
                fadeoutstart = value;
                if (fadeoutstart < fadeinend)
                    fadeoutstart = fadeinend;
            }
        }
        #endregion
    }

    /// <summary>
    /// Example of dynamic geometry.
    /// </summary>
    /// 
    [LogicSystemBrowsable(true)]
    [Browsable(true)]
    public class Beam : MapObject
    {
        // private local variables...
        Mesh mesh;
        MapObjectAttachedMesh attachedMesh;
        float raycastdistance;
        float currentduration = 1.0f;
        int numofcycle;
        float updateTimeRemaining;
        bool editormode = false;
        bool needUpdateVertices;
        bool needUpdateIndices;
        BeamType.AnimatedFloat endwidth = new BeamType.AnimatedFloat();
        BeamType.AnimatedFloat startwidth = new BeamType.AnimatedFloat();
        BeamType.CylindricCoord bcontrolpoint = new BeamType.CylindricCoord();
        BeamType.CylindricCoord ccontrolpoint = new BeamType.CylindricCoord();

        // properties
        [FieldSerialize]
        bool doraycast = false;
        public bool UseRayCast
        {
            get { return doraycast; }
            set { doraycast = value; }
        }


        [FieldSerialize]
        bool useoverridenparam = false;
        public bool UseOverridenParameters
        {
            get { return useoverridenparam; }
            set { useoverridenparam = value; }
        }

        [FieldSerialize]
        float length = 1.0f;
        [DefaultValue(1.0f)]
        public float Length
        {
            get
            {
                if (Type.UseRayCast)
                    return raycastdistance;
                if (useoverridenparam)
                    return length;
                else
                    return this.Type.Length;
            }
            set
            {
                if (useoverridenparam)
                {
                    length = value;
                    raycastdistance = length;
                }
                else return;
            }
        }

        RayCastResult lastraycastresult;
        [Browsable(false)]
        public RayCastResult LastRayCastResult
        {
            get { return lastraycastresult; }
        }

        MapObject lastobjecthit = null;
        [Browsable(false)]
        public MapObject LastObjectHit
        {
            get { return lastobjecthit; }
        }

        [Browsable(false)]
        public int NumberOfPoints { get { return this.Type.NumberOfPoints; } }

        [Browsable(false)]
        public bool DoubleShape
        {
            get
            {
                if (editormode)
                    return false;
                else
                    return this.Type.DoubleShape;
            }

        }

        // list of mainpoint, ie points along the curves
        public class MainPoint
        {
            Vec3 _pos;
            public Vec3 Pos
            {
                get { return _pos; }
            }

            Vec3 _dir;
            public Vec3 Dir
            {
                get { return _dir; }
            }

            float _width;
            public float Width
            {
                get { return _width; }
            }

            uint _pcolor;
            public uint PColor
            {
                get { return _pcolor; }
            }

            public MainPoint(Vec3 pos, Vec3 dir, float width, uint pcolor)
            {
                _pos = pos;
                _dir = dir;
                _width = width;
                _pcolor = pcolor;
            }
        }
        List<MainPoint> mainpoints = new List<MainPoint>();

        Bounds bounds = new Bounds(new Vec3(-10.0f, -5.0f, -5.0f), new Vec3(10.0f, 5.0f, 5.0f));

        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vec3 position;
            public Vec3 normal;
            public Vec2 texCoord;
            public uint color;
        }

        BeamType _type = null; public new BeamType Type { get { return _type; } }

        ///////////////////////////////////////////////////////////////////////////////////////bernie

        public bool test;

        [LogicSystemBrowsable(true)]
        [Browsable(false)]
        [DefaultValue(false)]
        public bool Test
        {
            get { return test; }
            set { test = value; }
        }
        ///////////////////////////////////////////////////////////////////////////////////////bernie end


        #region Overiden section
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            if (!Type.NoMeshMode)
            {
                CreateMesh();
                AttachMesh();
                RenderSystem.Instance.RenderSystemEvent += RenderSystem_RenderSystemEvent;
            }

            SubscribeToTickEvent(); //AddTimer();



        }

        protected override void OnPreCreate()  //(bool loaded)
        {
            base.OnPreCreate();//(loaded);

            // initializing variables
            raycastdistance = this.Type.Length;
            ControlPointInit();
            endwidth = this.Type.WidthEnd; endwidth.Init();
            startwidth = this.Type.WidthStart; startwidth.Init();
            length = this.Type.Length;
            currentduration = Type.Duration;
            numofcycle = Type.CycleRepetitions;
            doraycast = Type.UseRayCast;

            if (EntitySystemWorld.Instance.WorldSimulationType == WorldSimulationTypes.Editor)
            { editormode = true; }
        }

        protected override void OnTick()
        {
            base.OnTick();
            ControlPointTick(TickDelta); // ticking control points
            endwidth.Tick(TickDelta);  // tick width parametes...
            startwidth.Tick(TickDelta); // tick width parametes...
            TickDuration(TickDelta); // ticking update time and life related elements.
        }

        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);
            if (attachedMesh != null)
            {
                // emulating ticking in editor mode, to be able to customize more easily the object.
                if (editormode)
                {
                    float editorticktime = 0.01f;
                    ControlPointTick(editorticktime);
                    endwidth.Tick(editorticktime);
                    startwidth.Tick(editorticktime);
                    TickDuration(editorticktime);

                    updateTimeRemaining -= editorticktime;
                    if (updateTimeRemaining < 0)
                    {
                        updateTimeRemaining += Type.UpDateTime;
                        needUpdateVertices = true; // Indices = true;
                    }
                }

                if (attachedMesh.Visible && !Type.NoMeshMode)
                {
                    //update mesh if needed
                    if (needUpdateVertices)
                    {
                        ComputeMainpoints(Type.UseFastBeam);
                        UpdateMeshVertices(camera, Type.UseFastBeam);
                        needUpdateVertices = false;
                    }

                    if (needUpdateIndices)
                    {
                        UpdateMeshIndices();
                        needUpdateIndices = false;
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            RenderSystem.Instance.RenderSystemEvent -= RenderSystem_RenderSystemEvent;
            DetachMesh();
            DestroyMesh();
            base.OnDestroy();
        }
        #endregion

        #region Control Point Handling
        void ControlPointInit()
        {
            ccontrolpoint = this.Type.CControlPoint;
            bcontrolpoint = this.Type.BControlPoint;
            ccontrolpoint.Init();
            bcontrolpoint.Init();
        }
        void ControlPointTick(float delta)
        {
            ccontrolpoint.Tick(delta);
            bcontrolpoint.Tick(delta);
        }
        #endregion

        #region Mesh Handling
        void CreateMesh()
        {
            string meshName = MeshManager.Instance.GetUniqueName("DynamicMesh");
            mesh = MeshManager.Instance.CreateManual(meshName);

            mesh.SetBoundsAndRadius(bounds, bounds.GetRadius(Vec3.Zero));

            SubMesh subMesh = mesh.CreateSubMesh();
            subMesh.UseSharedVertices = false;

            int maxVertices = (NumberOfPoints + 1) * 2;
            int maxIndices = (NumberOfPoints) * 6;// *3;

            if (DoubleShape)
            {
                maxVertices = maxVertices * 2;
                maxIndices = maxIndices * 2;
            }

            //init vertexData
            VertexDeclaration declaration = subMesh.VertexData.VertexDeclaration;
            declaration.AddElement(0, 0, VertexElementType.Float3, VertexElementSemantic.Position);
            declaration.AddElement(0, 12, VertexElementType.Float3, VertexElementSemantic.Normal);
            declaration.AddElement(0, 24, VertexElementType.Float2, VertexElementSemantic.TextureCoordinates);
            declaration.AddElement(0, 32, VertexElementType.Color, VertexElementSemantic.Diffuse, 0);

            VertexBufferBinding bufferBinding = subMesh.VertexData.VertexBufferBinding;
            HardwareVertexBuffer vertexBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                32 + sizeof(uint), maxVertices, HardwareBuffer.Usage.DynamicWriteOnly);
            bufferBinding.SetBinding(0, vertexBuffer, true);

            //init indexData
            HardwareIndexBuffer indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
                HardwareIndexBuffer.IndexType._16Bit, maxIndices, HardwareBuffer.Usage.DynamicWriteOnly);
            subMesh.IndexData.SetIndexBuffer(indexBuffer, true);

            //set material	
            subMesh.MaterialName = Type.MaterialName;
            if (Type.DisableMipMapFiltering)
            {
                // todo : currently, it only supports highlevel material
                HighLevelMaterial material = HighLevelMaterialManager.Instance.GetMaterialByName(Type.MaterialName);
                Material basemat = material.BaseMaterial;
                foreach (Technique tech in basemat.Techniques)
                    foreach (Pass pass in tech.Passes)
                        foreach (TextureUnitState techunitstat in pass.TextureUnitStates)
                        {
                            techunitstat.SetTextureFiltering(FilterOptions.None, FilterOptions.None, FilterOptions.None);
                        }
            }

            needUpdateVertices = true;
            needUpdateIndices = true;
        }
        void DestroyMesh()
        {
            if (!Type.NoMeshMode)
            {
                if (mesh != null)
                {
                    mesh.Dispose();
                    mesh = null;
                }
            }
        }
        void AttachMesh()
        {
            attachedMesh = new MapObjectAttachedMesh();
            //attachedMesh.CastShadows = false;
            attachedMesh.CastDynamicShadows = false;
            //attachedMesh.MeshObject( mesh.Name );
            attachedMesh.MeshName = mesh.Name;
            Attach(attachedMesh);
        }
        void DetachMesh()
        {
            if (attachedMesh != null)
            {
                Detach(attachedMesh);
                attachedMesh = null;
            }
        }
        #endregion

        #region Width related
        float compwidth(float param)
        {
            float localparam = param / Length;
            return (endwidth.Val - startwidth.Val) * localparam + startwidth.Val;
        }

        #endregion

        #region Object Spawning
        void TrySpawnObjects()
        {
            if (!editormode)
            {
                if (mainpoints.Count > 0)
                {
                    if (Type.MainPointObjectType != null)
                    {
                        foreach (MainPoint curvepoint in mainpoints)
                        {
                            if (World.Instance.Random.NextFloat() <= Type.JointObjectSpawnProbability)
                            {
                                MapObject curveobj = (MapObject)Entities.Instance.Create(Type.MainPointObjectType, Map.Instance);
                                // MapObjectAttachedMapObject curveattached = new MapObjectAttachedMapObject();
                                //curveattached = 

                                curveobj.Position = curvepoint.Pos * this.Rotation + Position;
                                curveobj.Rotation = this.Rotation * new Angles(World.Instance.Random.NextFloatCenter() * Type.RollSpawningDeviation,
                                                                World.Instance.Random.NextFloatCenter() * Type.PitchSpawningDeviation,
                                                                World.Instance.Random.NextFloatCenter() * Type.YawSpawningDeviation).ToQuat();
                                curveobj.PostCreate();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Color Handling
        uint ComputeColorValue(float param)
        {
            ColorValue color;
            ColorValue A, B;
            float localparam = param / Length;
            // interpolation
            if (param > Type.MiddleColorPosition)
            {
                B = (Type.MiddleColor * 1.0f - Type.EndColor * Type.MiddleColorPosition) / (1.0f - Type.MiddleColorPosition);
                A = (Type.EndColor - B) / 1.0f;
                color = A * localparam + B;
            }
            else
            {
                B = Type.StartColor;
                A = (Type.MiddleColor - B) / (Type.MiddleColorPosition);
                color = A * localparam + B;
            }

            // time related computation
            float timefact = 1.0f - currentduration / Type.Duration;
            float a = 1.0f;
            // color = new ColorValue(1.0f, 1.0f, 1.0f, 1.0f);
            if (timefact < Type.FadeInEnd)
            {
                a = timefact / Type.FadeInEnd;
            }
            else
                if (timefact > Type.FadeOutStart)
                {
                    a = (1 - (timefact - Type.FadeOutStart) / (1.0f - Type.FadeOutStart));
                }

            color *= a;
            color.Clamp(ColorValue.Zero, new ColorValue(1, 1, 1, 1));
            return RenderSystem.Instance.ConvertColorValue(color);
        }
        #endregion

        #region Life and Updatetime Handling
        void TickDuration(float tickdelta)
        {
            updateTimeRemaining -= TickDelta;
            if (updateTimeRemaining < 0)
            {
                updateTimeRemaining += Type.UpDateTime;
                needUpdateVertices = true; // Indices = true;
                TrySpawnObjects();
            }

            if (Type.Duration > 0)
            {
                currentduration -= tickdelta;
                if (currentduration < 0)
                {
                    if (editormode)
                    {
                        currentduration += Type.Duration;
                    }
                    else
                    {
                        // checking if infini repetition enabled ?
                        if (Type.CycleRepetitions > 0)
                        {
                            numofcycle--;
                        }
                        else
                        {
                            currentduration += Type.Duration;
                            return;
                        }

                        // checking number of cycle remaining
                        if (numofcycle > 0)
                        {
                            currentduration += Type.Duration;
                        }
                        else
                        { this.SetForDeletion(true); } //{ this.SetShouldDelete(); }
                    }
                }
            }
        }
        #endregion

        #region Shape Computation - Vertices and Indices
        /// <summary>
        /// Call this to compute points along the line. It fills MainPoint and MainDirPoint Lists before rendering.
        /// </summary>
        /// <param name="fastbeam"></param>
        void ComputeMainpoints(bool fastbeam)
        {
            mainpoints.Clear();
            // init...
            Vec3 v_A, v_B, v_C, v_D;
            Vec3 v_dir = new Vec3(1, 0, 0); // this.Rotation.ToMat3().Item0; // Type.EndPoint - Type.StartPoint;
            Vec3 v_pos_next = Vec3.Zero;
            Vec3 v_pos_prev = v_pos_next;
            Vec3 localnoise = Vec3.Zero;
            int pointnum = NumberOfPoints;
            float a = 1.0f - 1.0f / pointnum;
            float b = 1.0f - a;

            // performingraycast if needed 
            if (UseRayCast)
                PerformRayCast();

            float f_basedistance = Length;

            // extremity point
            v_A = v_pos_next;
            v_D = v_pos_next + v_dir * f_basedistance;

            // controlpoint
            Vec3 loccpoint = bcontrolpoint.ToXYZ(); loccpoint.X *= Length;
            v_B = v_pos_next + (loccpoint + Type.ControlPointNoiseFactor * World.Instance.Random.NextFloatCenter());
            loccpoint = ccontrolpoint.ToXYZ(); loccpoint.X *= Length;
            v_C = v_pos_next + (loccpoint + Type.ControlPointNoiseFactor * World.Instance.Random.NextFloatCenter());

            // adding first point on the curve
            mainpoints.Add(new MainPoint(v_pos_next, v_dir, compwidth(0), ComputeColorValue(0)));// this value is not used, in fact...
            // go through all points...
            for (int i = 1; i < pointnum + 1; i++)
            {
                if (fastbeam)
                {
                    v_pos_next = v_pos_prev + v_dir * f_basedistance / pointnum;
                    if (i < pointnum)
                    {
                        localnoise = new Vec3(Type.NoiseAlongCurve.X * World.Instance.Random.NextFloatCenter(),
                                Type.NoiseAlongCurve.Y * World.Instance.Random.NextFloatCenter(),
                                Type.NoiseAlongCurve.Z * World.Instance.Random.NextFloatCenter());
                        v_pos_next += localnoise;
                    }
                    v_dir = v_D - v_pos_next;
                    v_dir.Normalize();
                }
                else
                {
                    // Get a point on the curve
                    v_pos_next = v_A * (float)Math.Pow(a, 3.0f) + v_B * 3.0f * (float)Math.Pow(a, 2.0f) * b + v_C * 3.0f * a * (float)Math.Pow(b, 2.0f) + v_D * (float)Math.Pow(b, 3.0f);
                    if (i < pointnum)
                    {
                        localnoise = new Vec3(Type.NoiseAlongCurve.X * World.Instance.Random.NextFloatCenter(),
                                Type.NoiseAlongCurve.Y * World.Instance.Random.NextFloatCenter(),
                                Type.NoiseAlongCurve.Z * World.Instance.Random.NextFloatCenter());
                        v_pos_next += localnoise;
                    }
                    v_dir = v_pos_next - v_pos_prev;
                    v_dir.Normalize();
                }

                mainpoints.Add(new MainPoint(v_pos_next, v_dir, compwidth(v_pos_next.X), ComputeColorValue(v_pos_next.X)));

                // Change the variable
                a -= 1.0f / pointnum;
                b = 1.0f - a;
                v_pos_prev = v_pos_next;
            }

            // updating bounds. a margin of 1.5 time the mainpoint coord are taken to ensure visibility
            bounds = Bounds.Zero;
            bounds.Add(Vec3.Zero);
            foreach (MainPoint mainpoint in mainpoints)
                bounds.Add(mainpoint.Pos * 1.5f);

            mesh.SetBoundsAndRadius(bounds, bounds.GetRadius(Vec3.Zero));

            foreach (MapObjectAttachedObject obj in AttachedObjects)
            {
                if (obj.Alias.Contains("EndObject"))
                { obj.PositionOffset = new Vec3(Length, 0, 0); continue; }

                if (obj.Alias.Contains("StartObject"))
                { obj.PositionOffset = new Vec3(0, 0, 0); continue; }

                if ((!Type.UseAttachedObjectAsControlPoint) && (!fastbeam))
                {
                    if (obj.Alias.Contains("BControlPoint"))
                    { obj.PositionOffset = v_B; continue; }

                    if (obj.Alias.Contains("CControlPoint"))
                    { obj.PositionOffset = v_C; continue; }
                }
            }
        }

        void UpdateMeshVertices(Camera camera, bool fastbeam)
        {
            SubMesh subMesh = mesh.SubMeshes[0];
            Quat rot = this.Rotation; rot.Inverse();
            HardwareVertexBuffer vertexBuffer = subMesh.VertexData.VertexBufferBinding.GetBuffer(0);
            Vec3 v_dir = new Vec3(1, 0, 0);
            Vec3 v_pos = Vec3.Zero;

            Vec3 localnoise = Vec3.Zero;
            int pointnum = NumberOfPoints;
            bool doperpointfacing = Type.PerMainPointCameraFace;
            uint color;
            float width;
            float f_basedistance = Length;

            //init of lateral vector, facing cam.
            Vec3 tempy = GetLateralVector(this.Rotation, rot, v_pos, v_dir, camera);
            Vec3 tempz = Vec3.Cross(tempy, v_dir);


            unsafe
            {
                Vertex* buffer = (Vertex*)vertexBuffer.Lock(HardwareBuffer.LockOptions.Normal).ToPointer();
                subMesh.VertexData.VertexCount = pointnum * 2;
                // go through all points
                for (int i = 0; i < pointnum + 1; i++)
                {
                    v_pos = mainpoints[i].Pos;
                    v_dir = mainpoints[i].Dir;
                    color = mainpoints[i].PColor;
                    width = mainpoints[i].Width;

                    if (doperpointfacing) // recompute only if doperpointfacing.
                    {
                        tempy = GetLateralVector(this.Rotation, rot, v_pos, v_dir, camera);
                        //tempz = tempy.Cross(v_dir);
                        tempz = Vec3.Cross(tempy, v_dir);
                    }

                    *buffer = BuildBeamVertex(v_pos + (tempy * width / 2.0f), i * 1.0f, 0.0f, tempy, v_dir, color);
                    buffer++;
                    *buffer = BuildBeamVertex(v_pos - (tempy * width / 2.0f), i * 1.0f, 1.0f, tempy, v_dir, color);
                    buffer++;
                }

                if (DoubleShape)
                {

                    // go through all points, second pass.
                    for (int i = 0; i < pointnum + 1; i++)
                    {
                        // Get a point on the curve    
                        v_pos = mainpoints[i].Pos;
                        v_dir = mainpoints[i].Dir;
                        color = mainpoints[i].PColor;
                        width = mainpoints[i].Width;

                        if (doperpointfacing)
                        {
                            tempy = GetLateralVector(this.Rotation, rot, v_pos, v_dir, camera);
                            //tempz = tempy.Cross(v_dir);
                            tempz = Vec3.Cross(tempy, v_dir);

                        }

                        *buffer = BuildBeamVertex2(v_pos + (tempz * width / 2.0f), i * 1.0f, 0.0f, tempy, v_dir, color);
                        buffer++;
                        *buffer = BuildBeamVertex2(v_pos - (tempz * width / 2.0f), i * 1.0f, 1.0f, tempy, v_dir, color);
                        buffer++;
                    }
                }
                vertexBuffer.Unlock();
            }//end of unsafe bloc.
        }

        Vec3 GetLateralVector(Quat rot, Quat invertedrot, Vec3 point, Vec3 dir, Camera camera)
        {
            //taking camera position into account....
            Vec3 campos = (camera.Position);// - Position);
            Vec3 worldpoint = point * rot + Position;
            Vec3 worldcamdirtopoint = campos - worldpoint;
            Vec3 localcamdirtopoint = worldcamdirtopoint;
            localcamdirtopoint = localcamdirtopoint * invertedrot;
            localcamdirtopoint.Normalize();
            //Vec3 returnresult = localcamdirtopoint.Cross(dir);
            Vec3 returnresult = Vec3.Cross(localcamdirtopoint, dir);
            returnresult.Normalize();
            return returnresult;

        }

        void PerformRayCast()
        {
            if (!editormode)
            {
                Ray lookRay = new Ray(Position, Rotation.ToMat3().Item0);
                Vec3 lookFrom = lookRay.Origin;
                Vec3 lookDir = Vec3.Normalize(lookRay.Direction);
                if (useoverridenparam)
                    raycastdistance = length;
                else
                    raycastdistance = this.Type.Length;
                //List<RayCastResult> piercingResult = PhysicsWorld.Instance.RayCastPiercing( 
                RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    new Ray(lookFrom, lookDir * raycastdistance), (int)ContactGroup.CastOnlyContact);

                lastobjecthit = null;
                foreach (RayCastResult result in piercingResult)
                {
                    lastobjecthit = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);
                    raycastdistance = result.Distance;
                    lastraycastresult = result;
                    break;
                }
            }
        }

        /// <summary>
        /// normal version, for one shape
        /// </summary>
        /// <param name="Point"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="lateral"></param>
        /// <param name="dir"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        Vertex BuildBeamVertex(Vec3 Point, float u, float v, Vec3 lateral, Vec3 dir, uint color)
        {
            Vertex vertex = new Vertex();
            vertex.position = Point;
            //vertex.normal = lateral.Cross(dir); 
            vertex.normal = Vec3.Cross(lateral, dir);
            vertex.texCoord = new Vec2(u, v);
            vertex.color = color;
            return vertex;
        }

        /// <summary>
        /// second version, for the orthogonal shape
        /// </summary>
        /// <param name="Point"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="lateral"></param>
        /// <param name="dir"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        Vertex BuildBeamVertex2(Vec3 Point, float u, float v, Vec3 lateral, Vec3 dir, uint color)
        {
            Vertex vertex = new Vertex();
            vertex.position = Point;
            vertex.normal = lateral;
            vertex.texCoord = new Vec2(u, v);
            vertex.color = color;
            return vertex;
        }

        void UpdateMeshIndices()
        {
            if (mesh == null)
                return;

            SubMesh subMesh = mesh.SubMeshes[0];
            HardwareIndexBuffer indexBuffer = subMesh.IndexData.IndexBuffer;

            unsafe
            {
                ushort* buffer = (ushort*)indexBuffer.Lock(HardwareBuffer.LockOptions.Normal).ToPointer();

                subMesh.IndexData.IndexCount = 0;
                int curindex = 0;
                for (int i = 0; i < (NumberOfPoints); i++)
                {
                    curindex = i * 2;
                    *buffer = (ushort)(curindex); buffer++;
                    *buffer = (ushort)(curindex + 1); buffer++;
                    *buffer = (ushort)(curindex + 3); buffer++;
                    *buffer = (ushort)(curindex + 3); buffer++;
                    *buffer = (ushort)(curindex + 2); buffer++;
                    *buffer = (ushort)(curindex); buffer++;
                    subMesh.IndexData.IndexCount += 6;
                }

                if (DoubleShape)
                {
                    for (int i = 0; i < (NumberOfPoints); i++)
                    {
                        curindex = (NumberOfPoints + 1) * 2 + i * 2;
                        *buffer = (ushort)(curindex); buffer++;
                        *buffer = (ushort)(curindex + 1); buffer++;
                        *buffer = (ushort)(curindex + 3); buffer++;
                        *buffer = (ushort)(curindex + 3); buffer++;
                        *buffer = (ushort)(curindex + 2); buffer++;
                        *buffer = (ushort)(curindex); buffer++;
                        subMesh.IndexData.IndexCount += 6;
                    }

                }
                indexBuffer.Unlock();
            }

        }
        #endregion

        #region Events
        void RenderSystem_RenderSystemEvent(RenderSystemEvents name)
        {
            if (name == RenderSystemEvents.DeviceRestored)
            {
                needUpdateVertices = true;
                needUpdateIndices = true;
            }
        }
        #endregion

    }
}

