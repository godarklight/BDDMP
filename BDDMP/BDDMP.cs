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
		private static BDDMPSynchronizer singleton;

		//Tracking
		public Dictionary<string, List<string>> partsDestroyed;

		//State
		private bool initialized = false;

		public BDDMPSynchronizer ()
		{
			singleton = this;
		}

		public void Awake()
		{
			GameObject.DontDestroyOnLoad (this);
			partsDestroyed = new Dictionary<string, List<string>> ();
			initialized = true;

            //Message Registration
            DMPModInterface.fetch.RegisterRawModHandler("HitHook", HandleHitHook);
            DMPModInterface.fetch.RegisterRawModHandler("BulletHook", HandleBulletHook);
            DMPModInterface.fetch.RegisterRawModHandler ("ExplosionHook", HandleExplosionHook);

            //Hook Registration
            HitManager.RegisterHitHook(HitHook);
            HitManager.RegisterBulletHook(BulletHook);
            HitManager.RegisterExplosionHook (ExplosionHook);

		}

        public void HitHook(Part hitPart)
        {
            //DarkLog.Debug ("BDDMP Asked to handle HitHook!");
            using (MessageWriter mw = new MessageWriter ()) {
                mw.Write<string> (hitPart.vessel.id.ToString ());
                mw.Write<uint> (hitPart.flightID);
                mw.Write<double> (hitPart.temperature);
                mw.Write<double> (hitPart.externalTemperature);

                DMPModInterface.fetch.SendDMPModMessage("HitHook", mw.GetMessageBytes(), false, true);
            }
        }

        public void BulletHook(BulletObject bullet)
        {
            //DarkLog.Debug ("BDDMP Asked to handle BulletHook!");
            using (MessageWriter mw = new MessageWriter ()) {
                mw.Write<float> (bullet.position.x);
                mw.Write<float> (bullet.position.y);
                mw.Write<float> (bullet.position.z);
                mw.Write<float> (bullet.normalDirection.x);
                mw.Write<float> (bullet.normalDirection.y);
                mw.Write<float> (bullet.normalDirection.z);
                mw.Write<bool> (bullet.ricochet);

                DMPModInterface.fetch.SendDMPModMessage("BulletHook", mw.GetMessageBytes(), false, true);
            }


        }

        public void ExplosionHook(ExplosionObject explosion)
        {
            //DarkLog.Debug ("BDDMP Asked to handle ExplosionHook!");
            using (MessageWriter mw = new MessageWriter ()) {
                mw.Write<float> (explosion.position.x);
                mw.Write<float> (explosion.position.y);
                mw.Write<float> (explosion.position.z);
                mw.Write<float> (explosion.raduis);
                mw.Write<float> (explosion.power);
                mw.Write<string> (explosion.sourceVessel.id.ToString());
                mw.Write<float> (explosion.direction.x);
                mw.Write<float> (explosion.direction.y);
                mw.Write<float> (explosion.direction.z);
                mw.Write<string> (explosion.explModelPath);
                mw.Write<string> (explosion.soundPath);

                DMPModInterface.fetch.SendDMPModMessage("ExplosionHook", mw.GetMessageBytes(), false, true);
            }
        }

        private void HandleHitHook(byte[] messageData)
        {
            //DarkLog.Debug ("BDDMP Got HitHook from DMPServer!");
            using (MessageReader mr = new MessageReader (messageData)) {
                Guid vesselID = new Guid (mr.Read<string> ());
                uint partID = mr.Read<uint> ();
                double partTemp = mr.Read<double> ();
                double partTempExt = mr.Read<double> ();


                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel.id == vesselID) {
                        //DarkLog.Debug ("BDDMP HitHook matched vessel!");
                        foreach (Part part in vessel.Parts) {
                            if (part.flightID == partID) {
                                //DarkLog.Debug ("BDDMP HitHook matched part!");
                                part.temperature = partTemp;
                                part.externalTemperature = partTempExt;
                            }
                        }
                    }
                }
            }
        }

        private void HandleBulletHook(byte[] messageData)
        {
            //DarkLog.Debug ("BDDMP Got BulletHook from DMPServer!");
            using (MessageReader mr = new MessageReader(messageData))
            {
                float posX = mr.Read<float>();
                float posY = mr.Read<float>();
                float posZ = mr.Read<float>();
                Vector3 pos = new Vector3 (posX, posY, posZ);

                float normX = mr.Read<float>();
                float normY = mr.Read<float>();
                float normZ = mr.Read<float>();
                Vector3 norm = new Vector3 (normX, normY, normZ);

                bool rico = mr.Read<bool>();

                BulletHitFX.CreateBulletHit (pos, norm, rico, false);
            }
        }

        public void HandleExplosionHook(byte[] messageData)
        {
            //DarkLog.Debug ("BDDMP Got ExplosionHook from DMPServer!");
            using (MessageReader mr = new MessageReader (messageData)) {
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

                foreach (Vessel vessel in FlightGlobals.Vessels)
                {
                    if (vessel.id == vesselGUID)
                    {
                        //DarkLog.Debug ("BDDMP ExplosionHook matched vessel!");
                        ExplosionFX.CreateExplosion (pos, radi, power, vessel, dir, explPath, soundPath, false);
                    }
                }
            }
        }
	}
}

