using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
//using BDArmory.Armor;
using BDArmory.Core.Extension;
using BDArmory.FX;
using BDArmory.Misc;
using BDArmory.UI;
using BDArmory.Modules;
using KSP.UI.Screens;
using UniLinq;
using UnityEngine;
using BDArmory;
using DarkMultiPlayer;

namespace BDDMP.Detours
{

    class WeaponDetour : ModuleWeapon
    {
        /**
         * CC BY-SA 2.0 as taken from BDA
         */
        public new void EnableWeapon()
        {
            if (weaponState == WeaponStates.Enabled || weaponState == WeaponStates.PoweringUp)
            {
                return;
            }

            //StopCoroutine("StartupRoutine");
            StopCoroutine("ShutdownRoutine");

            StartCoroutine("StartupRoutine");

            if (BDDMPSynchronizer.sendTurretState && FlightGlobals.ActiveVessel.id == part.vessel.id
                && !Client.dmpClient.dmpGame.vesselWorker.isSpectating && Time.frameCount % 20 == 0)
            {
                HitManager.FireTurretDeployHooks(true, part.vessel.id, part.craftID);
            }
        }

        /**
         * CC BY-SA 2.0 as taken from BDA
         */
        public new void DisableWeapon()
        {
            if (weaponState == WeaponStates.Disabled || weaponState == WeaponStates.PoweringDown)
            {
                return;
            }

            StopCoroutine("StartupRoutine");
            //StopCoroutine("ShutdownRoutine");

            StartCoroutine("ShutdownRoutine");

            if (BDDMPSynchronizer.sendTurretState && FlightGlobals.ActiveVessel.id == part.vessel.id 
                && !Client.dmpClient.dmpGame.vesselWorker.isSpectating && Time.frameCount % 20 == 0)
            {
                HitManager.FireTurretDeployHooks(false, part.vessel.id, part.craftID);
            }
        }
    }
}
