using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace syntax_z.ejectPlayer.Patches
{

    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        public static bool notsafe = false;
        public static bool currentlyEjectingPlayer = false;

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Update))]
        [HarmonyPostfix]
        private static void UpdatePatch(ref StartOfRound __instance)
        {

            if (Plugin.localEject)
            {
                Plugin.localEject = false;
                currentlyEjectingPlayer = true;
                StartOfRound.Instance.StartCoroutine(EjectPlayer(__instance));
            }

        }


        private static IEnumerator EjectPlayer(StartOfRound __instance)
        {


            yield return new WaitForSeconds(5f);
            __instance.shipAnimatorObject.gameObject.GetComponent<Animator>().SetBool("AlarmRinging", value: true);
            __instance.shipRoomLights.SetShipLightsOnLocalClientOnly(setLightsOn: false);
            __instance.shipDoorAudioSource.PlayOneShot(__instance.alarmSFX);



            yield return new WaitForSeconds(9.37f);
            __instance.shipDoorsAnimator.SetBool("OpenInOrbit", value: true);
            __instance.shipDoorAudioSource.PlayOneShot(__instance.airPressureSFX);
            __instance.starSphereObject.SetActive(value: true);
            __instance.starSphereObject.transform.position = GameNetworkManager.Instance.localPlayerController.transform.position;



            yield return new WaitForSeconds(0.25f);

            if (notsafe == true)
            {
                __instance.hangarDoorsClosed = false;
                __instance.suckingPlayersOutOfShip = true;
                __instance.suckingFurnitureOutOfShip = true;
                SuckLocalPlayerOutOfShipDoor(__instance);
                GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = true;
                GameNetworkManager.Instance.localPlayerController.DropAllHeldItems();
                HUDManager.Instance.UIAudio.PlayOneShot(__instance.suckedIntoSpaceSFX);
            }



            yield return new WaitForSeconds(6f);
            HUDManager.Instance.ShowPlayersFiredScreen(show: true);


            yield return new WaitForSeconds(3.5f);
            __instance.shipDoorAudioSource.Stop();
            __instance.speakerAudioSource.Stop();
            __instance.starSphereObject.SetActive(value: false);
            __instance.shipDoorAudioSource.Stop();
            __instance.suckingFurnitureOutOfShip = false;
            __instance.suckingPlayersOutOfShip = false;
            __instance.shipRoomLights.SetShipLightsOnLocalClientOnly(setLightsOn: true);
            __instance.shipDoorsAnimator.SetBool("OpenInOrbit", value: false);
            __instance.shipAnimatorObject.gameObject.GetComponent<Animator>().SetBool("AlarmRinging", value: false);
            __instance.suckingPower = 0f;
            HUDManager.Instance.ShowPlayersFiredScreen(show: false);


            if (notsafe == true)
            {
                GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = false;
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(__instance.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
                /*
                if (Plugin.tempEject.Value == true)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(UnityEngine.Vector3.zero, spawnBody: false, causeOfDeath: CauseOfDeath.Gravity);
                }
                */
            }

            notsafe = false;
            Plugin.localEject = false;
            currentlyEjectingPlayer = false;

        }


        private static void SuckLocalPlayerOutOfShipDoor(StartOfRound __instance)
        {

            __instance.suckingPower += Time.deltaTime * 2f;
            GameNetworkManager.Instance.localPlayerController.fallValue = 0f;
            GameNetworkManager.Instance.localPlayerController.fallValueUncapped = 0f;
            if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, __instance.middleOfShipNode.position) < 25f)
            {
                if (Physics.Linecast(GameNetworkManager.Instance.localPlayerController.transform.position, __instance.shipDoorNode.position, __instance.collidersAndRoomMask))
                {
                    GameNetworkManager.Instance.localPlayerController.externalForces = Vector3.Normalize(__instance.middleOfShipNode.position - GameNetworkManager.Instance.localPlayerController.transform.position) * 350f;
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController.externalForces = Vector3.Normalize(__instance.middleOfSpaceNode.position - GameNetworkManager.Instance.localPlayerController.transform.position) * (350f / Vector3.Distance(__instance.moveAwayFromShipNode.position, GameNetworkManager.Instance.localPlayerController.transform.position)) * (__instance.suckingPower / 2.25f);
                }
                return;
            }


            GameNetworkManager.Instance.localPlayerController.externalForces = Vector3.Scale(Vector3.one, new Vector3(-1f, 0f, UnityEngine.Random.Range(-0.7f, 0.7f))) * 70f;
        }


        /*
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ReviveDeadPlayers))]
        [HarmonyPrefix]
        private static bool ReviveDeadPlayersPatch()
        {
            Plugin.tempEject.Value = false;
            return true;
        }
        */

    }
}
