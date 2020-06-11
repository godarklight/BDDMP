using UnityEngine;
using BDArmory.Modules;
using DarkMultiPlayer;
using BDArmory.Core;

namespace BDDMP.Detours
{
    static class TurretExt
    {
        public static AudioSource audioSource(this ModuleTurret mt)
        {
            return (AudioSource)typeof(ModuleTurret).GetField("audioSource", BDDMPSynchronizer.flags).GetValue(mt);
        }

        public static float audioRotationRate(this ModuleTurret mt)
        {
            return (float)typeof(ModuleTurret).GetField("audioRotationRate", BDDMPSynchronizer.flags).GetValue(mt);
        }

        public static bool hasAudio(this ModuleTurret mt)
        {
            return (bool)typeof(ModuleTurret).GetField("hasAudio", BDDMPSynchronizer.flags).GetValue(mt);
        }
    }

    class TurretDetour : ModuleTurret
    {
        /**
         * CC BY-SA 2.0 as taken from BDA
         */
        void Update()
        {
            
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (this.hasAudio())
                {
                    if (this.audioRotationRate() > 0.05f)
                    {
                        if (!this.audioSource().isPlaying) this.audioSource().Play();
                    }
                    else
                    {
                        if (this.audioSource().isPlaying)
                        {
                            this.audioSource().Stop();
                        }
                    }
                }
            }
            //Done to reduce network load
            if (BDDMPSynchronizer.sendTurretRot && FlightGlobals.ActiveVessel.id == this.part.vessel.id 
                && !Client.dmpClient.dmpGame.vesselWorker.isSpectating && Time.frameCount % BDDMPSynchronizer.sendTurretRotRate == 0)
            {
                HitManager.FireTurretPitchHook(pitchTransform.localRotation, this.part.vessel.id, this.part.craftID);
                HitManager.FireTurretYawHook(yawTransform.localRotation, this.part.vessel.id, this.part.craftID);
            }

            
        }
    }
}
