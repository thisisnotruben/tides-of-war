using Godot;
using GC = Godot.Collections;
using Game.Actor;
using Game.Actor.State;
using Game.Actor.Doodads;
using System;
using Game.Quest;
using Game.Database;
namespace Game.Ui
{
	public class NpcMenu : GameMenu
	{
		// External
		private SaveLoadModel saveLoadModel;
		private MenuMasterController menuMaster;

		// Internal
		public MerchantController store;
		private Control dialogue;
		private Button speak, trade, sellBuy;

		private Npc npc;
		private bool canTrade, canTalk;
		private Control focusedControl;
		private WorldQuest worldQuest;

		public override void _Ready()
		{
			store = GetNode<MerchantController>("v/merchantView");
			store.Connect("visibility_changed", this, nameof(OnStoreVisibilityChanged));

			Control optionsContainer = GetNode<Control>("v/mainButtonGroup");
			optionsContainer.GetNode("speak").Connect("pressed", this, nameof(StartDialogue));
			optionsContainer.GetNode("trade").Connect("pressed", this, nameof(StartStore));

			speak = optionsContainer.GetNode<Button>("speak");
			trade = optionsContainer.GetNode<Button>("trade");
			sellBuy = optionsContainer.GetNode<Button>("sellBuy");

			optionsContainer.GetNode<BaseButton>("close").Connect("pressed", this, nameof(OnClose));

			Connect("hide", this, nameof(OnHide));
			SetProcess(false);
		}
		public NpcMenu Init(MenuMasterController menuMaster, InventoryController inventoryController, SpellBookController spellBookController,
		InventoryModel playerInventory, InventoryModel playerSpellBook, SaveLoadModel saveLoadModel)
		{
			this.menuMaster = menuMaster;
			this.saveLoadModel = saveLoadModel;

			store.Init(
				playerInventory, inventoryController,
				playerSpellBook, spellBookController,
				sellBuy
			);

			return this;
		}
		private void OnHide()
		{
			store.Visible = canTrade = canTalk = false;
			focusedControl = null;
			worldQuest = null;
			npc = null;
			ClearDialogue();
			SetProcess(false);

			// remove all connections from characters relating to dialogue
			foreach (GC.Dictionary connectionPacket in GetIncomingConnections())
			{
				if (connectionPacket["signal_name"].Equals(nameof(FSM.StateChanged))
				&& connectionPacket["method_name"].Equals(nameof(CheckInteractionValid)))
				{
					(connectionPacket["source"] as Godot.Object)?.Disconnect(
						connectionPacket["signal_name"].ToString(),
						this, connectionPacket["method_name"].ToString());
				}
			}
		}
		private void OnClose()
		{
			if (focusedControl != null)
			{
				focusedControl.Hide();
				focusedControl = null;
				ClearDialogue();
			}
			else
			{
				Hide();
			}
		}
		/*
		 * DIALOGUE Start
		*/
		private void StartDialogue()
		{
			if (dialogue != null || npc == null)
			{
				return; // to avoid double-clicking
			}

			// setup dialogic variables
			Func<QuestMaster.QuestStatus, bool> isStatus = (QuestMaster.QuestStatus status) =>
		   {
			   return worldQuest != null ? worldQuest.status == status : false;
		   };

			GC.Dictionary<string, bool> questDefinitions = new GC.Dictionary<string, bool>()
			{
				{"questActive", isStatus(QuestMaster.QuestStatus.ACTIVE)},
				{"questAvailable", isStatus(QuestMaster.QuestStatus.AVAILABLE)},
				{"questCompleted", isStatus(QuestMaster.QuestStatus.COMPLETED)},
				{"questDelivered", isStatus(QuestMaster.QuestStatus.DELIVERED)},
				{"questObjective", worldQuest?.IsPartOfObjective(npc.GetPath(),
					QuestDB.QuestType.TALK) ?? false}
			};

			bool questAny = false;
			foreach (string definition in questDefinitions.Keys)
			{
				questAny = questAny || questDefinitions[definition];

				Globals.dialogic.SetVariable(definition,
					(questDefinitions[definition] ? 1 : 0).ToString());
			}

			Globals.dialogic.SetVariable("npcName", npc.Name);
			Globals.dialogic.SetVariable("questAny", (questAny ? 1 : 0).ToString());

			// init dialogic
			dialogue = focusedControl = Globals.dialogic.Start(
				worldQuest?.quest.dialogue ?? Globals.unitDB.GetData(npc.Name).dialogue, false
			);

			dialogue.Connect("tree_exiting", this, nameof(ClearDialogue));
			dialogue.Connect("visibility_changed", this, nameof(OnDialogueVisibilityChanged));
			dialogue.Connect("dialogic_signal", this, nameof(OnDialogueSignalCallback));

			// add dialogic to scene
			store.GetParent().AddChildBelowNode(store, dialogue);
			dialogue.RectPosition = Vector2.Zero;
			store.Hide();
		}
		private void ClearDialogue()
		{
			if (focusedControl == dialogue)
			{
				focusedControl = null;
			}
			dialogue?.Hide();
			dialogue?.QueueFree();
			dialogue = null;
		}
		private void OnDialogueVisibilityChanged() { speak.Visible = (canTalk || worldQuest != null) && (!dialogue?.Visible ?? true); }
		private void OnDialogueSignalCallback(object value)
		{
			switch (value.ToString().Trim().ToLower())
			{
				case "start":
					if (worldQuest != null)
					{
						PlaySound(NameDB.UI.QUEST_ACCEPT);
						Globals.questMaster.ActivateQuest(worldQuest.quest.questName);
						// TODO: add quest to quest log
					}
					break;

				case "complete":
					if (worldQuest?.IsCompleted() ?? false)
					{
						PlaySound(NameDB.UI.QUEST_FINISH);

						// turn in items
						if (!worldQuest.quest.keepRewardItems)
						{
							worldQuest.TurnInItems(player.menu.playerMenu.playerInventory);
						}

						// start next quest dialogue if anys
						WorldQuest nextQuest = Globals.questMaster.DeliverQuest(worldQuest.quest.questName);
						if (nextQuest != null)
						{
							if (dialogue == null)
							{
								OnDialogueNext(nextQuest);
							}
							else
							{
								dialogue.Connect("tree_exited", this, nameof(OnDialogueNext),
									new GC.Array() { nextQuest });
							}
						}

						// reward gold if any
						if (player.gold > 0)
						{
							player.gold += worldQuest.quest.goldReward;
							player.SpawnCombatText(worldQuest.quest.goldReward.ToString(), CombatText.TextType.GOLD);
						}
					}
					break;

				case "objective":
					QuestDB.ExtraContentData extraContentData;

					if (Globals.questMaster.CheckQuests(npc.GetPath(), npc.worldName, QuestDB.QuestType.TALK)
					&& Globals.questMaster.TryGetExtraQuestContent(npc.worldName, out extraContentData))
					{
						if (extraContentData.gold > 0)
						{
							player.gold += extraContentData.gold;
							player.SpawnCombatText(extraContentData.gold.ToString(), CombatText.TextType.GOLD);
						}

						if (Globals.itemDB.HasData(extraContentData.reward)
						&& player.menu.playerMenu.playerInventory.AddCommodity(extraContentData.reward) == -1)
						{
							// TODO: what if is inventory full
						}
					}
					break;
			}
		}
		private void CheckInteractionValid(FSM.State state)
		{
			if (!GetTree().Paused)
			{
				if (player.attacking
				|| (npc?.attacking ?? true)
				|| !WithinSpeakingDistance(npc.GlobalPosition))
				{
					Hide();
				}
				else if (player.moving || npc.moving)
				{
					SetProcess(true);
				}
			}
		}
		public override void _Process(float delta) // only used for 'CheckInteractionValid'
		{
			if (npc == null || !WithinSpeakingDistance(npc.GlobalPosition))
			{
				Hide();
			}
			else if (!npc.moving && !player.moving)
			{
				SetProcess(false);
			}
		}
		private void OnDialogueNext(WorldQuest nextQuest)
		{
			worldQuest = nextQuest;
			StartDialogue();
		}
		/*
		* DIALOGUE END
		* STORE STAT
		*/
		private void StartStore()
		{
			ClearDialogue();
			focusedControl = store;

			saveLoadModel.SetCurrentGameImage();

			sellBuy.Pressed = false;
			sellBuy.Text = "Sell";

			store.merchant = npc;
			store.Show();
		}
		private void OnStoreVisibilityChanged()
		{
			bool isVisible = store.Visible;
			GetTree().Paused = sellBuy.Visible = isVisible;

			trade.Visible = canTrade && !isVisible;
		}
		/*
		* STORE END
		*/
		private bool WithinSpeakingDistance(Vector2 npcGlobalPos) { return 3 >= Map.Map.map.getAPath(player.GlobalPosition, npcGlobalPos).Count; }
		public void NpcInteract(Npc npc)
		{
			Hide(); // resets view

			if (player.dead)
			{
				return;
			}

			this.npc = npc;

			canTalk = Globals.unitDB.HasData(npc.Name) && !Globals.unitDB.GetData(npc.Name).dialogue.Empty();
			canTrade = Globals.contentDB.HasData(npc.Name) && Globals.contentDB.GetData(npc.Name).merchandise.Length > 0;

			bool hasQuest =
				Globals.questMaster.IsPartOfObjective(npc.GetPath(), out worldQuest, QuestDB.QuestType.TALK)
				|| Globals.questMaster.HasOutstandingQuest(npc.GetPath(), out worldQuest)
				|| Globals.questMaster.HasQuestToOffer(npc.GetPath(), out worldQuest)
				|| Globals.questMaster.TryGetLastDeliveredQuest(npc.GetPath(), out worldQuest);

			bool interactable = !npc.enemy && WithinSpeakingDistance(npc.GlobalPosition) && (canTalk || canTrade || hasQuest);

			Action showNpcMenuOptions = () =>
			{
				ClearDialogue();
				speak.Visible = canTalk || hasQuest;
				trade.Visible = canTrade;
				sellBuy.Visible = false;
				Visible = canTalk || canTrade || hasQuest;

				if (Visible)
				{
					Globals.TryLinkSignal(player.fsm, nameof(FSM.StateChanged), this, nameof(CheckInteractionValid), true);
					Globals.TryLinkSignal(npc.fsm, nameof(FSM.StateChanged), this, nameof(CheckInteractionValid), true);
				}
			};

			if (menuMaster.hud.targetStatus.IsCharacterConnected(npc))
			{
				if (interactable)
				{
					showNpcMenuOptions();
				}
				else
				{
					menuMaster.ClearTarget();
				}
			}
			else
			{
				menuMaster.SetTargetDisplay(npc);

				player.target = npc;
				if (npc.enemy && player.pos.DistanceTo(npc.pos) <= player.stats.weaponRange.value)
				{
					player.state = FSM.State.ATTACK;
				}
				else if (interactable)
				{
					showNpcMenuOptions();
				}
			}
		}
	}
}