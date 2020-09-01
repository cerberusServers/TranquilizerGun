using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TranquilizerGun {
    public class EventsHandler {

        private Plugin plugin;
        public List<string> tranquilized, armored;
        public Dictionary<string, float> scpShots;
        bool allArmorEnabled = false, testFix;

        public EventsHandler(Plugin plugin) {
            this.plugin = plugin;
            tranquilized = new List<string>();
            armored = new List<string>();
            scpShots = new Dictionary<string, float>();
        }

        public void RoundEnd(RoundEndedEventArgs ev) {
            tranquilized.Clear();
            armored.Clear();
            scpShots.Clear();

            testFix = true;
        }

        public void RoundStart() {
            Timing.RunCoroutine(DelayedReplace());
            testFix = false;
        } 

        public void ShootEvent(ShootingEventArgs ev) {
            // I know this is a lazy fix, don't ree at me about it, I'm trying to fix my sleeping schedule and other projects so haven't got much time to find the cause of the Exception this week, prob will actually fix it soon 
            if(testFix)
                return;
            try {
                if((ev.Shooter.CurrentItem.id == ItemType.GunCOM15 && plugin.Config.Com15IsTranquilizer)
                    || (ev.Shooter.CurrentItem.id == ItemType.GunUSP && plugin.Config.UspIsTranquilizer)
					|| (ev.Shooter.CurrentItem.id == ItemType.GunMP7 && plugin.Config.Mp7IsTranquilizer)
					|| (ev.Shooter.CurrentItem.id == ItemType.GunProject90 && plugin.Config.Project90IsTranquilizer)
					|| (ev.Shooter.CurrentItem.id == ItemType.GunE11SR && plugin.Config.E11IsTranquilizer)
					|| (ev.Shooter.CurrentItem.id == ItemType.GunLogicer && plugin.Config.LogicerIsTranquilizer))
				{

                    if(plugin.Config.silencerRequired && !ev.Shooter.HasSilencer())
                        return;

                    if(ev.Shooter.CurrentItem.durability < plugin.Config.ammoUsedPerShot - 1) {
                        if(plugin.Config.notEnoughAmmoBroadcastDuration > 0) {
                            if(plugin.Config.clearBroadcasts)
                                ev.Shooter.ClearBroadcasts();
                            if(plugin.Config.UseHintsSystem)
                                ev.Shooter.ShowHint(plugin.Config.notEnoughAmmoBroadcastDuration, plugin.Config.notEnoughAmmoBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
                            else ev.Shooter.Broadcast(plugin.Config.notEnoughAmmoBroadcastDuration, plugin.Config.notEnoughAmmoBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
                        }
                        ev.IsAllowed = false;
                        return;
                    }
                    ev.Shooter.RemoveWeaponAmmo(plugin.Config.ammoUsedPerShot - 1);
                }
            } catch(Exception e) {
                e.Print("ShootEvent");
            }
        }

        public void OnPickupEvent(PickingUpItemEventArgs ev) {
            if(IsTranquilizer(ev.Pickup.ItemId) && plugin.Config.pickedUpBroadcastDuration > 0) {
                if(!ev.Pickup.ItemId.IsPistol() || (plugin.Config.silencerRequired && ev.Pickup.weaponMods.Barrel != 1))
                    return;
                if(plugin.Config.clearBroadcasts)
                    ev.Player.ClearBroadcasts();
                if(plugin.Config.UseHintsSystem)
                    ev.Player.ShowHint(plugin.Config.pickedUpBroadcastDuration, plugin.Config.pickedUpBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
                else
                    ev.Player.Broadcast(plugin.Config.pickedUpBroadcastDuration, plugin.Config.pickedUpBroadcast.Replace("%ammo", $"{plugin.Config.ammoUsedPerShot}"));
            }
        }

        public void HurtEvent(HurtingEventArgs ev) {
            try {
                float oldamount = ev.Amount;
                if (ev.Attacker == null || ev.Attacker == ev.Target || plugin.Config.roleBlacklist.Contains(ev.Target.Role) || !plugin.Config.TranquilizeCuffed && ev.Target.IsCuffed) return;
                else if(tranquilized.Contains(ev.Target.UserId)
                    && (ev.DamageType == DamageTypes.Decont || ev.DamageType == DamageTypes.Nuke || ev.DamageType == DamageTypes.Scp939) 
                    && (plugin.Config.teleportAway || plugin.Config.SummonRagdoll)) {
                    ev.Amount = 0;
                    return;
                } else if(IsTranquilizerDamage(ev.DamageType) && !tranquilized.Contains(ev.Target.UserId)) {
                    if(!IsTranquilizer(ev.Attacker.CurrentItem.id) && plugin.Config.silencerRequired && !ev.Attacker.HasSilencer()) return;
                    ev.Amount = plugin.Config.tranquilizerDamage;

                    if(!plugin.Config.FriendlyFire && (ev.Target.Side == ev.Attacker.Side
                        || (plugin.Config.areTutorialSerpentsHand && ev.Attacker.Side == Side.ChaosInsurgency && ev.Target.Role == RoleType.Tutorial)))
                        return;

                    string id = ev.Target.UserId;
                    if(plugin.Config.TreatedLikeNormalDamage || plugin.Config.specialRoles.Keys.Contains(ev.Target.Role)) {
                        if(!scpShots.ContainsKey(id)) scpShots.Add(id, 0);
                        if (plugin.Config.TreatedLikeNormalDamage) scpShots[id] += oldamount;
						else scpShots[id] += 1;
                        if((plugin.Config.TreatedLikeNormalDamage && scpShots[id] >= ev.Target.Health) || (!plugin.Config.TreatedLikeNormalDamage && scpShots[id] >= plugin.Config.specialRoles[ev.Target.Role]))
						{
                            Sleep(ev.Target);
                            scpShots[id] = 0;
                        }
                        return;
                    }
					
                    Sleep(ev.Target);
                }
            } catch(Exception e) {
                e.Print("HurtEvent (TranqHandler)");
            }
        }

        #region Commands
        public void OnCommand(SendingRemoteAdminCommandEventArgs ev) {
            try {
                if(ev.Name.Contains("REQUEST_DATA PLAYER_LIST"))
                    return;

                string cmd = ev.Name.ToLower();
                Player sender = ev.Sender;
                // reload / protect / replaceguns / toggle / sleep / version / setgun / addgun / defaultplugin.Config

                if(cmd.Equals("tg") || cmd.Equals("tgun") || cmd.Equals("tranqgun") || cmd.Equals("tranquilizergun")) {
                    ev.IsAllowed = false;
                    if(ev.Arguments.Count >= 1) {
                        switch(ev.Arguments[0].ToLower()) {
                            case "protect":
                            case "protection":
                            case "armor":
                                if(!sender.CheckPermission("tgun.armor")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }

                                if(ev.Arguments.Count > 1) {
                                    string argument = ev.Arguments[1];
                                    if(argument.ToLower() == "all" || argument == "*") {
                                        int amountArmored = 0;
                                        foreach(Player p in Player.List) {
                                            if(allArmorEnabled && armored.Contains(p.UserId)) {
                                                armored.Remove(p.UserId);
                                                amountArmored++;
                                            } else if(!allArmorEnabled && !armored.Contains(p.UserId)) {
                                                armored.Add(p.UserId);
                                                amountArmored++;
                                            }
                                        }
                                        ev.ReplyMessage = allArmorEnabled ? $"<color=#4ce300>Tranquilizer protection has been disabled for {amountArmored} players.</color>" : $"<color=#4ce300>Tranquilizer protection has been enabled for {amountArmored} players.</color>";
                                        allArmorEnabled = !allArmorEnabled;
                                    } else {
                                        Player p = Player.Get(argument);

                                        if(p == null) {
                                            ev.ReplyMessage = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                                            return;
                                        }

                                        ToggleArmor(p, out string newMessage);
                                        ev.ReplyMessage = newMessage;
                                        return;
                                    }
                                } else {
                                    ToggleArmor(ev.Sender, out string newMessage);
                                    ev.ReplyMessage = newMessage;
                                }
                                return;
                            case "replaceguns":
                                if(!sender.CheckPermission("tgun.replaceguns")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                int a = 0;

                                foreach(Pickup item in Object.FindObjectsOfType<Pickup>()) {
                                    if(item.ItemId == ItemType.GunCOM15 && UnityEngine.Random.Range(1, 100) <= plugin.Config.replaceChance) {
                                        ItemType.GunUSP.Spawn(18, item.Networkposition + new Vector3(0, 1, 0), default, 0, 1, 0);
                                        item.Delete();
                                    }
                                }
                                ev.ReplyMessage = $"<color=#4ce300>A total of {a} COM-15 pistols have been replaced.</color>";
                                return;
                            case "sleep":
                                if(!sender.CheckPermission("tgun.sleep")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                if(ev.Arguments.Count > 1) {
                                    string argument = ev.Arguments[1];
                                    if(argument.ToLower() == "all" || argument == "*") {
                                        int amountSleeping = 0;
                                        foreach(Player p in Player.List) {
                                            if(p.Side != Side.None && !tranquilized.Contains(p.UserId)) {
                                                Sleep(p);
                                                amountSleeping++;
                                            }
                                        }
                                        ev.ReplyMessage = $"<color=#4ce300>A total of {amountSleeping} players have been put to sleep.</color>";
                                    } else {
                                        Player p = Player.Get(argument);

                                        if(p == null) {
                                            ev.ReplyMessage = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                                            return;
                                        } else if(tranquilized.Contains(p.UserId)) {
                                            ev.ReplyMessage = "<color=red>You're already sleeping...?</color>";
                                            return;
                                        }

                                        Sleep(p);
                                        ev.ReplyMessage = $"<color=#4ce300>{p.Nickname} has been forced to sleep. Tell him sweet dreams!</color>";
                                        return;
                                    }
                                } else {
                                    Sleep(ev.Sender);
                                    ev.ReplyMessage = $"<color=#4ce300>You've been forced to sleep. Sweet dreams!</color>";
                                }
                                return;
                            case "version":
                                ev.ReplyMessage = "You're currently using " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                                return;
                            case "receivegun":
                            case "addgun":
                            case "givegun":
                                if(!sender.CheckPermission("tgun.givegun")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                if(ev.Arguments.Count > 1) {
                                    string argument = ev.Arguments[1];
                                    if(argument.ToLower() == "all" || argument == "*") {
                                        int amountGiven = 0;
                                        foreach(Player p in Player.List) {
                                            if(p.Side != Side.None) {
                                                ev.Sender.AddItem(Extensions.GetTranquilizerItem());
                                                amountGiven++;
                                            }
                                        }
                                        ev.ReplyMessage = $"<color=#4ce300>A total of {amountGiven} players received Tranquilizers.</color>";
                                    } else {
                                        Player p = Player.Get(argument);

                                        if(p == null) {
                                            ev.ReplyMessage = $"<color=red>Couldn't find player <b>{argument}</b>.</color>";
                                            return;
                                        }

                                        ev.Sender.AddItem(Extensions.GetTranquilizerItem());
                                        ev.ReplyMessage = $"<color=#4ce300>{p.Nickname} received a Tranquilizer.</color>";
                                        return;
                                    }
                                } else {
                                    ev.Sender.AddItem(Extensions.GetTranquilizerItem());
                                    ev.ReplyMessage = $"<color=#4ce300>Enjoy your Tranquilizer!</color>";
                                }
                                return;
                            case "toggle":
                                if(!sender.CheckPermission("tgun.toggle")) {
                                    ev.ReplyMessage = "<color=red>Permission denied.</color>";
                                    return;
                                }
                                if(plugin.Config.IsEnabledCustom) {
                                    plugin.UnregisterEvents();
                                    plugin.Config.IsEnabledCustom = false;
                                    ev.ReplyMessage = $"<color=#4ce300>The plugin has now been disabled!</color>";
                                } else {
                                    plugin.RegisterEvents();
                                    plugin.Config.IsEnabledCustom = true;
                                    ev.ReplyMessage = $"<color=#4ce300>The plugin has now been enabled!</color>";
                                }
                                return;
                        }
                    }
                    ev.ReplyMessage =
                        $"\n<color=#4ce300>--- [ TranqGun Help ] ---</color>" +
                        $"\n<color=#006eff>Protection:</color> <color=#f7ff9c>Grants you special protection against Tranquilizers.</color>" +
                        $"\n<color=#006eff>ReplaceGuns:</color> <color=#f7ff9c>Replaces any COM-15s with Tranquilizers.</color>" +
                        $"\n<color=#006eff>Sleep:</color> <color=#f7ff9c>Forces the sleep method on someone.</color>" +
                        $"\n<color=#006eff>AddGun:</color> <color=#f7ff9c>Add a Tranquilizer to your inventory.</color>" +
                        $"\n<color=#006eff>Toggle:</color> <color=#f7ff9c>Toggles the plugin's features on/off.</color>" +
                        $"\n<color=#006eff>Version:</color> <color=#f7ff9c>Check the installed version of this plugin.</color>";
                }
            } catch(Exception e) {
                e.Print("OnCommand");
            }
        }
        #endregion

        public void Sleep(Player player) {
            try {
                // Initialize variables & add player to list
                Vector3 oldPos = player.Position;
                PlayerEffectsController controller = player.ReferenceHub.playerEffectsController;
                tranquilized.Add(player.Nickname);
                float sleepDuration = UnityEngine.Random.Range(plugin.Config.sleepDurationMin, plugin.Config.sleepDurationMax), bdd = controller.GetEffect<Bleeding>().Duration;
                bool pd = controller.GetEffect<Corroding>().Enabled, bd = controller.GetEffect<Bleeding>().Enabled;

                // Broadcast message (if enabled)
                if(plugin.Config.tranquilizedBroadcastDuration > 0) {
                    if(plugin.Config.clearBroadcasts)
                        player.ClearBroadcasts();
                    if(plugin.Config.UseHintsSystem)
                        player.ShowHint(plugin.Config.tranquilizedBroadcastDuration, plugin.Config.tranquilizedBroadcast.Replace("%seconds", ((int) sleepDuration).ToString()));
                    else player.Broadcast(plugin.Config.tranquilizedBroadcastDuration, plugin.Config.tranquilizedBroadcast.Replace("%seconds", ((int) sleepDuration).ToString()));
                    
                }

                if(plugin.Config.dropItems)
                    player.Inventory.ServerDropAll();

                if(plugin.Config.usingEffects) {
                    EnableEffects(controller);
                }

                if(plugin.Config.SummonRagdoll) {
                    // Spawn a Ragdoll
                    PlayerStats.HitInfo hitInfo = new PlayerStats.HitInfo(1000f, player.UserId, DamageTypes.Usp, player.Id);

                    player.GameObject.GetComponent<RagdollManager>().SpawnRagdoll(
                        oldPos, player.GameObject.transform.rotation, player.ReferenceHub.playerMovementSync.PlayerVelocity,
                        (int) player.Role, hitInfo, false, player.Nickname, player.Nickname, 0);
                    
                }

                if(plugin.Config.teleportAway) {
                    // Apply effects
                    controller.EnableEffect<Amnesia>(sleepDuration);
                    controller.EnableEffect<Scp268>(sleepDuration);

                    player.Position = new Vector3(plugin.Config.newPos_x, plugin.Config.newPos_y, plugin.Config.newPos_z);
                    Timing.CallDelayed(1f, () => player.ReferenceHub.playerEffectsController.DisableEffect<Decontaminating>());
                }

                Timing.CallDelayed(sleepDuration, () => Wake(player, oldPos, pd, bd, bdd));

            } catch(Exception e) {
                e.Print($"Sleeping {player.Nickname} {e.StackTrace}");
            }
        }

        public void Wake(Player player, Vector3 oldPos, bool inPd = false, bool bleeding = false, float bleedingDur = 3) {
            try {
                tranquilized.Remove(player.UserId);

                if(plugin.Config.SummonRagdoll)
                foreach(Ragdoll doll in Object.FindObjectsOfType<Ragdoll>()) {
                    if(doll.owner.ownerHLAPI_id == player.Nickname) {
                        NetworkServer.Destroy(doll.gameObject);
                    }
                }

                if(plugin.Config.teleportAway) {
                    player.ReferenceHub.playerEffectsController.DisableEffect<Decontaminating>();
                    player.Position = oldPos;

                    if(inPd)
                        player.ReferenceHub.playerEffectsController.EnableEffect<Corroding>();

                    if(bleeding)
                        player.ReferenceHub.playerEffectsController.EnableEffect<Bleeding>(bleedingDur);

                    if(Warhead.IsDetonated) {
                        if(player.CurrentRoom.Zone != ZoneType.Entrance)
                            player.Kill();
                        else
                            foreach(Lift l in Map.Lifts)
                                if(l.elevatorName.ToLower() == "gatea" || l.elevatorName.ToLower() == "gateb")
                                    foreach(Lift.Elevator e in l.elevators)
                                        if(e.target.name == "ElevatorChamber (1)")
                                            if(Vector3.Distance(player.Position, e.target.position) <= 3.6f)
                                                player.Kill();
                    }
                }
            } catch(Exception e) {
                e.Print("Sleeping " + player.Nickname);
            }
        }

        public void EnableEffects(PlayerEffectsController controller) {
            if(plugin.Config.amnesia) {
                controller.EnableEffect<Amnesia>(plugin.Config.amnesiaDuration);
            }

            if(plugin.Config.asphyxiated) {
                controller.EnableEffect<Asphyxiated>(plugin.Config.asphyxiatedDuration);
            }

            if(plugin.Config.blinded) {
                controller.EnableEffect<Blinded>(plugin.Config.blindedDuration);
            }

            if(plugin.Config.concussed) {
                controller.EnableEffect<Concussed>(plugin.Config.concussedDuration);
            }

            if(plugin.Config.deafened) {
                controller.EnableEffect<Deafened>(plugin.Config.deafenedDuration);
            }

            if(plugin.Config.disabled) {
                controller.EnableEffect<Disabled>(plugin.Config.disabledDuration);
            }

            if(plugin.Config.ensnared) {
                controller.EnableEffect<Ensnared>(plugin.Config.ensnaredDuration);
            }

            if(plugin.Config.exhausted) {
                controller.EnableEffect<Exhausted>(plugin.Config.exhaustedDuration);
            }

            if(plugin.Config.flash) {
                controller.EnableEffect<Flashed>(plugin.Config.flashDuration);
            }

            if(plugin.Config.poisoned) {
                controller.EnableEffect<Poisoned>(plugin.Config.poisonedDuration);
            }

            if(plugin.Config.bleeding) {
                controller.EnableEffect<Bleeding>(plugin.Config.bleedingDuration);
            }

            if(plugin.Config.sinkhole) {
                controller.EnableEffect<SinkHole>(plugin.Config.sinkholeDuration);
            }

            if(plugin.Config.invisible) {
                controller.EnableEffect<Scp268>(plugin.Config.invisibleDuration);
            }

            if(plugin.Config.speed) {
                controller.EnableEffect<Scp207>(plugin.Config.speedDuration);
            }

            if(plugin.Config.hemorrhage) {
                //hemrorrrogohgage
                controller.EnableEffect<Hemorrhage>(plugin.Config.hemorrhageDuration);
            }

            if(plugin.Config.decontaminating) {
                controller.EnableEffect<Decontaminating>(plugin.Config.decontaminatingDuration, true);
            }
        }

        public bool IsTranquilizerDamage(DamageTypes.DamageType damageType) 
            => (plugin.Config.Com15IsTranquilizer && damageType == DamageTypes.Com15) || (plugin.Config.UspIsTranquilizer && damageType == DamageTypes.Usp) || (plugin.Config.Mp7IsTranquilizer && damageType == DamageTypes.Mp7) || (plugin.Config.Project90IsTranquilizer && damageType == DamageTypes.P90) || (plugin.Config.E11IsTranquilizer && damageType == DamageTypes.E11StandardRifle) || (plugin.Config.LogicerIsTranquilizer && damageType == DamageTypes.Logicer);

        public IEnumerator<float> DelayedReplace() {
            yield return Timing.WaitForSeconds(2f);
            foreach(Pickup item in Object.FindObjectsOfType<Pickup>()) {
                if(item.ItemId == ItemType.GunCOM15 && UnityEngine.Random.Range(1, 100) <= plugin.Config.replaceChance) {
                    ItemType.GunUSP.Spawn(18, item.Networkposition + new Vector3(0, 1, 0), default, 0, 1, 0);
                    item.Delete();
                }
            }
        }

        public bool IsTranquilizer(ItemType type) =>
            (type == ItemType.GunCOM15 && plugin.Config.Com15IsTranquilizer)
                || (type == ItemType.GunUSP && plugin.Config.UspIsTranquilizer)
                || (type == ItemType.GunMP7 && plugin.Config.Mp7IsTranquilizer)
                || (type == ItemType.GunProject90 && plugin.Config.Project90IsTranquilizer)
                || (type == ItemType.GunE11SR && plugin.Config.E11IsTranquilizer)
                || (type == ItemType.GunLogicer && plugin.Config.LogicerIsTranquilizer);

        // I'm fucking lazy 
        private void ToggleArmor(Player p, out string ReplyMessage) {
            if(armored.Contains(p.UserId)) {
                armored.Remove(p.UserId);
                ReplyMessage = $"<color=red>{p.Nickname} is no longer protected against Tranquilizers.</color>";
            } else {
                armored.Add(p.UserId);
                ReplyMessage = $"<color=#4ce300>{p.Nickname} is now protected against Tranquilizers.</color>";
            }
        }
    }
}
