using BDArmory;
using BDArmory.Core;
using BDArmory.Core.Extension;
using BDArmory.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDDMP.Detours
{
    static class DamageDetour
    {
        public static void AddDamage(this Part p, double damage)
        {
            Dependencies.Get<DamageService>().AddDamageToPart(p, damage);
            HitManager.FireHitHooks(p);
        }

        public static void SetDamage(this Part p, double damage)
        {
            Dependencies.Get<DamageService>().SetDamageToPart(p, damage);
            HitManager.FireHitHooks(p);
        }
    }
}
