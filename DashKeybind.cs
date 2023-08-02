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
		private static void VanillaDashPatch(Terraria.On_Player.orig_DoCommonDashHandle orig, Terraria.Player self, out int dir, out bool dashing, Player.DashStartAction dashStartAction) {
			dir = 0;
			dashing = false;
			if (self.whoAmI != Main.myPlayer)
				orig.Invoke(self, out dir, out dashing, dashStartAction);
			else if (DashKeybind.Dash.JustPressed) {
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
				if (self.dashTime > 0) {
					self.dashTime--;
				}
				if (self.dashTime < 0) {
					self.dashTime++;
				}
				if ((self.dashTime <= 0 && self.direction == -1) || (self.dashTime >= 0 && self.direction == 1)) {
					self.dashTime = 15;
					return;
				}
				dashing = true;
				self.dashTime = 0;
				self.timeSinceLastDashStarted = 0;
				if (dashStartAction != null)
					dashStartAction?.Invoke(dir);
			}
			else {
				dir = 0;
				dashing = false;
			}
		}
		public override void Load() {
			if (ModLoader.HasMod("CalamityMod")) return;
			Terraria.On_Player.DoCommonDashHandle += VanillaDashPatch;
			Dash = KeybindLoader.RegisterKeybind (this, "Dash", "Mouse3");
		}
		public override void Unload() {
			Dash = null;
			Terraria.On_Player.DoCommonDashHandle -= VanillaDashPatch;
		}
	}
}
