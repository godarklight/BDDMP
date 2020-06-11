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
        public static void AddDamage(this Part p, float damage)
        {
            if (HitManager.ShouldAllowDamageHooks(p.vessel.id))
            {
                Dependencies.Get<DamageService>().AddDamageToPart_svc(p, damage);
                HitManager.FireHitHooks(p);
            }
        }

        public static void SetDamage(this Part p, float damage)
        {
            if (HitManager.ShouldAllowDamageHooks(p.vessel.id))
            {
                Dependencies.Get<DamageService>().SetDamageToPart_svc(p, damage);
                HitManager.FireHitHooks(p);
            }
        }
    }
}
