using Godot;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class QuestLogController : GameMenu
	{
		private Control main, focusedEntry, dialogue;
		private Tree entryTree;
		private Label questTitle, questGiverName;
		private TextureRect questGiverPortrait;
		private RichTextLabel questProgressDes;
		private Button showDialogueButton, switchQuestLogButton;

		private WorldQuest focusedWorldQuest;
		private bool isCurrentQuests = true;

		public override void _Ready()
		{
			main = GetChild<Control>(0);
			switchQuestLogButton = main.GetNode<Button>("button");
			switchQuestLogButton.Connect("pressed", this, nameof(OnSwitchQuestLogPressed));

			entryTree = main.GetNode<Tree>("tree");
			entryTree.Connect("cell_selected", this, nameof(OnTreeCellSelected));
			entryTree.CreateItem();

			focusedEntry = GetChild<Control>(1);
			questTitle = focusedEntry.GetNode<Label>("questTitle");
			questGiverName = focusedEntry.GetNode<Label>("hBoxContainer/QuestGiverName");
			questGiverPortrait = focusedEntry.GetNode<TextureRect>("hBoxContainer/QuestGiverPortrait");
			questProgressDes = focusedEntry.GetNode<RichTextLabel>("questProgress");
			showDialogueButton = focusedEntry.GetNode<Button>("buttons/dialogue");

			questProgressDes.Connect("visibility_changed", this, nameof(OnQuestProgressdDesVisibilityChanged));
			focusedEntry.GetNode<Button>("buttons/back").Connect("pressed", this, nameof(OnFocusedEntryBack));
			showDialogueButton.Connect("pressed", this, nameof(OnShowDialoguePressed));

			ShowQuests(true);
			Connect("draw", this, nameof(OnDraw));
		}
		public void OnDraw()
		{
			WorldQuest worldQuest;
			TreeItem section = entryTree.GetRoot().GetChildren(),
				questChild;

			while (section != null)
			{
				questChild = section.GetChildren();
				while (questChild != null)
				{
					worldQuest = questChild.GetMetadata(0) as WorldQuest;
					if (worldQuest != null)
					{
						questChild.SetCustomColor(0,
							worldQuest.status == QuestMaster.QuestStatus.COMPLETED
								? Color.ColorN("yellow")
								: new Color("#cccccc"));
					}
					questChild = questChild.GetNext();
				}
				section = section.GetNext();
			}
		}
		public void ShowQuests(bool current)
		{
			Globals.questMaster.GetAllPlayerQuests().ForEach(q =>
			{
				if ((current && q.status != QuestMaster.QuestStatus.DELIVERED)
				|| (!current && q.status == QuestMaster.QuestStatus.DELIVERED))
				{
					AddEntry(q);
				}
			});
		}
		public void AddEntry(WorldQuest worldQuest)
		{
			if (!isCurrentQuests && worldQuest.status != QuestMaster.QuestStatus.DELIVERED)
			{
				return;
			}

			string zoneName = new NodePath(worldQuest.quest.questGiverPath).GetName(1);
			TreeItem child = entryTree.GetRoot().GetChildren(),
				section = null,
				questChild;

			while (child != null)
			{
				if (child.GetText(0).Equals(zoneName))
				{
					section = child;
					break;
				}
				child = child.GetNext();
			}

			if (section == null)
			{
				section = entryTree.CreateItem(entryTree.GetRoot());
				section.SetText(0, zoneName);
			}

			questChild = entryTree.CreateItem(section);
			questChild.SetText(0, $"- {worldQuest.quest.questName}");
			questChild.SetMetadata(0, worldQuest);
		}
		private void OnTreeCellSelected()
		{
			TreeItem treeItem = entryTree.GetSelected();
			WorldQuest worldQuest = treeItem?.GetMetadata(0) as WorldQuest;

			treeItem?.Deselect(0);
			if (worldQuest != null)
			{
				OnEntrySelected(worldQuest);
			}
		}
		private void OnEntrySelected(WorldQuest worldQuest)
		{
			focusedWorldQuest = worldQuest;
			questTitle.Text = worldQuest.quest.questName;

			// show quest giver portrait and name
			NodePath questGiverPath = new NodePath(worldQuest.quest.questGiverPath);
			string zoneName = questGiverPath.GetName(1),
				characterEditorName = questGiverPath.GetName(questGiverPath.GetNameCount() - 1);

			if (Globals.unitDB.HasDataFromCache(zoneName, characterEditorName))
			{
				UnitDB.UnitData unitData = Globals.unitDB.GetDataFromCache(zoneName, characterEditorName);

				AtlasTexture atlasTexture = new AtlasTexture();
				Texture texture = GD.Load<Texture>(string.Format(PathManager.characterTexturePath, unitData.img));
				atlasTexture.Atlas = texture;
				atlasTexture.Region = new Rect2(Vector2.Zero,
					new Vector2(texture.GetWidth() / Globals.imageDB.GetData(unitData.img).total, texture.GetHeight()));

				questGiverName.Text = unitData.name;
				questGiverPortrait.Texture = atlasTexture;
			}
			else
			{
				questGiverName.Text = string.Empty;
				questGiverPortrait.Texture = null;
			}

			// show quest progress text
			string questProgess = string.Empty;
			int objectiveAmount;
			foreach (string objectiveName in worldQuest.objectives.Keys)
			{
				objectiveAmount = worldQuest.quest.objectives[objectiveName].amount;
				questProgess += string.Format("- {0}: {1}/{2}\n",
					objectiveName,
					Mathf.Clamp(worldQuest.objectives[objectiveName], 0, objectiveAmount),
					objectiveAmount);
			}
			questProgess = questProgess.Remove(questProgess.FindLast("\n"));
			questProgressDes.BbcodeText = questProgess;

			// show
			main.Hide();
			focusedEntry.Visible = questProgressDes.Visible = true;
		}
		private void OnSwitchQuestLogPressed()
		{
			isCurrentQuests = !isCurrentQuests;

			entryTree.Clear();
			entryTree.CreateItem();
			ShowQuests(isCurrentQuests);

			switchQuestLogButton.Text = isCurrentQuests
				? "Show Archived"
				: "Show Current";
		}
		private void OnShowDialoguePressed()
		{
			if (focusedWorldQuest == null)
			{
				return;
			}
			else if (dialogue != null)
			{
				OnDialogueExited();
				questProgressDes.Show();
				return;
			}

			questProgressDes.Hide();

			Globals.dialogic.ResetSaves();
			Globals.dialogic.SetVariable("questAvailable", "1");
			dialogue = Globals.dialogic.Start(focusedWorldQuest.quest.dialogue, false);
			dialogue.GetNode<Control>("TextBubble").SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;

			focusedEntry.AddChildBelowNode(questProgressDes, dialogue);

			dialogue.GetNode<RichTextLabel>("TextBubble/RichTextLabel").Set(
				"custom_colors/default_color", new Color("#cccccc"));

			dialogue.Connect("tree_exiting", this, nameof(OnDialogueExited));
		}
		private void OnDialogueExited()
		{
			dialogue?.Hide();
			dialogue?.QueueFree();
			ClearDialoguePtr();
			questProgressDes.Show();
		}
		private void ClearDialoguePtr() { dialogue = null; }
		private void OnQuestProgressdDesVisibilityChanged()
		{
			showDialogueButton.Text = questProgressDes.Visible
				? "Show Dialogue"
				: "Show Progress";
		}
		private void OnFocusedEntryBack()
		{
			PlaySound(NameDB.UI.CLICK3);
			focusedEntry.Hide();
			OnDialogueExited();
			main.Show();
		}
	}
}