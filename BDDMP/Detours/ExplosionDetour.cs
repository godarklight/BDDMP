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

        public new static void DoExplosionRay(Ray ray, float power, float heat, float maxDistance, ref List<Part> ignoreParts, ref List<DestructibleBuilding> ignoreBldgs, Vessel sourceVessel = null)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, maxDistance, 688129))
            {
                float sqrDist = (rayHit.point - ray.origin).sqrMagnitude;
                float sqrMaxDist = maxDistance * maxDistance;
                float distanceFactor = Mathf.Clamp01((sqrMaxDist - sqrDist) / sqrMaxDist);
                //parts
                KerbalEVA eva = rayHit.collider.gameObject.GetComponentUpwards<KerbalEVA>();
                Part part = eva ? eva.part : rayHit.collider.GetComponentInParent<Part>();

                if (part)
                {
                    Vessel missileSource = null;
                    if (sourceVessel != null)
                    {
                        MissileBase ml = part.FindModuleImplementing<MissileBase>();
                        if (ml)
                        {
                            missileSource = ml.SourceVessel;
                        }
                    }


                    if (!ignoreParts.Contains(part) && part.physicalSignificance == Part.PhysicalSignificance.FULL &&
                        (!sourceVessel || sourceVessel != missileSource))
                    {
                        ignoreParts.Add(part);
                        Rigidbody rb = part.GetComponent<Rigidbody>();
                        if (rb)
                        {
                            rb.AddForceAtPosition(ray.direction * power * distanceFactor * ExplosionImpulseMultiplier,
                                rayHit.point, ForceMode.Impulse);
                        }
                        if (heat < 0)
                        {
                            heat = power;
                        }
                        float heatDamage = (BDArmorySettings.DMG_MULTIPLIER / 100) * ExplosionHeatMultiplier * heat *
                                           distanceFactor / part.crashTolerance;
                        float excessHeat = Mathf.Max(0, (float)(part.temperature + heatDamage - part.maxTemp));
                        part.AddDamage(heatDamage);
                        if (BDArmorySettings.DRAW_DEBUG_LABELS)
                            Debug.Log("[BDArmory]:====== Explosion ray hit part! Damage: " + heatDamage);
                        if (excessHeat > 0 && part.parent)
                        {
                            part.parent.AddDamage(excessHeat);
                        }
                        return;
                    }
                }

                //buildings
                DestructibleBuilding building = rayHit.collider.GetComponentInParent<DestructibleBuilding>();
                if (building && !ignoreBldgs.Contains(building))
                {
                    ignoreBldgs.Add(building);
                    float damageToBuilding = (BDArmorySettings.DMG_MULTIPLIER / 100) * ExplosionHeatMultiplier * 0.00645f *
                                             power * distanceFactor;
                    if (damageToBuilding > building.impactMomentumThreshold / 10) building.AddDamage(damageToBuilding);
                    if (building.Damage > building.impactMomentumThreshold) building.Demolish();
                    if (BDArmorySettings.DRAW_DEBUG_LABELS)
                        Debug.Log("[BDArmory]:== Explosion hit destructible building! Damage: " +
                                  (damageToBuilding).ToString("0.00") + ", total Damage: " + building.Damage);
                }
            }
        }
    }
}
