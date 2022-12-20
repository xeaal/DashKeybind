using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace DashKeybind {
	public class DashKeybind : ModSystem {
		public static ModKeybind Dash {
			get;
			private set;
		}
		private void Player_DoCommonDashHandle(On.Terraria.Player.orig_DoCommonDashHandle orig, Terraria.Player self, out int dir, out bool dashing, Player.DashStartAction dashStartAction = null) {
			dir = 0;
			dashing = false;
			if (DashKeybind.Dash.JustPressed) {
				switch(self.direction) {
					case -1:
						if (self.controlRight) {
							dir = 1;
							dashing = true;
							self.timeSinceLastDashStarted = 0;
							if (dashStartAction == null)
								return;
							dashStartAction(1);
						}
						else {
							dir = -1;
							dashing = true;
							self.timeSinceLastDashStarted = 0;
							if (dashStartAction == null)
								return;
							dashStartAction(-1);
						}
						break;
					case 1:
						if (self.controlLeft) {
							dir = -1;
							dashing = true;
							self.timeSinceLastDashStarted = 0;
							if (dashStartAction == null)
								return;
							dashStartAction(-1);
						}
						else {
							dir = 1;
							dashing = true;
							self.timeSinceLastDashStarted = 0;
							if (dashStartAction == null)
								return;
							dashStartAction(1);
						}
						break;
				}
			}
		}
		public override void Load() {
			On.Terraria.Player.DoCommonDashHandle += Player_DoCommonDashHandle;
			Dash = KeybindLoader.RegisterKeybind (Mod, "Dash", "Mouse3");
		}
		public override void Unload() {
			Dash = null;
			On.Terraria.Player.DoCommonDashHandle -= Player_DoCommonDashHandle;
		}
	}
}
