﻿using GridDominance.Shared.Resources;
using System.Linq;
using MonoSAMFramework.Portable.DeviceBridge;
using MonoSAMFramework.Portable.LogProtocol;
using MonoSAMFramework.Portable.Localization;
using MonoSAMFramework.Portable.ColorHelper;
using GridDominance.Levelfileformat.Blueprint;
using System;
using GridDominance.Shared.Screens.OverworldScreen.HUD;
using MonoSAMFramework.Portable.GameMath.Geometry;

#pragma warning disable 162
namespace GridDominance.Shared.Screens.OverworldScreen.Entities
{
	class OverworldNode_W2 : OverworldNode_Graph
	{
		private const string IAB_CODE = GDConstants.IAB_WORLD2;

		public OverworldNode_W2(GDOverworldScreen scrn, FPoint pos) : base(scrn, pos, Levels.WORLD_002)
		{
			//
		}
		 
		private bool? _isGPUnlocked = null;
		protected override UnlockState IsUnlocked()
		{
			if (_isGPUnlocked == null)
			{
				var supplyNodes = Levels.WORLD_001.Nodes.Where(n => n.OutgoingPipes.Any(p => p.Target == Blueprint.ID));

				_isGPUnlocked = supplyNodes.Any(l => MainGame.Inst.Profile.GetLevelData(l).HasAnyCompleted());
			}

			if (GDConstants.USE_IAB)
			{
				// LIGHT VERSION

				if (!_isGPUnlocked.Value) return UnlockState.Locked;

				if (MainGame.Inst.Profile.PurchasedWorlds.Contains(Blueprint.ID)) return UnlockState.Unlocked;
				
				var ip = MainGame.Inst.Bridge.IAB.IsPurchased(IAB_CODE);

				switch (ip)
				{
					case PurchaseQueryResult.Purchased:
						MainGame.Inst.Profile.PurchasedWorlds.Add(Blueprint.ID);
						MainGame.Inst.SaveProfile();
						return UnlockState.Unlocked;
						
					case PurchaseQueryResult.NotPurchased:
					case PurchaseQueryResult.Cancelled:
						return UnlockState.NeedsPurchase;
						
					case PurchaseQueryResult.Error:
						Owner.HUD.ShowToast(L10N.T(L10NImpl.STR_IAB_TESTERR), 40, FlatColors.Pomegranate, FlatColors.Foreground, 2.5f);
						return UnlockState.NeedsPurchase;
						
					case PurchaseQueryResult.Refunded:
						MainGame.Inst.Profile.PurchasedWorlds.Remove(Blueprint.ID);
						MainGame.Inst.SaveProfile();
						return UnlockState.Locked;
						
					case PurchaseQueryResult.NotConnected:
						Owner.HUD.ShowToast(L10N.T(L10NImpl.STR_IAB_TESTNOCONN), 40, FlatColors.Pomegranate, FlatColors.Foreground, 2.5f);
						return UnlockState.NeedsPurchase;
						
					case PurchaseQueryResult.CurrentlyInitializing:
						Owner.HUD.ShowToast(L10N.T(L10NImpl.STR_IAB_TESTINPROGRESS), 40, FlatColors.Pomegranate, FlatColors.Foreground, 2.5f);
						return UnlockState.NeedsPurchase;
						
					default:
						SAMLog.Error("EnumSwitch", "IsUnlocked()", "MainGame.Inst.Bridge.IAB.IsPurchased(MainGame.IAB_WORLD2)) -> " + ip);
						return UnlockState.NeedsPurchase;
				}
			}
			else
			{
				// FULL VERSION

				return _isGPUnlocked.Value ? UnlockState.Unlocked : UnlockState.Locked;
			}
		}

		protected override void OnClickDeny()
		{
			if (_isGPUnlocked == false)
			{
				base.OnClickDeny();
			}
			else
			{
				LevelBlueprint[] previews =
				{
					Levels.LEVELS[Guid.Parse(@"b16b00b5-0001-4000-0000-000002000019")],
					Levels.LEVELS[Guid.Parse(@"b16b00b5-0001-4000-0000-000002000015")],
					Levels.LEVELS[Guid.Parse(@"b16b00b5-0001-4000-0000-000002000012")],
					Levels.LEVELS[Guid.Parse(@"b16b00b5-0001-4000-0000-000002000013")],
					Levels.LEVELS[Guid.Parse(@"b16b00b5-0001-4000-0000-000002000005")],
					Levels.LEVELS[Guid.Parse(@"b16b00b5-0001-4000-0000-000002000025")],
					Levels.LEVELS[Guid.Parse(@"b16b00b5-0001-4000-0000-000002000027")],
				};

				Owner.HUD.AddModal(new WorldPreviewPanel(previews, Blueprint.ID, IAB_CODE, 2), true, 0.8f, 1f);
			}
		}
	}
}
