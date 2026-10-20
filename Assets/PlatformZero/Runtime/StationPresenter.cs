#if PLATFORM_ZERO_YARN
#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

namespace PlatformZero
{
    public sealed class StationPresenter : DialoguePresenterBase
    {
        private StationHud hud = null!;
        private StationGame game = null!;
        public void Initialize(StationHud ui, StationGame owner) { hud = ui; game = owner; }
        public override YarnTask OnDialogueStartedAsync() { hud.ShowDialogue(true); return YarnTask.CompletedTask; }
        public override YarnTask OnDialogueCompleteAsync() { hud.ShowDialogue(false); return YarnTask.CompletedTask; }
        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            hud.SetLine(line.CharacterName ?? "", line.TextWithoutCharacterName.Text);
            // Yield once so the input that opened dialogue never advances its first line.
            await YarnTask.Yield();
            float readyAt = Time.unscaledTime + .12f;
            while (!token.IsNextContentRequested && this != null && isActiveAndEnabled)
            {
                if (!game.Paused && Time.unscaledTime >= readyAt &&
                    (hud.ContinueRequested || Keyboard.current?.spaceKey.wasPressedThisFrame == true || Keyboard.current?.enterKey.wasPressedThisFrame == true)) break;
                hud.ContinueRequested = false;
                await YarnTask.Yield();
            }
            hud.ContinueRequested = false;
        }
        public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] options, LineCancellationToken token)
        {
            var available = new List<DialogueOption>();
            foreach (var option in options) if (option.IsAvailable) available.Add(option);
            if (available.Count == 0) return null;
            hud.SetChoices(available);
            await YarnTask.Yield();
            while (!token.IsNextContentRequested && this != null && isActiveAndEnabled)
            {
                if (!game.Paused)
                {
                    int selected = hud.SelectedChoice;
                    if (Keyboard.current?.digit1Key.wasPressedThisFrame == true) selected = 0;
                    if (Keyboard.current?.digit2Key.wasPressedThisFrame == true) selected = 1;
                    if (selected >= 0 && selected < available.Count) { hud.ClearChoices(); return available[selected]; }
                }
                await YarnTask.Yield();
            }
            hud.ClearChoices();
            return null;
        }
    }
}

#endif

