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
    class ExplosionDetour : ExplosionFX
    {
        public new static void CreateExplosion(Vector3 position, float radius, float power, float heat, Vessel sourceVessel, Vector3 direction, string explModelPath, string soundPath)
        {
            HitManager.FireExplosionHooks(new ExplosionObject(position, radius, power, heat, sourceVessel, direction, explModelPath, soundPath));
            _CreateExplosion(position, radius, power, heat, sourceVessel, direction, explModelPath, soundPath);
        }

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
            ExplosionFX eFx = newExplosion.AddComponent<ExplosionFX>();
            eFx.exSound = soundClip;
            eFx.audioSource = newExplosion.AddComponent<AudioSource>();
            eFx.audioSource.minDistance = 200;
            eFx.audioSource.maxDistance = 5500;
            eFx.audioSource.spatialBlend = 1;
            eFx.range = radius;

            if (power <= 5)
            {
                eFx.audioSource.minDistance = 4f;
                eFx.audioSource.maxDistance = 3000;
                eFx.audioSource.priority = 9999;
            }
            IEnumerator<KSPParticleEmitter> pe = newExplosion.GetComponentsInChildren<KSPParticleEmitter>().Cast<KSPParticleEmitter>()
                .GetEnumerator();
            while (pe.MoveNext())
            {
                if (pe.Current == null) continue;
                pe.Current.emit = true;

            }
            pe.Dispose();

            DoExplosionDamage(position, power, heat, radius, sourceVessel);
        }
    }

    class BulletHitDetour : BulletHitFX
    {
        public new static void CreateBulletHit(Vector3 position, Vector3 normalDirection, bool ricochet)
        {
            HitManager.FireBulletHooks(new BulletObject(position, normalDirection, ricochet));
            _CreateBulletHit(position, normalDirection, ricochet);
        }

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
