using System;
using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour.HookGen;

namespace DashKeybind {
	public class DashKeybind : Mod {
		public static ModKeybind Dash;
		private void VanillaDashPatch(On.Terraria.Player.orig_DoCommonDashHandle orig, Terraria.Player self, out int dir, out bool dashing, Player.DashStartAction dashStartAction = null) {
			dir = 0;
			dashing = false;
			if (DashKeybind.Dash.JustPressed) {
				switch(self.direction) {
					case -1:
						if (self.controlRight) {
							dir = 1;
							if (dashStartAction == null)
								return;
						}
						else {
							dir = -1;
							if (dashStartAction == null)
								return;
						}
						dashing = true;
						self.timeSinceLastDashStarted = 0;
						dashStartAction(dir);
						break;
					case 1:
						if (self.controlLeft) {
							dir = -1;
							if (dashStartAction == null)
								return;
						}
						else {
							dir = 1;
							if (dashStartAction == null)
								return;
						}
						dashing = true;
						self.timeSinceLastDashStarted = 0;
						dashStartAction(dir);
						break;
				}
			}
		}
		public override void Load() {
			On.Terraria.Player.DoCommonDashHandle += VanillaDashPatch;
			Dash = KeybindLoader.RegisterKeybind (this, "Dash", "Mouse3");
		}
		public override void Unload() {
			Dash = null;
			On.Terraria.Player.DoCommonDashHandle -= VanillaDashPatch;
		}
	}
	[JITWhenModsEnabled("CalamityMod")]
	public sealed class CalamityDetour : ModSystem {
		public static ModKeybind GSDash;
		public override void Load() {
			if (!ModLoader.HasMod("CalamityMod")) return;
//			GSDash = KeybindLoader.RegisterKeybind (Mod, "God Slayer Dash", "Mouse3");
			PatchCalamity();
		}
		private static void PatchCalamity() {
			HookEndpointManager.Add(typeof(CalamityMod.CalPlayer.CalamityPlayer).GetMethod("HandleHorizontalDash", BindingFlags.Public | BindingFlags.Instance), HDashPatch);
//			HookEndpointManager.Add(typeof(CalamityMod.CalPlayer.CalamityPlayer).GetMethod("HandleOmnidirectionalDash", BindingFlags.Public | BindingFlags.Instance), ODDashPatch);
		}
		private delegate bool orig_HandleHorizontalDash(CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction);
		private static bool HDashPatch(orig_HandleHorizontalDash orig, CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction) {
			direction = CalamityMod.Enums.DashDirection.Directionless;
			bool result = false;
			if (DashKeybind.Dash.JustPressed) {
				switch(self.Player.direction) {
					case -1:
						if (self.Player.controlRight) {
							direction = CalamityMod.Enums.DashDirection.Right;
						}
						else direction = CalamityMod.Enums.DashDirection.Left;
						result = true;
						self.dashTimeMod = 0;
						break;
					case 1:
						if (self.Player.controlLeft) {
							direction = CalamityMod.Enums.DashDirection.Left;
						}
						else direction = CalamityMod.Enums.DashDirection.Right;
						result = true;
						self.dashTimeMod = 0;
						break;
				}
			}
			return result;
		}
/*		private delegate bool orig_HandleOmnidirectionalDash(CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction);
		private static bool ODDashPatch(orig_HandleOmnidirectionalDash orig, CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction) {
			direction = CalamityMod.Enums.DashDirection.Directionless;
			bool result = false;
			if (CalamityDetour.GSDash.JustPressed) {
				if (!(self.Player.controlLeft && self.Player.controlRight) && (self.Player.controlUp || self.Player.controlDown)) {
					if (self.Player.controlUp)
						direction = CalamityMod.Enums.DashDirection.Up;
					if (self.Player.controlDown) {
						direction = CalamityMod.Enums.DashDirection.Down;
						self.Player.maxFallSpeed = 50f;
					}
					result = true;
					self.dashTimeMod = 0;
				}
				else switch (self.Player.direction) {
					case -1:
						if (self.Player.controlUp && self.Player.controlRight)
							direction = CalamityMod.Enums.DashDirection.UpRight;
						else if (self.Player.controlRight)
							direction = CalamityMod.Enums.DashDirection.Right;
						else if (self.Player.controlDown && self.Player.controlRight)
							direction = CalamityMod.Enums.DashDirection.DownRight;
						else direction = CalamityMod.Enums.DashDirection.Left;
						result = true;
						self.dashTimeMod = 0;
						break;
					case 1:
						if (self.Player.controlUp && self.Player.controlLeft)
							direction = CalamityMod.Enums.DashDirection.UpLeft;
						else if (self.Player.controlLeft)
							direction = CalamityMod.Enums.DashDirection.Left;
						else if (self.Player.controlDown && self.Player.controlLeft) {
							direction = CalamityMod.Enums.DashDirection.DownLeft;
							self.Player.maxFallSpeed = 50f;
						}
						else direction = CalamityMod.Enums.DashDirection.Right;
						result = true;
						self.dashTimeMod = 0;
						break;
				}
			}
			Main.NewText(direction);
			return result;
		}*/
	}
}
