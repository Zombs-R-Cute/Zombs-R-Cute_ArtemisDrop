using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Random = System.Random;

namespace Zombs_R_Cute_ArtemisDrop
{
    public class CommandDrop : IRocketCommand
    {
        private const ushort DropModuleId = 51328;
        private const ushort DropCrateId = 51694;
        private const ushort DropSpawnTableId = 51151;
        private Vector3 DropPosition = new Vector3(361.96f, 29.5f, 199.19f); //Fletcher island

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID((CSteamID)ulong.Parse(caller.Id));
            if (Vector3.Distance(player.Position, DropPosition) > 15f)
            {
                UnturnedChat.Say("You need to be next to the drop pad on Fletcher island to use this command.", Color.yellow);
                return;
            }
            
            var inventorySearchResult = player.Inventory.search(DropModuleId, true, true); // 51328 Airdrop call module
            if (inventorySearchResult.Count > 0)
            {
                var result = inventorySearchResult[0];
                player.Inventory.removeItem(result.page,
                    player.Inventory.getIndex(result.page, result.jar.x, result.jar.y));
                var asset = Assets.find(EAssetType.ITEM, DropCrateId);
                SpawnCarepackage();
                return;
            }
            
            UnturnedChat.Say(caller, "You do not have an Airdrop Call Module in your inventory.", Color.red);
        }


        private void SpawnCarepackage()
        {
            Random random = new Random();
            var randomOffsetx = new Vector3(random.Next(-2, 2), 0, random.Next(-2, 2));
            DropPosition += randomOffsetx;
            
            Transform barricade = BarricadeManager.dropBarricade(
                new Barricade(Assets.find(EAssetType.ITEM, DropCrateId) as ItemBarricadeAsset), (Transform)null,
                DropPosition, 0.0f, 0.0f, 0.0f, 0UL, 0UL);
            if ((UnityEngine.Object)barricade != (UnityEngine.Object)null)
            {
                InteractableStorage component = barricade.GetComponent<InteractableStorage>();
                component.despawnWhenDestroyed = true;
                if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.items != null)
                {
                    int num = 0;
                    while (num < 8)
                    {
                        ushort newID = SpawnTableTool.ResolveLegacyId(DropSpawnTableId, EAssetType.ITEM,
                            new Func<string>(OnGetSpawnTableErrorContext));
                        if (newID != (ushort)0)
                        {
                            if (!component.items.tryAddItem(new Item(newID, EItemOrigin.ADMIN), false))
                                ++num;
                        }
                        else
                            break;
                    }

                    component.items.onStateUpdated();
                }

                barricade.gameObject.AddComponent<CarepackageDestroy>();
            }
        }
        private string OnGetSpawnTableErrorContext() => "airdrop care package";

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "drop";
        public string Help => "Places airdrop module on Fletcher island in exchange for drop module.";
        public string Syntax => "/drop";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "ArtemisDrop" };
    }
}