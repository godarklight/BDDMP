using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BDArmory.FX;
using UnityEngine;

namespace BDDMP.Detours
{
    class ExplosionDetour : ExplosionFX
    {
        public new static void CreateExplosion(Vector3 position, float radius, float power, float heat, Vessel sourceVessel, Vector3 direction, string explModelPath, string soundPath)
        {
            Debug.LogError("BANG");
        }
    }
}
