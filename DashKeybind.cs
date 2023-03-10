using System;
using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Cil;
using Mono;

namespace DashKeybind {
	public class DashKeybind : Mod {
		public static ModKeybind Dash;
		private void VanillaDashPatch(On.Terraria.Player.orig_DoCommonDashHandle orig, Terraria.Player self, out int dir, out bool dashing, Player.DashStartAction dashStartAction) {
			dir = 0;
			dashing = false;
			if (DashKeybind.Dash.JustPressed) {
				switch(self.direction) {
					case -1:
						if (self.controlRight)
							dir = 1;
						else
							dir = -1;
						break;
					case 1:
						if (self.controlLeft)
							dir = -1;
						else
							dir = 1;
						break;
				}
				dashing = true;
				self.timeSinceLastDashStarted = 0;
				dashStartAction?.Invoke(dir);
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
		public override void Load() {
			if (!ModLoader.HasMod("CalamityMod")) return;
			PatchCalamity();

		}
		private static void PatchCalamity() {
			HookEndpointManager.Add(typeof(CalamityMod.CalPlayer.CalamityPlayer).GetMethod("HandleHorizontalDash", BindingFlags.Public | BindingFlags.Instance), HDashPatch);
			HookEndpointManager.Add(typeof(CalamityMod.CalPlayer.CalamityPlayer).GetMethod("HandleOmnidirectionalDash", BindingFlags.Public | BindingFlags.Instance), ODDashPatch);
			HookEndpointManager.Modify(typeof(CalamityMod.CalPlayer.CalamityPlayer).GetMethod("ProcessTriggers", BindingFlags.Public | BindingFlags.Instance), ODDashCheckPatch);
		}
		private delegate bool orig_HandleHorizontalDash(CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction);
		private static bool HDashPatch(orig_HandleHorizontalDash orig, CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction) {
			direction = CalamityMod.Enums.DashDirection.Directionless;
			bool result = false;
			if (DashKeybind.Dash.JustPressed) {
				switch(self.Player.direction) {
					case -1:
						if (self.Player.controlRight)
							direction = CalamityMod.Enums.DashDirection.Right;
						else
							direction = CalamityMod.Enums.DashDirection.Left;
						break;
					case 1:
						if (self.Player.controlLeft)
							direction = CalamityMod.Enums.DashDirection.Left;
						else
							direction = CalamityMod.Enums.DashDirection.Right;
						break;
				}
				result = true;
				self.dashTimeMod = 0;
			}
			return result;
		}
		private delegate bool orig_HandleOmnidirectionalDash(CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction);
		private static bool ODDashPatch(orig_HandleOmnidirectionalDash orig, CalamityMod.CalPlayer.CalamityPlayer self, out CalamityMod.Enums.DashDirection direction) {
			direction = CalamityMod.Enums.DashDirection.Directionless;
			bool result = false;
			if (self.godSlayerDashHotKeyPressed) {
				if (!(self.Player.controlLeft && self.Player.controlRight) && (self.Player.controlUp || self.Player.controlDown)) {
					if (self.Player.controlUp)
						direction = CalamityMod.Enums.DashDirection.Up;
					if (self.Player.controlDown) {
						direction = CalamityMod.Enums.DashDirection.Down;
						self.Player.maxFallSpeed = 50f;
					}
				}
				else switch (self.Player.direction) {
					case -1:
						if (self.Player.controlUp && self.Player.controlRight)
							direction = CalamityMod.Enums.DashDirection.UpRight;
						else if (self.Player.controlRight)
							direction = CalamityMod.Enums.DashDirection.Right;
						else if (self.Player.controlDown && self.Player.controlRight)
							direction = CalamityMod.Enums.DashDirection.DownRight;
						else
							direction = CalamityMod.Enums.DashDirection.Left;
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
						else
							direction = CalamityMod.Enums.DashDirection.Right;
						break;
				}
				result = true;
				self.dashTimeMod = 0;
			}
			return result;
		}
		private static void ODDashCheckPatch(ILContext il) {
			var c = new ILCursor(il);
			if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdfld(typeof(Player), "controlUp")))
				throw new Exception("ControlUp field not found");
			if (!c.TryGotoPrev(MoveType.Before, i => i.MatchLdarg(0)))
				throw new Exception("Ldarg field not found");
			c.RemoveRange(16);
		}
	}
}
