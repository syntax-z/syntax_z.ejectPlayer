using HarmonyLib;
using TMPro;
using UnityEngine;


namespace syntax_z.ejectPlayer.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        private static string defaultHeader = "YOU ARE FIRED.";
        private static string defaultSub = "You did not meet the profit quota before the deadline.";

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.ShowPlayersFiredScreen))]
        [HarmonyPrefix]
        private static void ShowEjectScreen(ref Animator ___playersFiredAnimator)
        {


            if (StartOfRoundPatch.currentlyEjectingPlayer)
            {
                ___playersFiredAnimator.gameObject.transform.Find("MaskImage").Find("HeaderText").GetComponent<TextMeshProUGUI>().text = Plugin.globalMessage.Value;
                ___playersFiredAnimator.gameObject.transform.Find("MaskImage").Find("HeaderText (1)").GetComponent<TextMeshProUGUI>().text = "";
            }
            else
            {
                ___playersFiredAnimator.gameObject.transform.Find("MaskImage").Find("HeaderText").GetComponent<TextMeshProUGUI>().text = defaultHeader;
                ___playersFiredAnimator.gameObject.transform.Find("MaskImage").Find("HeaderText (1)").GetComponent<TextMeshProUGUI>().text = defaultSub;
            }
        }

    }
}
