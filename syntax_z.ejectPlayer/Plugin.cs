using BepInEx;
using syntax_z.ejectPlayer.Patches;
using GameNetcodeStuff;
using HarmonyLib;
using LethalAPI.LibTerminal;
using LethalAPI.LibTerminal.Attributes;
using LethalAPI.LibTerminal.Models;
using LethalNetworkAPI;
using System.Numerics;
using System.Collections.Generic;



namespace syntax_z.ejectPlayer
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("LethalNetworkAPI")]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "syntax_z.ejectPlayer";
        private const string modName = "syntax_z.ejectPlayer";
        private const string modVersion = "1.2.0";
        private readonly Harmony _harmony = new(modGUID);
        public static Plugin? Instance;
        private TerminalModRegistry? TCommands;

        public static bool localEject = false;


        public static readonly LethalServerMessage<string> customServerMessage = new LethalServerMessage<string>(identifier: "ejectingPlayers", ReceiveByServer);
        public static readonly LethalClientMessage<string> customClientMessage = new LethalClientMessage<string>(identifier: "ejectingPlayers", ReceiveFromServer, ReceiveFromClient);
        public static LethalNetworkVariable<string> globalMessage = new LethalNetworkVariable<string>(identifier: "globalMessage");
        public static LethalNetworkVariable<bool> tempEject = new LethalNetworkVariable<bool>(identifier: "tempEject");
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
            PlayerControllerB player;

            foreach (KeyValuePair<ulong, int> clientPlayer in StartOfRound.Instance.ClientPlayerList)
            {
                player = StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientPlayer.Key]];
                name += $"{player.playerUsername} || ID: {player.playerClientId}\n";
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

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.playerClientId == playerID)
                {
                    customClientMessage.SendServer($"eject/{playerID}/{msg}");
                    return "ejecting player from ship";
                }
            }
            return "Something went wrong. Make sure you're typing the correct number.";
        }



        [TerminalCommand("rnd_eject")]
        [CommandInfo("eject a random player from the ship")]
        public string rndEjectCommand()
        {
            if (GameNetworkManager.Instance.localPlayerController.playersManager.travellingToNewLevel || !GameNetworkManager.Instance.localPlayerController.playersManager.inShipPhase)
                return "Can't eject player at this moment";

            Random random = new Random();
            ulong randomNum = (ulong)random.Next(0, StartOfRound.Instance.ClientPlayerList.Keys.ToArray().Length);
            ulong randomPlayerID = (ulong)StartOfRound.Instance.ClientPlayerList[randomNum];

            customClientMessage.SendServer($"eject/{randomPlayerID}/Ejected");
            return "ejecting a random player from the ship";
        }


        /*
        [TerminalCommand("rndtemp_eject")]
        [CommandInfo("Ejects and kills player for the next round")]
        public string tempEjectCommand()
        {
            if (GameNetworkManager.Instance.localPlayerController.playersManager.travellingToNewLevel || !GameNetworkManager.Instance.localPlayerController.playersManager.inShipPhase)
                return "Can't eject player at this moment";

            // TODO: Ignore dummy player scripts
            Random random = new Random();
            int randomNum = random.Next(0, StartOfRound.Instance.allPlayerScripts.ToArray().Length);
            ulong randomPlayerID = StartOfRound.Instance.allPlayerScripts[randomNum].playerClientId;
            tempEject.Value = true;

            customClientMessage.SendServer($"eject/{randomPlayerID}/Ejected");
            return "ejecting random player from ship";
        }
        */
    }
}