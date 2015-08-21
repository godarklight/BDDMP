using System;
using System.Collections.Generic;
using UnityEngine;
using BahaTurret;
using DarkMultiPlayer;
using MessageStream2;


namespace BDDMP
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class BDDMPSynchronizer : MonoBehaviour
	{
		static BDDMPSynchronizer singleton;
        //FX Sync
        const int syncFXHz = 40;
        static float lastFXSync = 0;
        static int tickCount = 0;
        const int updateHistoryMinutesToLive = 3;

        /* TODO
        bool cmPoolInited = false;
        ObjectPool flarePool;
        ObjectPool chaffPool;
        ObjectPool smokePool;
        */
         
        //Update Entries
        static List<BDArmoryDamageUpdate> damageEntries = new List<BDArmoryDamageUpdate> ();
        static List<BDArmoryBulletHitUpdate> bulletHitEntries = new List<BDArmoryBulletHitUpdate> ();
        static List<BDArmoryExplosionUpdate> explosionEntries = new List<BDArmoryExplosionUpdate> ();
        static List<BDArmoryTracerInitUpdate> tracerInitEntries = new List<BDArmoryTracerInitUpdate>();
        static List<BDArmoryTracerUpdate> tracerEntries = new List<BDArmoryTracerUpdate> ();
        static List<BDArmoryTracerDestroyUpdate> tracerDestroyEntries = new List<BDArmoryTracerDestroyUpdate>();
        static List<BDArmoryTurretRotUpdate> turretYawEntries = new List<BDArmoryTurretRotUpdate>();
        static List<BDArmoryTurretRotUpdate> turretPitchEntries = new List<BDArmoryTurretRotUpdate>();
        static List<BDArmoryTurretDeployUpdate> turretDeployEntries = new List<BDArmoryTurretDeployUpdate>();
        static List<BDArmoryLaserUpdate> laserEntries = new List<BDArmoryLaserUpdate>();
        static List<BDArmoryFlareUpdate> flareEntries = new List<BDArmoryFlareUpdate>();

        //Update Completion Entries
        static List<BDArmoryDamageUpdate> damageEntriesCompleted = new List<BDArmoryDamageUpdate> ();
        static List<BDArmoryBulletHitUpdate> bulletHitEntriesCompleted = new List<BDArmoryBulletHitUpdate> ();
        static List<BDArmoryExplosionUpdate> explosionEntriesCompleted = new List<BDArmoryExplosionUpdate> ();
        static List<BDArmoryTracerInitUpdate> tracerInitEntriesCompleted = new List<BDArmoryTracerInitUpdate>();
        static List<BDArmoryTracerUpdate> tracerEntriesCompleted = new List<BDArmoryTracerUpdate>();
        static List<BDArmoryTracerDestroyUpdate> tracerDestroyEntriesCompleted = new List<BDArmoryTracerDestroyUpdate>();
        static List<BDArmoryTurretRotUpdate> turretYawEntriesCompleted = new List<BDArmoryTurretRotUpdate>();
        static List<BDArmoryTurretRotUpdate> turretPitchEntriesCompleted = new List<BDArmoryTurretRotUpdate>();
        static List<BDArmoryTurretDeployUpdate> turretDeployEntriesCompleted = new List<BDArmoryTurretDeployUpdate>();
        static List<BDArmoryLaserUpdate> laserEntriesCompleted = new List<BDArmoryLaserUpdate>();
        static List<BDArmoryFlareUpdate> flareEntriesCompleted = new List<BDArmoryFlareUpdate>();

        //Tracers
        static List<BDArmouryTracer> tracers = new List<BDArmouryTracer>();

        //Combinator pool
        static Dictionary<FlareObject, double> flares = new Dictionary<FlareObject, double>(); System.Object flareLock = new System.Object();   

		public BDDMPSynchronizer ()
		{
			singleton = this;
		}

		public void Awake()
		{
			GameObject.DontDestroyOnLoad (this);

            //Message Registration
            DMPModInterface.fetch.RegisterRawModHandler ("BDDMP:DamageHook", HandleDamageHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:MultiDamageHook", HandleMultiDamageHook);
            DMPModInterface.fetch.RegisterRawModHandler ("BDDMP:BulletHitFXHook", HandleBulletHitFXHook);
            DMPModInterface.fetch.RegisterRawModHandler ("BDDMP:ExplosionFXHook", HandleExplosionFXHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:TurretPitchHook", HandleTurretPitchHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:TurretYawHook", HandleTurretYawHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:TurretDeployHook", HandleTurretDeployHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:BulletTracerInitHook", HandleBulletTracerInitHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:BulletTracerHook", HandleBulletTracerHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:BulletTracerDestroyHook", HandleBulletTracerDestroyHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:LaserHook", HandleLaserHook);
            DMPModInterface.fetch.RegisterRawModHandler("BDDMP:FlareHook", HandleFlareHook);

            //Hook Registration
            HitManager.RegisterHitHook (DamageHook);
            HitManager.RegisterMultiHitHook(MultiDamageHook);
            HitManager.RegisterBulletHook (BulletHitFXHook);
            HitManager.RegisterExplosionHook (ExplosionFXHook);
            HitManager.RegisterTurretYawHook(TurretYawHook);
            HitManager.RegisterTurretPitchHook(TurretPitchHook);
            HitManager.RegisterTurretDeployHook(TurretDeployHook);
            HitManager.RegisterTracerInitHook(BulletTracerInitHook);
            HitManager.RegisterTracerHook (BulletTracerHook);
            HitManager.RegisterTracerDestroyHook(BulletTracerDestroyHook);
            HitManager.RegisterLaserHook(LaserHook);
            HitManager.RegisterFlareHook(FlareHook);

            HitManager.RegisterAllowControlHook(CanControl);
            HitManager.RegisterAllowDamageHook (VesselCanBeDamaged);

        }

        public void Update()
        {
            /*
            #region CM Init
            if (!cmPoolInited && GameDatabase.Instance.IsReady())
            {
                try
                {
                    GameObject cm = (GameObject)Instantiate(GameDatabase.Instance.GetModel("BDArmory/Models/CMFlare/model"));
                    cm.SetActive(false);
                    cm.AddComponent<CMFlare>();
                    flarePool = ObjectPool.CreateObjectPool(cm, 10, true, true);

                    cm = (GameObject)Instantiate(GameDatabase.Instance.GetModel("BDArmory/Models/CMSmoke/cmSmokeModel"));
                    cm.SetActive(false);
                    cm.AddComponent<CMSmoke>();
                    smokePool = ObjectPool.CreateObjectPool(cm, 10, true, true);

                    cm = (GameObject)Instantiate(GameDatabase.Instance.GetModel("BDArmory/Models/CMChaff/model"));
                    cm.SetActive(false);
                    cm.AddComponent<CMChaff>();
                    chaffPool = ObjectPool.CreateObjectPool(cm, 10, true, true);

                    cmPoolInited = true;
                }
                catch (ArgumentException) { cmPoolInited = false; } //GameDB not loaded
            }
            #endregion
            */

            PurgeDamageUpdates();
            PurgeBulletHitUpdates();
            PurgeExplosionUpdates();
            PurgeTracerInitUpdates();
            PurgeTracerUpdates();
            PurgeTracerDestroyUpdates();
            PurgeYawUpdates();
            PurgePitchUpdates();
            PurgeDeployUpdates();
            PurgeLaserUpdates();
            PurgeFlareUpdates();

            CombineFlares();

            UpdateDamage ();
            UpdateBulletHit ();
            UpdateExplosion ();
            UpdateTracerInit ();
            UpdateTracer ();
            UpdateTracerDestroy ();
            UpdateTurretDeploy();
            UpdateTurretYaw ();
            UpdateTurretPitch ();
            UpdateLaser ();
            UpdateFlare();

            PurgeTracers ();
        }


        #region Update Functions

        #region Purge Functions

        private void PurgeDamageUpdates()
        {
            foreach (BDArmoryDamageUpdate update in damageEntriesCompleted)
            {
                damageEntries.Remove(update);
            }
            foreach (BDArmoryDamageUpdate update in damageEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    damageEntries.Remove(update);
                }
            }
            damageEntriesCompleted.Clear();
        }

        private void PurgeBulletHitUpdates()
        {
            foreach (BDArmoryBulletHitUpdate update in bulletHitEntriesCompleted)
            {
                bulletHitEntries.Remove(update);
            }
            foreach (BDArmoryBulletHitUpdate update in bulletHitEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    bulletHitEntries.Remove(update);
                }
            }
            bulletHitEntriesCompleted.Clear();
        }

        private void PurgeExplosionUpdates()
        {
            foreach (BDArmoryExplosionUpdate update in explosionEntriesCompleted)
            {
                explosionEntries.Remove(update);
            }
            foreach (BDArmoryExplosionUpdate update in explosionEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    explosionEntries.Remove(update);
                }
            }
            explosionEntriesCompleted.Clear();
        }

        private void PurgeTracerInitUpdates()
        {
            foreach (BDArmoryTracerInitUpdate update in tracerInitEntriesCompleted)
            {
                tracerInitEntries.Remove(update);
            }
            foreach (BDArmoryTracerInitUpdate update in tracerInitEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    tracerInitEntries.Remove(update);
                }
            }
            tracerInitEntriesCompleted.Clear();
        }

        private void PurgeTracerUpdates()
        {
            foreach (BDArmoryTracerUpdate update in tracerEntriesCompleted)
            {
                tracerEntries.Remove(update);
            }

            foreach (BDArmoryTracerUpdate update in tracerEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    tracerEntries.Remove(update);
                }
            }
            tracerEntriesCompleted.Clear();
        }

        private void PurgeTracerDestroyUpdates()
        {
            foreach (BDArmoryTracerDestroyUpdate update in tracerDestroyEntriesCompleted)
            {
                tracerDestroyEntries.Remove(update);
            }
            foreach (BDArmoryTracerDestroyUpdate update in tracerDestroyEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    tracerDestroyEntries.Remove(update);
                }
            }
            tracerDestroyEntriesCompleted.Clear();
        }

        private void PurgeYawUpdates()
        {
            foreach (BDArmoryTurretRotUpdate update in turretYawEntriesCompleted)
            {
                turretYawEntries.Remove(update);
            }
            foreach (BDArmoryTurretRotUpdate update in turretYawEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    turretYawEntries.Remove(update);
                }
            }
            turretYawEntriesCompleted.Clear();
        }

        private void PurgePitchUpdates()
        {
            foreach (BDArmoryTurretRotUpdate update in turretPitchEntriesCompleted)
            {
                turretPitchEntries.Remove(update);
            }
            foreach (BDArmoryTurretRotUpdate update in turretPitchEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    turretPitchEntries.Remove(update);
                }
            }
            turretPitchEntriesCompleted.Clear();
        }

        private void PurgeDeployUpdates()
        {
            foreach (BDArmoryTurretDeployUpdate update in turretDeployEntriesCompleted)
            {
                turretDeployEntries.Remove(update);
            }
            foreach (BDArmoryTurretDeployUpdate update in turretDeployEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    turretDeployEntries.Remove(update);
                }
            }
            turretDeployEntriesCompleted.Clear();
        }

        private void PurgeLaserUpdates()
        {
            foreach (BDArmoryLaserUpdate update in laserEntriesCompleted)
            {
                laserEntries.Remove(update);
            }
            foreach (BDArmoryLaserUpdate update in laserEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    laserEntries.Remove(update);
                }
            }
            laserEntriesCompleted.Clear();
        }

        private void PurgeFlareUpdates()
        {
            foreach (BDArmoryFlareUpdate update in flareEntriesCompleted)
            {
                flareEntries.Remove(update);
            }
            foreach (BDArmoryFlareUpdate update in flareEntries)
            {
                //If update is older than 3 seconds, purge it
                if (Planetarium.GetUniversalTime() - update.entryTime > updateHistoryMinutesToLive * 60)
                {
                    flareEntries.Remove(update);
                }
            }
            flareEntriesCompleted.Clear();
        }

        private void PurgeTracers()
        {
            //Cull all desynced Tracers 
            foreach (BDArmouryTracer tracer in tracers.ToArray())
            {
                try
                {
                    if (Planetarium.GetUniversalTime() - tracer.lastUpdateTime > 20)
                    {
                        tracers.Remove(tracer);
                        GameObject.Destroy(tracer.tracer);
                    }
                }
                catch (NullReferenceException)
                {
                    tracers.Remove(tracer);
                }
            }
        }

        #endregion

        #region Combinators

        private void CombineFlares()
        {
            if (flares.Count > 0)
            {
                lock (flareLock)
                {
                    using (MessageWriter mw = new MessageWriter())
                    {
                        mw.Write<int>(flares.Count);
                        foreach (KeyValuePair<FlareObject, double> flare in flares)
                        {
                            mw.Write<double>(flare.Value);

                            mw.Write<float>(flare.Key.pos.x);
                            mw.Write<float>(flare.Key.pos.y);
                            mw.Write<float>(flare.Key.pos.z);

                            mw.Write<float>(flare.Key.rot.x);
                            mw.Write<float>(flare.Key.rot.y);
                            mw.Write<float>(flare.Key.rot.z);
                            mw.Write<float>(flare.Key.rot.w);

                            mw.Write<float>(flare.Key.vel.x);
                            mw.Write<float>(flare.Key.vel.y);
                            mw.Write<float>(flare.Key.vel.z);

                            mw.Write<string>(flare.Key.sourceVessel.ToString());
                        }
                        DMPModInterface.fetch.SendDMPModMessage("BDDMP:FlareHook", mw.GetMessageBytes(), true, true);
                        flares.Clear();
                    }

                }
            }
        }

        #endregion

        private void UpdateDamage()
        {
            //Iterate over updates
            foreach (BDArmoryDamageUpdate update in damageEntries) {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryDamageUpdate> (update)) {
                    foreach (Vessel vessel in FlightGlobals.Vessels) {
                        if (vessel.id == update.vesselID) {
                            DarkLog.Debug("DAMAGE: Found Target Vessel");
                            foreach (Part part in vessel.Parts) {
                                if (part.flightID == update.flightID) {
                                    part.temperature = update.tempurature;
                                    part.vessel.externalTemperature = update.externalTempurature;
                                    DarkLog.Debug("DAMAGE: Found And Changed Dammaged Part");
                                }
                            }
                        }
                    }
                    damageEntriesCompleted.Add (update);
                }
            }
        }

        private void UpdateBulletHit()
        {
            //Iterate over updates
            foreach (BDArmoryBulletHitUpdate update in bulletHitEntries) {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryBulletHitUpdate> (update)) {
                    Vector3 relPosition = new Vector3 ();
                    bool positionSet = false;

                    if (FlightGlobals.ActiveVessel.id == update.vesselOriginID)
                    {
                        relPosition = update.position + FlightGlobals.ActiveVessel.transform.position;
                        positionSet = true;
                    }
                    else
                    {
                        foreach (Vessel vessel in FlightGlobals.Vessels)
                        {
                            if (vessel.id == update.vesselOriginID)
                            {
                                relPosition = update.position + vessel.transform.position;
                                positionSet = true;
                            }
                        }
                    }

                    if (!positionSet) {
                        DarkLog.Debug ("BDDMP Could not find basis vessel!");
                        return;
                    }


                    if (HighLogic.LoadedScene == GameScenes.FLIGHT) {
                        try {
                            //BulletHitFX.CreateBulletHit (relPosition, norm, rico, false);
                            GameObject go = GameDatabase.Instance.GetModel("BDArmory/Models/bulletHit/bulletHit");
                            GameObject newExplosion = (GameObject) GameObject.Instantiate(go, relPosition, Quaternion.LookRotation(update.normalDirection));
                            //Debug.Log ("CreateBulletHit instantiating at position X: " + position.x + " Y: " + position.y + " Z: " + position.z);
                            newExplosion.SetActive(true);
                            newExplosion.AddComponent<BulletHitFX>();
                            newExplosion.GetComponent<BulletHitFX>().ricochet = update.ricochet;
                            foreach(KSPParticleEmitter pe in newExplosion.GetComponentsInChildren<KSPParticleEmitter>())
                            {
                                pe.emit = true;
                                pe.force = (4.49f * FlightGlobals.getGeeForceAtPosition(relPosition));
                            }
                        } catch (Exception e) {
                            DarkLog.Debug ("BDDMP Exception while trying to spawn bullet effect " + e.Message);
                        }
                    }
                    bulletHitEntriesCompleted.Add (update);
                }
            }
        }

        private void UpdateExplosion()
        {
            //Iterate over updates
            foreach (BDArmoryExplosionUpdate update in explosionEntries) {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryExplosionUpdate> (update)) {
                    if (HighLogic.LoadedScene == GameScenes.FLIGHT) {
                        foreach (Vessel vessel in FlightGlobals.Vessels) {
                            if (vessel.id == update.vesselOriginID) {
                                ExplosionFX.CreateExplosion ((vessel.transform.position + update.position), update.radius, update.power, vessel, update.direction, update.explModelPath, update.soundPath, false);
                            }
                        }
                    }

                    explosionEntriesCompleted.Add (update);
                }
            }
        }

        private void UpdateTracerInit()
        {
            //Iterate over updates
            foreach (BDArmoryTracerInitUpdate update in tracerInitEntries) {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryTracerInitUpdate> (update)) {

                    BDArmouryTracer tracer = new BDArmouryTracer();

                    tracer.id = update.tracerID;
                    tracer.lastUpdateTime = Planetarium.GetUniversalTime();

                    foreach (Vessel v in FlightGlobals.Vessels.ToArray())
                    {
                        if (v.id == update.vesselID)
                        {
                            DarkLog.Debug("TRACER: Found Target Vessel");
                            tracer.offset = v.transform.position;
                        }
                    }

                    tracer.tracer = new GameObject();
                    
                    Light light = tracer.tracer.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = Misc.ParseColor255("255, 235, 145, 255");
                    light.range = 8;
                    light.intensity = 1;

                    LineRenderer lr = tracer.tracer.AddComponent<LineRenderer>();
                    lr.SetVertexCount(2);
                    lr.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
                    lr.material.mainTexture = GameDatabase.Instance.GetTexture(update.bulletTexPath, false);

                    tracers.Add(tracer);

                    tracerInitEntriesCompleted.Add (update);
                }
            }

        }

        private void UpdateTracer()
        {
            //Iterate over updates
            foreach (BDArmoryTracerUpdate update in tracerEntries)
            {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryTracerUpdate>(update))
                {

                    foreach (BDArmouryTracer tracer in tracers)
                    {
                        if (tracer.id == update.tracerID)
                        {
                            tracer.lastUpdateTime = Planetarium.GetUniversalTime();

                            LineRenderer lr = tracer.tracer.GetComponent<LineRenderer>();
                            lr.SetPosition(0, update.p1 + tracer.offset);
                            lr.SetPosition(1, update.p2 + tracer.offset);
                            lr.material.SetColor("_TintColor", update.color);
                            lr.SetWidth(update.w1, update.w2);

                            Light light = tracer.tracer.GetComponent<Light>();
                            light.transform.position = update.p1 + tracer.offset;
                        }
                    }

                    tracerEntriesCompleted.Add(update);
                }
            }

        }

        private void UpdateTracerDestroy()
        {
            //Iterate over updates
            foreach (BDArmoryTracerDestroyUpdate update in tracerDestroyEntries)
            {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryTracerDestroyUpdate>(update))
                {
                    foreach (BDArmouryTracer tracer in tracers.ToArray())
                    {
                        if (tracer.id == update.tracerID) {
                            tracers.Remove(tracer);
                            GameObject.Destroy(tracer.tracer);
                        }
                    }

                    tracerDestroyEntriesCompleted.Add(update);
                }
            }

        }

        private void UpdateTurretYaw()
        {
            //Iterate over updates
            foreach (BDArmoryTurretRotUpdate update in turretYawEntries)
            {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryTurretRotUpdate>(update))
                {
                    foreach (Vessel v in FlightGlobals.Vessels.ToArray())
                    {
                        if (v.id == update.vesselID)
                        {
                            //DarkLog.Debug("YAW: Found Target Vessel");
                            foreach (Part p in v.Parts.ToArray())
                            {
                                if (p.craftID == update.turretID)
                                {
                                    p.GetComponent<ModuleTurret>().yawTransform.localRotation = update.rot;
                                    //DarkLog.Debug("YAW: Found And Changed Turret");
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    turretYawEntriesCompleted.Add(update);
                }
            }

        }

        private void UpdateTurretPitch()
        {
            //Iterate over updates
            foreach (BDArmoryTurretRotUpdate update in turretPitchEntries)
            {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryTurretRotUpdate>(update))
                {
                    foreach (Vessel v in FlightGlobals.Vessels.ToArray())
                    {
                        if (v.id == update.vesselID)
                        {
                            //DarkLog.Debug("PITCH: Found Target Vessel");
                            foreach (Part p in v.Parts.ToArray())
                            {
                                if (p.craftID == update.turretID)
                                {
                                    p.GetComponent<ModuleTurret>().pitchTransform.localRotation = update.rot;
                                    //DarkLog.Debug("PITCH: Found And Changed Turret");
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    turretPitchEntriesCompleted.Add(update);
                }
            }
        }

        private void UpdateTurretDeploy()
        {
            //Iterate over updates
            foreach (BDArmoryTurretDeployUpdate update in turretDeployEntries)
            {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryTurretDeployUpdate>(update))
                {
                    foreach (Vessel v in FlightGlobals.Vessels.ToArray())
                    {
                        if (v.id == update.vesselID)
                        {
                            DarkLog.Debug("DEPLOY: Found Target Vessel");
                            foreach (Part p in v.Parts.ToArray())
                            {
                                if (p.craftID == update.turretID)
                                {
                                    //p.GetComponent<ModuleWeapon>().deployState.enabled = update.state;
                                    //p.GetComponent<ModuleWeapon>().dmpSlave = update.state;
                                    DarkLog.Debug("DEPLOY: Found And Changed Turret");
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    turretDeployEntriesCompleted.Add(update);
                }
            }
        }

        private void UpdateLaser()
        {
            //Iterate over updates
            foreach (BDArmoryLaserUpdate update in laserEntries)
            {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryLaserUpdate>(update))
                {
                    foreach (Vessel v in FlightGlobals.Vessels.ToArray())
                    {
                        if (v.id == update.vesselID)
                        {
                            DarkLog.Debug("LASER: Found Target Vessel");
                            foreach (Part p in v.Parts.ToArray())
                            {
                                if (p.craftID == update.turretID)
                                {
                                    /*
                                    if (!(update.p1 == Vector3.zero && update.p2 == Vector3.zero))
                                    {
                                        p.GetComponent<ModuleWeapon>().dmpFakeLaser = true;

                                        for (int i = 0; i < p.GetComponent<ModuleWeapon>().laserRenderers.Length; i++)
                                        {
                                            p.GetComponent<ModuleWeapon>().laserRenderers[i].enabled = true;
                                        }

                                        //TODO Add Sound
                                        for (int i = 0; i < p.GetComponent<ModuleWeapon>().fireTransforms.Length; i++)
                                        {
                                             Transform tf = p.GetComponent<ModuleWeapon>().fireTransforms[i];

                                             LineRenderer lr = p.GetComponent<ModuleWeapon>().laserRenderers[i];

                                             lr.SetPosition(0, update.p1 + tf.position);
                                             lr.SetPosition(1, update.p2 + v.gameObject.transform.position);
                                         }
                                         DarkLog.Debug("LASER: Found And Changed Turret");
                                         break;
                                     }
                                     else
                                     {
                                         p.GetComponent<ModuleWeapon>().dmpFakeLaser = false;
                                     }
                                    */
                                }
                                    
                            }
                            break;
                        }
                    }
                    laserEntriesCompleted.Add(update);
                }
            }
        }

        private void UpdateFlare()
        {
            //Iterate over updates
            foreach (BDArmoryFlareUpdate update in flareEntries)
            {
                //Don't apply updates till they happen
                if (ApplyUpdate<BDArmoryFlareUpdate>(update))
                {
                    foreach (Vessel v in FlightGlobals.Vessels.ToArray())
                    {
                        if (v.id == update.vesselID)
                        {
                            DarkLog.Debug("FLARE: Found Target Vessel");
                            GameObject cm = (GameObject)Instantiate(GameDatabase.Instance.GetModel("BDArmory/Models/CMFlare/model"));
                            cm.transform.position = update.pos + v.transform.position;
                            cm.transform.rotation = update.rot;
                            CMFlare cmf = cm.AddComponent<CMFlare>();
                            cmf.startVelocity = update.vel;
                            cmf.sourceVessel = v;

                            cm.SetActive(true);
                            DarkLog.Debug("FLARE: Created Flare");
                            break;
                        }
                    }
                    flareEntriesCompleted.Add(update);
                }
            }
        }

        #endregion

        #region Network Code

        #region Damage
        #region Single
        void DamageHook(Part hitPart)
        {
            //DarkLog.Debug ("BDDMP Asked to handle HitHook!");
            using (MessageWriter mw = new MessageWriter ()) {
                mw.Write<double> (Planetarium.GetUniversalTime ());
                mw.Write<string> (hitPart.vessel.id.ToString ());
                mw.Write<uint> (hitPart.flightID);
                mw.Write<double> (hitPart.temperature);
                mw.Write<double> (hitPart.vessel.externalTemperature);

                DMPModInterface.fetch.SendDMPModMessage("BDDMP:DamageHook", mw.GetMessageBytes(), true, true);
            }
        }

        void HandleDamageHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader (messageData)) {
                double timeStamp = mr.Read<double> ();

                Guid vesselID = new Guid (mr.Read<string> ());

                uint partID = mr.Read<uint> ();

                double partTemp = mr.Read<double> ();

                double partTempExt = mr.Read<double> ();

                BDArmoryDamageUpdate update = new BDArmoryDamageUpdate (timeStamp, vesselID, partID, partTemp, partTempExt);

                damageEntries.Add (update);

            }
        }
        #endregion

        #region Multi
        void MultiDamageHook(List<Part> hitParts)
        {
            //DarkLog.Debug ("BDDMP Asked to handle HitHook!");
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<double>(Planetarium.GetUniversalTime());
                mw.Write<int>(hitParts.Count);
                foreach (Part hitPart in hitParts)
                {
                    mw.Write<string>(hitPart.vessel.id.ToString());
                    mw.Write<uint>(hitPart.flightID);
                    mw.Write<double>(hitPart.temperature);
                    mw.Write<double>(hitPart.vessel.externalTemperature);
                }
                DMPModInterface.fetch.SendDMPModMessage("BDDMP:MultiDamageHook", mw.GetMessageBytes(), true, true);
            }
        }

        void HandleMultiDamageHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                double timeStamp = mr.Read<double>();
                int hitCount = mr.Read<int>();
                while (hitCount > 0)
                {
                    Guid vesselID = new Guid(mr.Read<string>());
                    uint partID = mr.Read<uint>();
                    double partTemp = mr.Read<double>();
                    double partTempExt = mr.Read<double>();

                    BDArmoryDamageUpdate update = new BDArmoryDamageUpdate(timeStamp, vesselID, partID, partTemp, partTempExt);
                    damageEntries.Add(update);

                    hitCount--;
                }

            }
        }
        #endregion

        #endregion

        #region Bullet Hit FX
        void BulletHitFXHook(BulletObject bullet)
        {
            //Only send per fx tick rate
            bool clearToSend = false || Time.realtimeSinceStartup - lastFXSync > (1f / syncFXHz);
            if (tickCount == syncFXHz && (Time.realtimeSinceStartup - lastFXSync) >= 1) {
                tickCount = 0;
            }

            if (clearToSend) {
                //Set lastFXSync right away
                lastFXSync = Time.realtimeSinceStartup;

                //DarkLog.Debug ("BDDMP Asked to handle BulletHook!");
                using (MessageWriter mw = new MessageWriter ()) {
                    //Get position in world coordinates
                    //Vector3 vesselPositionBullet = FlightGlobals.ActiveVessel.mainBody.bodyTransform.TransformPoint (bullet.position);
                    Vector3 vesselPositionBullet = bullet.position - FlightGlobals.ActiveVessel.transform.position;

                    mw.Write<double> (Planetarium.GetUniversalTime ());
                    mw.Write<string> (FlightGlobals.ActiveVessel.id.ToString ());
                    mw.Write<float> (vesselPositionBullet.x);
                    mw.Write<float> (vesselPositionBullet.y);
                    mw.Write<float> (vesselPositionBullet.z);
                    mw.Write<float> (bullet.normalDirection.x);
                    mw.Write<float> (bullet.normalDirection.y);
                    mw.Write<float> (bullet.normalDirection.z);
                    mw.Write<bool> (bullet.ricochet);

                    DMPModInterface.fetch.SendDMPModMessage ("BDDMP:BulletHitFXHook", mw.GetMessageBytes (), true, false);
                }
                tickCount++;
            }
        }

        void HandleBulletHitFXHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                double timeStamp = mr.Read<double> ();

                Guid baseVessel = new Guid (mr.Read<string> ());
                
                float posX = mr.Read<float>();
                float posY = mr.Read<float>();
                float posZ = mr.Read<float>();
                Vector3 pos = new Vector3 (posX, posY, posZ);

                float normX = mr.Read<float>();
                float normY = mr.Read<float>();
                float normZ = mr.Read<float>();
                Vector3 norm = new Vector3 (normX, normY, normZ);

                bool rico = mr.Read<bool> ();

                BDArmoryBulletHitUpdate update = new BDArmoryBulletHitUpdate (timeStamp, baseVessel, pos, norm, rico);
                bulletHitEntries.Add (update);
            }
        }
        #endregion

        #region Explosion FX
        void ExplosionFXHook(ExplosionObject explosion)
        {
            //Reset tickCount at beginning of Hook
            if (tickCount == syncFXHz && (Time.realtimeSinceStartup - lastFXSync) >= 1) {
                tickCount = 0;
            }

            //Only send per fx tick rate
            bool clearToSend = false || Time.realtimeSinceStartup - lastFXSync > (1f / (float)syncFXHz) * (float)tickCount;

            if (clearToSend) {
                //Set lastFXSync and raise tick count right away
                lastFXSync = Time.realtimeSinceStartup;
                tickCount++;

                //DarkLog.Debug ("BDDMP Asked to handle ExplosionHook!");
                using (MessageWriter mw = new MessageWriter ()) {
                    //Get position in world coordinates
                    Vector3 vesselPositionExplosion = explosion.position - explosion.sourceVessel.transform.position;

                    mw.Write<double> (Planetarium.GetUniversalTime ());
                    mw.Write<float> (vesselPositionExplosion.x);
                    mw.Write<float> (vesselPositionExplosion.y);
                    mw.Write<float> (vesselPositionExplosion.z);
                    mw.Write<float> (explosion.raduis);
                    mw.Write<float> (explosion.power);
                    mw.Write<string> (explosion.sourceVessel.id.ToString ());
                    mw.Write<float> (explosion.direction.x);
                    mw.Write<float> (explosion.direction.y);
                    mw.Write<float> (explosion.direction.z);
                    mw.Write<string> (explosion.explModelPath);
                    mw.Write<string> (explosion.soundPath);

                    DMPModInterface.fetch.SendDMPModMessage ("BDDMP:ExplosionFXHook", mw.GetMessageBytes (), true, true);
                }
                tickCount++;
            }
        }

        void HandleExplosionFXHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader (messageData)) {
                double timeStamp = mr.Read<double> ();

                float posX = mr.Read<float>();
                float posY = mr.Read<float>();
                float posZ = mr.Read<float>();
                Vector3 pos = new Vector3(posX, posY, posZ);

                float radi = mr.Read<float>();

                float power = mr.Read<float> ();

                Guid vesselGUID = new Guid(mr.Read<string>());

                float dirX = mr.Read<float>();
                float dirY = mr.Read<float>();
                float dirZ = mr.Read<float>();
                Vector3 dir = new Vector3 (dirX, dirY, dirZ);

                string explPath = mr.Read<string>();
                string soundPath = mr.Read<string>();

                BDArmoryExplosionUpdate update = new BDArmoryExplosionUpdate (timeStamp, pos, vesselGUID, radi, power, dir, explPath, soundPath);
                explosionEntries.Add (update);

            }
        }
        #endregion

        #region Tracer FX All

        #region Tracer Init

        void BulletTracerInitHook(InitTracerObject bullet)
        {
            using (MessageWriter mw = new MessageWriter ()) {

                mw.Write<double> (Planetarium.GetUniversalTime ());

                mw.Write<string>(bullet.tracerID.ToString());
                mw.Write<string>(bullet.vesselID.ToString());
                mw.Write<uint>(bullet.turretID);
                mw.Write<string>(bullet.bulletTexPath);

                DMPModInterface.fetch.SendDMPModMessage ("BDDMP:BulletTracerInitHook", mw.GetMessageBytes (), true, true);
            }
        }

        void HandleBulletTracerInitHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader (messageData)) {
                double timeStamp = mr.Read<double>();

                Guid tracerID = new Guid(mr.Read<string>());
                Guid vesselID = new Guid(mr.Read<string>());
                uint turretID = mr.Read<uint>();
                string bulletTexPath = mr.Read<string>();

                BDArmoryTracerInitUpdate update = new BDArmoryTracerInitUpdate(timeStamp, bulletTexPath, tracerID, vesselID, turretID);
                tracerInitEntries.Add(update);
            }
        }
        #endregion

        #region Tracer Update
        void BulletTracerHook(UpdateTracerObject bullet)
        {
            using (MessageWriter mw = new MessageWriter())
            {

                mw.Write<double>(Planetarium.GetUniversalTime());

                mw.Write<float>(bullet.p1.x);
                mw.Write<float>(bullet.p1.y);
                mw.Write<float>(bullet.p1.z);

                mw.Write<float>(bullet.p2.x);
                mw.Write<float>(bullet.p2.y);
                mw.Write<float>(bullet.p2.z);

                mw.Write<float>(bullet.color.r);
                mw.Write<float>(bullet.color.g);
                mw.Write<float>(bullet.color.b);
                mw.Write<float>(bullet.color.a);

                mw.Write<float>(bullet.width1);
                mw.Write<float>(bullet.width2);

                mw.Write<string>(bullet.tracerID.ToString());

                DMPModInterface.fetch.SendDMPModMessage("BDDMP:BulletTracerHook", mw.GetMessageBytes(), true, false);
            }
        }

        void HandleBulletTracerHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                // Using least var creation
                double timeStamp = mr.Read<double>();

                float x = mr.Read<float>();
                float y = mr.Read<float>();
                float z = mr.Read<float>();
                Vector3 p1 = new Vector3(x, y, z);

                x = mr.Read<float>();
                y = mr.Read<float>();
                z = mr.Read<float>();
                Vector3 p2 = new Vector3(x, y, z);
                
                x = mr.Read<float>();
                y = mr.Read<float>();
                z = mr.Read<float>();
                float w = mr.Read<float>();
                Color color = new Color(x, y, z, w);

                w = mr.Read<float>();
                float w2 = mr.Read<float>();

                Guid tracer = new Guid(mr.Read<string>());

                BDArmoryTracerUpdate update = new BDArmoryTracerUpdate(timeStamp, p1, p2, color, w, w2, tracer);
                tracerEntries.Add(update);
            }
        }
        #endregion

        #region Tracer Destroy
        void BulletTracerDestroyHook(Guid bullet)
        {
            using (MessageWriter mw = new MessageWriter())
            {

                mw.Write<double>(Planetarium.GetUniversalTime());
                mw.Write<string>(bullet.ToString());

                DMPModInterface.fetch.SendDMPModMessage("BDDMP:BulletTracerDestroyHook", mw.GetMessageBytes(), true, true);
            }
        }

        void HandleBulletTracerDestroyHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                double timeStamp = mr.Read<double>();
                Guid tracer = new Guid(mr.Read<string>());

                BDArmoryTracerDestroyUpdate update = new BDArmoryTracerDestroyUpdate(timeStamp, tracer);
                tracerDestroyEntries.Add(update);
            }
        }
        #endregion

        #endregion

        #region Turret Yaw

        void TurretYawHook(Quaternion rot, Guid vesselID, uint turretID) {
            /*
            //Reset tickCount at beginning of Hook
            if (turretTickCount == syncTurretHz && (Time.realtimeSinceStartup - lastTurretsync) >= 1)
            {
                turretTickCount = 0;
            }

            //Only send per fx tick rate
            bool clearToSend = false || Time.realtimeSinceStartup - lastTurretsync > (1f / (float)syncTurretHz) * (float)turretTickCount;

            if (clearToSend)
            {
                //Set lastFXSync and raise tick count right away
                lastTurretsync = Time.realtimeSinceStartup;
                turretTickCount++;
            */
                using (MessageWriter mw = new MessageWriter())
                {
                    mw.Write<double>(Planetarium.GetUniversalTime());

                    mw.Write<float>(rot.x);
                    mw.Write<float>(rot.y);
                    mw.Write<float>(rot.z);
                    mw.Write<float>(rot.w);

                    mw.Write<string>(vesselID.ToString());

                    mw.Write<uint>(turretID);

                    DMPModInterface.fetch.SendDMPModMessage("BDDMP:TurretYawHook", mw.GetMessageBytes(), true, true);
                }
            //}
            //turretTickCount++;
        }

        void HandleTurretYawHook(byte[] messageData) {
            using (MessageReader mr = new MessageReader(messageData))
            {
                double timeStamp = mr.Read<double>();

                float x = mr.Read<float>();
                float y = mr.Read<float>();
                float z = mr.Read<float>();
                float w = mr.Read<float>();

                Quaternion rot = new Quaternion(x, y, z, w);

                Guid vesselID = new Guid(mr.Read<string>());

                uint turretID = mr.Read<uint>();

                BDArmoryTurretRotUpdate update = new BDArmoryTurretRotUpdate(timeStamp, rot, vesselID, turretID);
                turretYawEntries.Add(update);
            }
        }

        #endregion

        #region Turret Pitch

        void TurretPitchHook(Quaternion rot, Guid vesselID, uint turretID)
        {
            /*
            //Reset tickCount at beginning of Hook
            if (turretTickCount == syncTurretHz && (Time.realtimeSinceStartup - lastTurretsync) >= 1)
            {
                turretTickCount = 0;
            }

            //Only send per fx tick rate
            bool clearToSend = false || Time.realtimeSinceStartup - lastTurretsync > (1f / (float)syncTurretHz) * (float)turretTickCount;

            if (clearToSend)
            {
                //Set lastFXSync and raise tick count right away
                lastTurretsync = Time.realtimeSinceStartup;
                turretTickCount++;
            */
                using (MessageWriter mw = new MessageWriter())
                {
                    mw.Write<double>(Planetarium.GetUniversalTime());

                    mw.Write<float>(rot.x);
                    mw.Write<float>(rot.y);
                    mw.Write<float>(rot.z);
                    mw.Write<float>(rot.w);

                    mw.Write<string>(vesselID.ToString());

                    mw.Write<uint>(turretID);

                    DMPModInterface.fetch.SendDMPModMessage("BDDMP:TurretPitchHook", mw.GetMessageBytes(), true, true);
                }
            //}
            //turretTickCount++;
        }

        void HandleTurretPitchHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                double timeStamp = mr.Read<double>();

                float x = mr.Read<float>();
                float y = mr.Read<float>();
                float z = mr.Read<float>();
                float w = mr.Read<float>();

                Quaternion rot = new Quaternion(x, y, z, w);

                Guid vesselID = new Guid(mr.Read<string>());

                uint turretID = mr.Read<uint>();

                BDArmoryTurretRotUpdate update = new BDArmoryTurretRotUpdate(timeStamp, rot, vesselID, turretID);
                turretPitchEntries.Add(update);
            }
        }

        #endregion

        #region Turret Deploy

        void TurretDeployHook(bool state, Guid vesselID, uint turretID)
        {
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<double>(Planetarium.GetUniversalTime());

                mw.Write<bool>(state);

                mw.Write<string>(vesselID.ToString());

                mw.Write<uint>(turretID);

                DMPModInterface.fetch.SendDMPModMessage("BDDMP:TurretDeployHook", mw.GetMessageBytes(), true, true);
            }
        }

        void HandleTurretDeployHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                double timeStamp = mr.Read<double>();

                bool state = mr.Read<bool>();

                Guid vesselID = new Guid(mr.Read<string>());

                uint turretID = mr.Read<uint>();

                BDArmoryTurretDeployUpdate update = new BDArmoryTurretDeployUpdate(timeStamp, state, vesselID, turretID);
                turretDeployEntries.Add(update);
            }
        }

        #endregion

        #region Laser

        void LaserHook(Vector3 p1, Vector3 p2, Guid vesselID, uint turretID)
        {
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<double>(Planetarium.GetUniversalTime());

                mw.Write<float>(p1.x);
                mw.Write<float>(p1.y);
                mw.Write<float>(p1.z);

                mw.Write<float>(p2.x);
                mw.Write<float>(p2.y);
                mw.Write<float>(p2.z);

                mw.Write<string>(vesselID.ToString());

                mw.Write<uint>(turretID);

                DMPModInterface.fetch.SendDMPModMessage("BDDMP:LaserHook", mw.GetMessageBytes(), true, false);
            }
        }

        void HandleLaserHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                double timeStamp = mr.Read<double>();

                float x = mr.Read<float>();
                float y = mr.Read<float>();
                float z = mr.Read<float>();

                Vector3 p1 = new Vector3(x, y, z);

                x = mr.Read<float>();
                y = mr.Read<float>();
                z = mr.Read<float>();

                Vector3 p2 = new Vector3(x, y, z);

                Guid vesselID = new Guid(mr.Read<string>());

                uint turretID = mr.Read<uint>();

                BDArmoryLaserUpdate update = new BDArmoryLaserUpdate(timeStamp, p1, p2, vesselID, turretID);
                laserEntries.Add(update);
            }
        }

        #endregion

        #region Flare

        void FlareHook(FlareObject flare)
        {
            lock (flareLock)
            {
                flares.Add(flare, Planetarium.GetUniversalTime());
            }
        }

        void HandleFlareHook(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                int count = mr.Read<int>();
                while (count > 0)
                {
                    double timeStamp = mr.Read<double>();

                    float x = mr.Read<float>();
                    float y = mr.Read<float>();
                    float z = mr.Read<float>();

                    Vector3 pos = new Vector3(x, y, z);

                    x = mr.Read<float>();
                    y = mr.Read<float>();
                    z = mr.Read<float>();
                    float w = mr.Read<float>();

                    Quaternion rot = new Quaternion(x, y, z, w);

                    x = mr.Read<float>();
                    y = mr.Read<float>();
                    z = mr.Read<float>();

                    Vector3 vel = new Vector3(x, y, z);

                    Guid sourceVessel = new Guid(mr.Read<string>());

                    BDArmoryFlareUpdate update = new BDArmoryFlareUpdate(timeStamp, pos, rot, vel, sourceVessel);
                    flareEntries.Add(update);

                    count--;
                }
            }
        }

        #endregion

        #endregion

        #region Utility Functions
        private bool ApplyUpdate<T> (T entry) where T : BDArmoryUpdate
        {
            double updateDelta = Planetarium.GetUniversalTime () - entry.entryTime;
            if (updateDelta >= 0 && updateDelta < 3 ) {
                return true;
            }
            return false;
        }

        private bool VesselCanBeDamaged(Guid vesselID)
        {
            if (VesselWorker.fetch.LenientVesselUpdatedInFuture (vesselID)) {
                ScreenMessages.PostScreenMessage("BDArmory-DMP: Cannot damage vessel from the past!", 3f, ScreenMessageStyle.UPPER_LEFT);
            }

            return !VesselWorker.fetch.LenientVesselUpdatedInFuture (vesselID);
        }

        private bool CanControl()
        {
            Guid vesselID = FlightGlobals.ActiveVessel.id;
            if (VesselWorker.fetch.LenientVesselUpdatedInFuture(vesselID) || VesselWorker.fetch.isSpectating)
            {
                ScreenMessages.PostScreenMessage("BDArmory-DMP: Cannot control vessel from the past or while spectating!", 3f, ScreenMessageStyle.UPPER_LEFT);
            }

            return !VesselWorker.fetch.LenientVesselUpdatedInFuture(vesselID) && !VesselWorker.fetch.isSpectating;
        }
        #endregion
	}

    #region BDA Update Classes

    public class BDArmoryUpdate
    {
        public double entryTime;
    }

    public class BDArmoryDamageUpdate : BDArmoryUpdate
    {
        public readonly Guid vesselID;
        public readonly uint flightID;
        public readonly double tempurature;
        public readonly double externalTempurature;

        public BDArmoryDamageUpdate(double entryTime, Guid vesselID, uint flightID, double tempurature, double externalTempurature)
        {
            this.entryTime = entryTime;
            this.vesselID = vesselID;
            this.flightID = flightID;
            this.tempurature = tempurature;
            this.externalTempurature = externalTempurature;
        }
    }

    public class BDArmoryBulletHitUpdate : BDArmoryUpdate
    {
        public readonly Guid vesselOriginID;
        public readonly Vector3 position;
        public readonly Vector3 normalDirection;
        public readonly bool ricochet;

        public BDArmoryBulletHitUpdate(double entryTime, Guid vesselOriginID, Vector3 position, Vector3 normalDirection, bool ricochet)
        {
            this.entryTime = entryTime;
            this.vesselOriginID = vesselOriginID;
            this.position = position;
            this.normalDirection = normalDirection;
            this.ricochet = ricochet;
        }
    }

    public class BDArmoryExplosionUpdate : BDArmoryUpdate
    {
        public readonly Vector3 position;
        public readonly Guid vesselOriginID;
        public readonly float radius;
        public readonly float power;
        public readonly Vector3 direction;
        public readonly string explModelPath;
        public readonly string soundPath;

        public BDArmoryExplosionUpdate(double entryTime, Vector3 position, Guid vesselOriginID, float radius, float power, Vector3 direction, string explModelPath, string soundPath)
        {
            this.entryTime = entryTime;
            this.position = position;
            this.vesselOriginID = vesselOriginID;
            this.radius = radius;
            this.power = power;
            this.direction = direction;
            this.explModelPath = explModelPath;
            this.soundPath = soundPath;
        }
    }

    public class BDArmoryTracerInitUpdate : BDArmoryUpdate
    {
        public readonly Guid vesselID;
        public readonly Guid tracerID;
        public readonly uint partID;
        public readonly string bulletTexPath;

        public BDArmoryTracerInitUpdate(double entryTime, string bulletTexPath, Guid tracerID, Guid vesselID, uint partID)
        {
            this.entryTime = entryTime;
            this.bulletTexPath = bulletTexPath;
            this.tracerID = tracerID;
            this.vesselID = vesselID;
            this.partID = partID;
        }
    }

    public class BDArmoryTracerUpdate : BDArmoryUpdate
    {
        public readonly Vector3 p1, p2;
        public readonly Color color;
        public readonly float w1, w2;
        public readonly Guid tracerID;

        public BDArmoryTracerUpdate(double entryTime, Vector3 p1, Vector3 p2, Color newColor, float width1, float width2, Guid tracerID)
        {
            this.entryTime = entryTime;
            this.p1 = p1;
            this.p2 = p2;
            this.color = newColor;
            this.w1 = width1;
            this.w2 = width2;
            this.tracerID = tracerID;
        }
    }

    public class BDArmoryTracerDestroyUpdate : BDArmoryUpdate
    {
        public readonly Guid tracerID;

        public BDArmoryTracerDestroyUpdate(double entryTime, Guid tracerID)
        {
            this.entryTime = entryTime;
            this.tracerID = tracerID;
        }
    }

    public class BDArmoryTurretRotUpdate : BDArmoryUpdate
    {
        public readonly Quaternion rot;
        public readonly Guid vesselID;
        public readonly uint turretID;

        public BDArmoryTurretRotUpdate(double entryTime, Quaternion rotation, Guid vesselID, uint turretID)
        {
            this.entryTime = entryTime;
            this.rot = rotation;
            this.vesselID = vesselID;
            this.turretID = turretID;
        }
    }

    public class BDArmoryTurretDeployUpdate : BDArmoryUpdate
    {
        public readonly bool state;
        public readonly Guid vesselID;
        public readonly uint turretID;

        public BDArmoryTurretDeployUpdate(double entryTime, bool state, Guid vesselID, uint turretID)
        {
            this.entryTime = entryTime;
            this.state = state;
            this.vesselID = vesselID;
            this.turretID = turretID;
        }
    }

    public class BDArmoryLaserUpdate : BDArmoryUpdate
    {
        public readonly Vector3 p1, p2;
        public readonly bool rayCast;
        public readonly Guid vesselID;
        public readonly uint turretID;

        public BDArmoryLaserUpdate(double entryTime, Vector3 p1, Vector3 p2, Guid vesselID, uint turretID)
        {
            this.entryTime = entryTime;
            this.p1 = p1;
            this.p2 = p2;
            this.vesselID = vesselID;
            this.turretID = turretID;
        }
    }

    public class BDArmoryFlareUpdate : BDArmoryUpdate
    {
        public readonly Vector3 pos, vel;
        public readonly Quaternion rot;
        public readonly Guid vesselID;
        public readonly uint turretID;

        public BDArmoryFlareUpdate(double entryTime, Vector3 pos, Quaternion rot, Vector3 vel, Guid vesselID)
        {
            this.entryTime = entryTime;
            this.rot = rot;
            this.pos = pos;
            this.vesselID = vesselID;
            this.vel = vel;
        }
    }

    #endregion

    public class BDArmouryTracer
    {
        public double lastUpdateTime;
        public Guid id;
        public Vector3 offset;
        public GameObject tracer;
    }
}

