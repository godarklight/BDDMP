using System;
using DarkMultiPlayerServer;

namespace BDDMPServer
{
    public class BDDMPServerSync : DMPPlugin
    {
        public BDDMPServerSync()
        {
            DMPModInterface.RegisterModHandler("HitHook", HandleHitHookMessage);
            DMPModInterface.RegisterModHandler("BulletHook", HandleBulletHookMessage);
            DMPModInterface.RegisterModHandler("ExplosionHook", HandleBulletHitHookMessage);
        }

        public void HandleHitHookMessage(ClientObject client, byte[] messageData)
        {
            //Relay hook messages to other clients
            DMPModInterface.SendDMPModMessageToAll(client, "HitHook", messageData, true);
        }

        public void HandleBulletHookMessage(ClientObject client, byte[] messageData)
        {
            //Relay hook messages to other clients
            DMPModInterface.SendDMPModMessageToAll(client, "BulletHook", messageData, true);
        }

        public void HandleBulletHitHookMessage(ClientObject client, byte[] messageData)
        {
            //Relay hook messages to other clients
            DMPModInterface.SendDMPModMessageToAll(client, "ExplosionHook", messageData, true);
        }
    }
}