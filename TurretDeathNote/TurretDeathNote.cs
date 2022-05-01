using BepInEx;
using IL;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using HarmonyLib;


// ReSharper disable UnusedMember.Global
// ReSharper disable once InconsistentNaming
// ReSharper disable StringLiteralTypo
namespace TurretDeathNote
{
    [BepInDependency(R2API.R2API.PluginGUID)]
//    [R2API.Utils.R2APISubmoduleDependency("NetworkingAPI")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class TurretDeathNote : BaseUnityPlugin
    {
        public const string PluginAuthor = "Watch_Me_Be_Meh";
        public const string PluginName = "TurretDeathNote";
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginVersion = "0.1.0";

        // Taken from github.com/NotTsunami/ShowDeathCause mod
        //private static DamageReport _damageReport;
        //private static string _damageTaken;
        //private static string _attacker;

        public static string GetAttacker(DamageReport damageReport)
        {
            // Standard code path
            if (damageReport.attackerMaster)
            {
                return damageReport.attackerMaster.playerCharacterMasterController ? damageReport.attackerMaster.playerCharacterMasterController.networkUser
                    .userName : Util.GetBestBodyName(damageReport.attackerBody.gameObject);
            }

            // For overrides like Suicide() of type VoidDeath, return damageReport.attacker, otherwise ???
            return damageReport.attacker ? Util.GetBestBodyName(damageReport.attacker) : "???";
        }

        public static bool IsVoidFogAttacker(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;

            // Checking done by referencing FogDamageController's EvaluateTeam()
            return damageInfo.damageColorIndex == DamageColorIndex.Void
                   && damageInfo.damageType.HasFlag(DamageType.BypassArmor)
                   && damageInfo.damageType.HasFlag(DamageType.BypassBlock)
                   && damageInfo.attacker == null
                   && damageInfo.inflictor == null;
        }


        public void Awake()
        {
            var harmony = new Harmony(Info.Metadata.GUID);
            new PatchClassProcessor(harmony, typeof(HarmonyPatch)).Patch();

            GlobalEventManager.onCharacterDeathGlobal += (damageReport) =>
            {

                if (damageReport == null) return;

                if (Util.GetBestMasterName(damageReport.victimMaster) == "Engineer Turret")
                {
                    // An Engineer's turret has died!

                    if (IsVoidFogAttacker(damageReport))
                    {
                        Chat.AddMessage(message: "Engineer's turret was abandoned in the Void Fog");
                    }
                    else
                    {
                        Chat.AddMessage(message: "Engineer's turret was killed by: " + GetAttacker(damageReport));
                    }
                }
            };
        }
    }
}
