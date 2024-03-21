using HarmonyLib;

namespace syntax_z.ejectPlayer.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {

        [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.Update))]
        [HarmonyPrefix]
        private static bool UpdatePatch(ref StartMatchLever __instance)
        {

            if (__instance.playersManager.travellingToNewLevel || !__instance.playersManager.inShipPhase)
            {
                return true;
            }

            // Disable Lever & Quit Terminal
            if (StartOfRoundPatch.currentlyEjectingPlayer)
            {
                __instance.triggerScript.interactable = false;
                Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                if (terminal.terminalInUse)
                {
                    terminal.QuitTerminal();
                }
            }
            else
            {
                __instance.triggerScript.interactable = true;
            }
            return true;
        }

    }
}
