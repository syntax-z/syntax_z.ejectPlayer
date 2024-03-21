using BepInEx;
using syntax_z.ejectPlayer.Patches;
using GameNetcodeStuff;
using HarmonyLib;
using LethalAPI.LibTerminal;
using LethalAPI.LibTerminal.Attributes;
using LethalAPI.LibTerminal.Models;
using LethalNetworkAPI;



namespace syntax_z.ejectPlayer
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("LethalNetworkAPI")]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "syntax_z.ejectPlayer";
        private const string modName = "syntax_z.ejectPlayer";
        private const string modVersion = "1.0.0";
        private readonly Harmony _harmony = new(modGUID);
        public static Plugin? Instance;
        private TerminalModRegistry? TCommands;

        public static bool localEject = false;


        public static readonly LethalServerMessage<string> customServerMessage = new LethalServerMessage<string>(identifier: "ejectingPlayers", ReceiveByServer);
        public static readonly LethalClientMessage<string> customClientMessage = new LethalClientMessage<string>(identifier: "ejectingPlayers", ReceiveFromServer, ReceiveFromClient);
        public static LethalNetworkVariable<string> globalMessage = new LethalNetworkVariable<string>(identifier: "globalMessage");
        private void Awake()
        {
            PatchAllStuff();

            TCommands = TerminalRegistry.CreateTerminalRegistry();
            TCommands.RegisterFrom(this);
        }




        private void PatchAllStuff()
        {
            _harmony.PatchAll(typeof(StartOfRoundPatch));
            _harmony.PatchAll(typeof(StartMatchLeverPatch));
            _harmony.PatchAll(typeof(HUDManagerPatch));
        }



        private static void ReceiveFromServer(string data)
        {
            string ejectedID = data.Split('/')[1];
            string playerID = $"{GameNetworkManager.Instance.localPlayerController.playerClientId}";
            string msg = data.Split('/')[2];

            globalMessage.Value = msg;
            if (ejectedID == playerID)
            {
                StartOfRoundPatch.notsafe = true;
            }

            localEject = true;
        }
        private static void ReceiveFromClient(string data, ulong id)
        {

        }
        private static void ReceiveByServer(string data, ulong id)
        {
            if (data.Contains("eject/"))
            {
                customServerMessage.SendAllClients(data);
            }
        }


        [TerminalCommand("list")]
        [CommandInfo("Display list of users to eject")]
        public string listCommand()
        {
            string name = "";
            string clientID;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                // Prevents dummy PlayerScript names from showing up on screen
                if (StartOfRound.Instance.allPlayerScripts[i].playerSteamId == 0) continue;

                clientID = $"{StartOfRound.Instance.allPlayerScripts[i].playerClientId}";
                name += $"{StartOfRound.Instance.allPlayerScripts[i].playerUsername} || ID: {clientID}\n";
            }
            return name;
        }

        [TerminalCommand("eject")]
        [CommandInfo("eg. eject (playerID) (message)")]
        public string ejectCommand(ulong playerID, [RemainingText] string msg)
        {
            if (!StartOfRound.Instance.ClientPlayerList.Keys.Contains(playerID))
                return "Invalid. Type \"list\" to see the available IDs";

            if (GameNetworkManager.Instance.localPlayerController.playersManager.travellingToNewLevel || !GameNetworkManager.Instance.localPlayerController.playersManager.inShipPhase)
                return "Can't eject player at this moment";

            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == playerID)
                {
                    customClientMessage.SendServer($"eject/{playerID}/{msg}");
                    return "ejecting player from ship";
                }
            }
            return "Something went wrong. Make sure you're typing the correct number.";
        }


    }
}