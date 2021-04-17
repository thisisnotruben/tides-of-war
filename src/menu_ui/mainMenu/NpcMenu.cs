using Godot;
using GC = Godot.Collections;
using Game.Actor;
using Game.Actor.State;
using System;
using Game.Quest;
namespace Game.Ui
{
	public class NpcMenu : GameMenu
	{
		// External
		private SaveLoadModel saveLoadModel;
		private MenuMasterController menuMaster;

		// Internal
		public MerchantController store;
		private Control dialogueContainer, dialogue;
		private Button speak, trade, sellBuy;

		private Npc focusedNpc;
		private bool canTrade, canTalk, hasQuest;
		private Control focusedControl;

		public override void _Ready()
		{
			store = GetNode<MerchantController>("v/merchantView");
			store.Connect("visibility_changed", this, nameof(OnStoreVisibilityChanged));

			dialogueContainer = GetNode<BaseButton>("v/dialoguePopup");
			dialogueContainer.Connect("pressed", this, nameof(OnDialogueNext));
			dialogueContainer.Connect("visibility_changed", this, nameof(OnDialogueVisibilityChanged));

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
			store.Visible = dialogueContainer.Visible =
				canTrade = canTalk = hasQuest = false;
			focusedControl = null;
			focusedNpc = null;
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
			focusedControl = dialogueContainer;

			dialogue = Globals.dialogic.Start(Map.Map.map.Name + "-" + focusedNpc.Name.Split("-")[0]);
			dialogue.Connect("tree_exiting", this, nameof(ClearDialogue));

			dialogueContainer.AddChild(dialogue);
			dialogue.RectPosition = Vector2.Zero;

			store.Hide();
			dialogueContainer.Show();
		}
		private void ClearDialogue()
		{
			dialogueContainer.Hide();
			focusedControl = null;

			dialogue?.QueueFree();
			dialogue = null;
		}
		private void OnDialogueVisibilityChanged() { speak.Visible = canTalk && !dialogueContainer.Visible; }
		private void CheckInteractionValid(FSM.State state)
		{
			if (!GetTree().Paused)
			{
				if (player.attacking
				|| (focusedNpc?.attacking ?? true)
				|| !WithinSpeakingDistance(focusedNpc.GlobalPosition))
				{
					Hide();
				}
				else if (player.moving || focusedNpc.moving)
				{
					SetProcess(true);
				}
			}
		}
		private void OnDialogueNext() // dialogic can only read this type of input
		{
			InputEventAction dialogueNext = new InputEventAction();
			dialogueNext.Action = "dialogue_next";
			dialogueNext.Pressed = true;
			Input.ParseInputEvent(dialogueNext);
		}
		public override void _Process(float delta) // only used for 'CheckInteractionValid'
		{
			if (focusedNpc == null || !WithinSpeakingDistance(focusedNpc.GlobalPosition))
			{
				Hide();
			}
			else if (!focusedNpc.moving && !player.moving)
			{
				SetProcess(false);
			}
		}
		/* 
		* DIALOGUE END 
		* STORE STAT
		*/
		private void StartStore()
		{
			focusedControl = store;

			saveLoadModel.SetCurrentGameImage();

			dialogueContainer.Visible = sellBuy.Pressed = false;
			sellBuy.Text = "Sell";

			store.merchant = focusedNpc;
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
			if (player.dead || focusedNpc == npc)
			{
				Hide();
				return;
			}

			Hide(); // resets view

			focusedNpc = npc;
			canTrade = Globals.contentDB.HasData(npc.Name) && Globals.contentDB.GetData(npc.Name).merchandise.Length > 0;
			canTalk = Globals.unitDB.HasData(npc.Name) && Globals.unitDB.GetData(npc.Name).dialogue;
			hasQuest = QuestMaster.HasQuestOrQuestExtraContent(npc.worldName, npc.GetPath());

			bool interactable = !npc.enemy && WithinSpeakingDistance(npc.GlobalPosition) && (canTalk || canTrade);

			Action showNpcMenuOptions = () =>
			{
				ClearDialogue();
				speak.Visible = canTalk;
				trade.Visible = canTrade;
				sellBuy.Visible = false;
				Visible = canTalk || canTrade;

				if (Visible)
				{
					Globals.TryLinkSignal(player.fsm, nameof(FSM.StateChanged), this, nameof(CheckInteractionValid), true);
					Globals.TryLinkSignal(focusedNpc.fsm, nameof(FSM.StateChanged), this, nameof(CheckInteractionValid), true);
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