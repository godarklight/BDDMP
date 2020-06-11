using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BDArmory.FX;
using UnityEngine;
using BDArmory.Misc;
using BDArmory;
using BDArmory.UI;
using BDArmory.Parts;
using BDArmory.Core.Extension;

namespace BDDMP.Detours
{
    class ExplosionDetour : ExplosionFx
    {
        public static void CreateExplosion(Vector3 position, float radius, float power, float heat, Vessel sourceVessel, Vector3 direction, string explModelPath, string soundPath)
        {
            HitManager.FireExplosionHooks(new ExplosionObject(position, radius, power, heat, sourceVessel, direction, explModelPath, soundPath));
            _CreateExplosion(position, radius, power, heat, sourceVessel, direction, explModelPath, soundPath);
        }

        /**
         * CC BY-SA 2.0 as taken from BDA
         */
        public static void _CreateExplosion(Vector3 position, float radius, float power, float heat, Vessel sourceVessel,
           Vector3 direction, string explModelPath, string soundPath)
        {
            GameObject go;
            AudioClip soundClip;

            go = GameDatabase.Instance.GetModel(explModelPath);
            soundClip = GameDatabase.Instance.GetAudioClip(soundPath);


            Quaternion rotation = Quaternion.LookRotation(VectorUtils.GetUpDirection(position));
            GameObject newExplosion = (GameObject)Instantiate(go, position, rotation);
            newExplosion.SetActive(true);
            ExplosionFx eFx = newExplosion.AddComponent<ExplosionFx>();
            eFx.ExSound = soundClip;
            eFx.AudioSource = newExplosion.AddComponent<AudioSource>();
            eFx.AudioSource.minDistance = 200;
            eFx.AudioSource.maxDistance = 5500;
            eFx.AudioSource.spatialBlend = 1;
            eFx.Range = radius;

            if (power <= 5)
            {
                eFx.AudioSource.minDistance = 4f;
                eFx.AudioSource.maxDistance = 3000;
                eFx.AudioSource.priority = 9999;
            }
            IEnumerator<KSPParticleEmitter> pe = newExplosion.GetComponentsInChildren<KSPParticleEmitter>().Cast<KSPParticleEmitter>()
                .GetEnumerator();
            while (pe.MoveNext())
            {
                if (pe.Current == null) continue;
                pe.Current.emit = true;

            }
            pe.Dispose();

            //TODO: Where the hell did this go?
            //DoExplosionDamage(position, power, heat, radius, sourceVessel);
        }
    }

    class BulletHitDetour : BulletHitFX
    {
        public static void CreateBulletHit(Vector3 position, Vector3 normalDirection, bool ricochet)
        {
            HitManager.FireBulletHooks(new BulletObject(position, normalDirection, ricochet));
            _CreateBulletHit(position, normalDirection, ricochet);
        }

        /**
         * CC BY-SA 2.0 as taken from BDA
         */
        public static void _CreateBulletHit(Vector3 position, Vector3 normalDirection, bool ricochet)
        {
            GameObject go = GameDatabase.Instance.GetModel("BDArmory/Models/bulletHit/bulletHit");
            GameObject newExplosion =
                (GameObject)Instantiate(go, position, Quaternion.LookRotation(normalDirection));
            newExplosion.SetActive(true);
            newExplosion.AddComponent<BulletHitFX>();
            newExplosion.GetComponent<BulletHitFX>().ricochet = ricochet;
            IEnumerator<KSPParticleEmitter> pe = newExplosion.GetComponentsInChildren<KSPParticleEmitter>().Cast<KSPParticleEmitter>().GetEnumerator();
            while (pe.MoveNext())
            {
                if (pe.Current == null) continue;
                pe.Current.emit = true;

                if (pe.Current.gameObject.name == "sparks")
                {
                    pe.Current.force = (4.49f * FlightGlobals.getGeeForceAtPosition(position));
                }
                else if (pe.Current.gameObject.name == "smoke")
                {
                    pe.Current.force = (1.49f * FlightGlobals.getGeeForceAtPosition(position));
                }
            }
            pe.Dispose();
        }
    }
}
