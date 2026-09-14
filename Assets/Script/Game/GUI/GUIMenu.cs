using System.Collections;
using System.Text;
using UnityEngine;

public partial class GUIMenu
{
    private enum MenuScreen { Main, Host, Pause, Settings, Keybinds, Keybind, Load, Confirm, Death }
    private enum ConfirmAction { None, Save, Load, Keybind }
    private MenuScreen _screen = MenuScreen.Main;
    private int _loadPage;
    private bool _transitioning;
    private ConfirmAction _confirmAction;
    private int _confirmIndex;
    private KeyCode _pendingKeybind;
    private MenuScreen _returnScreen;
    private MenuScreen _loadReturn = MenuScreen.Main;
    private MenuScreen _settingsReturn = MenuScreen.Pause;
    private CoroutineTask _scrollTask;
    private string _toast;
    private CoroutineTask _toastTask;
    public bool Showing { get; private set; }

    public void Show(bool isShow)
    {
        Showing = isShow;
        if (!isShow) _scrollTask?.Stop();
        if (Main.GUIMenu != null)
            Main.GUIMenu.gameObject.SetActive(isShow);
        if (isShow) Render();
    }

    public void ShowMain()
    {
        Showing = true;
        _transitioning = true;
        _scrollTask?.Stop();
        if (Main.GUIMenu != null)
            Main.GUIMenu.gameObject.SetActive(true);
        _ = new CoroutineTask(LoadingBeforeMain());
    }

    private IEnumerator LoadingBeforeMain()
    {
        yield return PlayLoading(0.75f);
        _transitioning = false;
        _screen = MenuScreen.Main;
        Render();
    }

    public void ShowPause()
    {
        _screen = MenuScreen.Pause;
        Show(true);
    }

    public void ShowDeath()
    {
        _screen = MenuScreen.Death;
        Show(true);
    }

    /// <summary>Hide the menu and clear any transition so a fresh new/loaded game
    /// starts with no menu on screen. Does NOT switch to the Main screen — the
    /// player is already entering a game.</summary>
    public void ResetToNeutral()
    {
        _transitioning = false;
        Show(false);
    }

    private void ShowSettings()
    {
        _screen = MenuScreen.Settings;
        Render();
    }

    private void ShowKeybinds()
    {
        _screen = MenuScreen.Keybinds;
        RenderNoScroll();
    }

    public void Update()
    {
        if (Main.GUIMenu == null || _transitioning) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Audio.PlaySFX(SfxID.Text);
            if (Showing && _screen == MenuScreen.Death) return; // death menu can't be dismissed
            if (Showing) Back();
            else ShowPause();
            return;
        }

        if (!Showing) return;

        if (_screen == MenuScreen.Keybind)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { Audio.PlaySFX(SfxID.Text); _rebinding = false; ShowKeybinds(); return; }
            if (TryGetPressedKey(out KeyCode key))
            {
                Audio.PlaySFX(SfxID.Text);
                _pendingKeybind = key;
                string conflict = FindKeybindConflict(key);
                if (conflict != null)
                {
                    _confirmAction = ConfirmAction.Keybind;
                    _returnScreen = MenuScreen.Keybind;
                    TransitionTo(MenuScreen.Confirm);
                }
                else
                {
                    ApplyKeybind(key);
                    _rebinding = false;
                    ShowKeybinds();
                }
            }
            return;
        }
        else if (_screen == MenuScreen.Keybinds && _rebinding)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { Audio.PlaySFX(SfxID.Text); _rebinding = false; Render(); return; }
            if (TryGetPressedKey(out KeyCode key))
            {
                Audio.PlaySFX(SfxID.Text);
                ApplyKeybind(key);
                _rebinding = false;
                Render();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) Select(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) Select(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) Select(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) Select(5);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) Select(6);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) Select(7);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) Select(8);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) Select(9);
        else if (Input.GetKeyDown(KeyCode.Alpha0)) Select(0);
    }

    private void Back()
    {
        switch (_screen)
        {
            case MenuScreen.Host:
                TransitionTo(MenuScreen.Main);
                break;
            case MenuScreen.Load:
                TransitionTo(_loadReturn);
                break;
            case MenuScreen.Pause:
                Show(false);
                break;
            case MenuScreen.Death:
                Show(false);
                break;
            case MenuScreen.Settings:
                TransitionTo(_settingsReturn);
                break;
            case MenuScreen.Keybinds:
                TransitionTo(MenuScreen.Settings);
                break;
            case MenuScreen.Keybind:
                _rebinding = false;
                ShowKeybinds();
                break;
            case MenuScreen.Confirm:
                TransitionTo(_returnScreen);
                break;
        }
    }

    private void Select(int n)
    {
        switch (_screen)
        {
            case MenuScreen.Main:
                if (n == 1) { Audio.PlaySFX(SfxID.Text); TransitionTo(MenuScreen.Host); }
                else if (n == 2) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(Server.StartClient()); Show(false); }
                else if (n == 3) { Audio.PlaySFX(SfxID.Text); _settingsReturn = MenuScreen.Main; ShowSettings(); }
                else if (n == 4) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(Quit()); }
                break;

            case MenuScreen.Host:
                if (n == 1) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(LoadingThen(() => { ScreenFade.FadeOut(0.3f); Save.Inst = new Save(GenType.Abyss); _ = new CoroutineTask(Server.StartHost()); ResetToNeutral(); })); }
                else if (n == 2) { Audio.PlaySFX(SfxID.Text); _loadReturn = MenuScreen.Host; _loadPage = 0; TransitionTo(MenuScreen.Load); }
                break;

            case MenuScreen.Pause:
                if (n == 1) { Audio.PlaySFX(SfxID.Text); _confirmAction = ConfirmAction.Save; _returnScreen = MenuScreen.Pause; TransitionTo(MenuScreen.Confirm); }
                else if (n == 2) { Audio.PlaySFX(SfxID.Text); _loadReturn = MenuScreen.Pause; _loadPage = 0; TransitionTo(MenuScreen.Load); }
                else if (n == 3) { Audio.PlaySFX(SfxID.Text); _settingsReturn = MenuScreen.Pause; ShowSettings(); }
                else if (n == 4) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(QuitToMenu()); }
                break;

            case MenuScreen.Death:
                if (n == 1) { Audio.PlaySFX(SfxID.Text); _loadReturn = MenuScreen.Death; _loadPage = 0; TransitionTo(MenuScreen.Load); }
                else if (n == 2) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(QuitToMenu()); }
                break;

            case MenuScreen.Settings:
                if (n >= 1 && n <= 9) { Audio.PlaySFX(SfxID.Text); CycleSetting(n - 1); }
                else if (n == 0) { Audio.PlaySFX(SfxID.Text); TransitionTo(MenuScreen.Keybinds); }
                break;

            case MenuScreen.Keybinds:
                HandleKeybindSelect(n);
                break;

            case MenuScreen.Load:
                if (n >= 1 && n <= 8)
                {
                    int index = _loadPage * 8 + (n - 1);
                    if (index < Saves.Inst.List.Count)
                    {
                        Audio.PlaySFX(SfxID.Text);
                        _confirmAction = ConfirmAction.Load;
                        _confirmIndex = index;
                        _returnScreen = MenuScreen.Load;
                        TransitionTo(MenuScreen.Confirm);
                    }
                }
                break;

            case MenuScreen.Confirm:
                if (n == 1) ConfirmExecute();
                else if (n == 2) { Audio.PlaySFX(SfxID.Text); TransitionTo(_returnScreen); }
                break;
        }
    }

    private void ConfirmExecute()
    {
        Audio.PlaySFX(SfxID.Text);
        if (_confirmAction == ConfirmAction.Save)
        {
            _ = new CoroutineTask(LoadingThen(() => { Saves.SaveGame(); _screen = MenuScreen.Pause; ShowToast("Game saved!"); }));
        }
        else if (_confirmAction == ConfirmAction.Load)
        {
            _ = new CoroutineTask(LoadingThen(() =>
            {
                ScreenFade.FadeOut(0.3f);
                Saves.LoadSave(Saves.Inst.List[_confirmIndex]);
                _ = new CoroutineTask(Server.StartHost());
                ResetToNeutral();
            }));
        }
        else if (_confirmAction == ConfirmAction.Keybind)
        {
            ApplyKeybind(_pendingKeybind);
            _rebinding = false;
            ShowKeybinds();
        }
    }

    private void Render()
    {
        Main.GUIMenu.text = BuildText() + (HasBack() ? "\n\nESC > Back" : "") + (_toast != null ? "\n\n" + _toast : "");
        _scrollTask?.Stop();
        float mult = Settings.ScrollSpeeds[Settings.Inst.ScrollSpeedIndex];
        int speed = Mathf.RoundToInt(1150 * mult);
        _scrollTask = TextScroller.HandleScroll(Main.GUIMenu, speed: speed);
    }

    private void RenderNoScroll()
    {
        _scrollTask?.Stop();
        Main.GUIMenu.text = BuildText() + (HasBack() ? "\n\nESC > Back" : "") + (_toast != null ? "\n\n" + _toast : "");
    }

    private bool HasBack()
    {
        return _screen switch
        {
            MenuScreen.Main => false,
            MenuScreen.Death => false,
            _ => true,
        };
    }

    private string BuildText()
    {
        return _screen switch
        {
            MenuScreen.Main => Header("MORIMORI") + "1 > Host\n2 > Join\n3 > Settings\n4 > Quit Game",
            MenuScreen.Host => Header("Host") + "1 > New\n2 > Load",
            MenuScreen.Pause => Header("Pause") + "1 > Save\n2 > Load\n3 > Settings\n4 > Quit to Menu",
            MenuScreen.Death => Header("Game Over") + "1 > Load\n2 > Main Menu",
            MenuScreen.Settings => Header("Settings") + RenderSettings(),
            MenuScreen.Keybinds => Header("Keybinds") + RenderKeybinds(),
            MenuScreen.Keybind => Header(KeybindList[_keybindIndex].Label) + RenderKeybind(),
            MenuScreen.Load => Header("Load") + RenderLoad(),
            MenuScreen.Confirm => Header("Confirm") + RenderConfirm(),
            _ => ""
        };
    }

    private static string Header(string name) => $"+-= =.·:·. {name} .·:·.= =-+\n";

    private void ShowToast(string message)
    {
        _toastTask?.Stop();
        _toast = message;
        _toastTask = new CoroutineTask(ClearToast());
        RenderNoScroll();
    }

    private IEnumerator ClearToast()
    {
        yield return new WaitForSeconds(1.5f);
        _toast = null;
        _toastTask = null;
        if (Showing) RenderNoScroll();
    }

    private void TransitionTo(MenuScreen target)
    {
        if (_transitioning) return;
        _transitioning = true;
        _scrollTask?.Stop();
        _ = new CoroutineTask(PlayLoading(0.3f, () =>
        {
            _screen = target;
            _transitioning = false;
            Render();
        }));
    }

    private const float LoadingFrameRate = 0.06f; // fixed animation speed

    private IEnumerator LoadingThen(System.Action done)
    {
        _transitioning = true;
        yield return PlayLoading(0.45f);
        _transitioning = false;
        done();
    }

    /// <summary>Plays the loading animation for a fixed duration at a constant
    /// frame rate, then invokes <paramref name="done"/> (if provided).</summary>
    private IEnumerator PlayLoading(float duration, System.Action done = null)
    {
        _scrollTask?.Stop(); // stop any stale scroll so old menu text isn't appended over the loading dots
        string[] frames = { ".", ". ·", ". · .", ". · . ·", ". · . · ." };
        int i = 0;
        float end = Time.time + duration;
        while (Time.time < end)
        {
            Main.GUIMenu.text = frames[i % frames.Length];
            i++;
            yield return new WaitForSeconds(LoadingFrameRate);
        }
        done?.Invoke();
    }
    private string RenderLoad()
    {
        if (Saves.Inst.List.Count == 0)
            return "No saves found";
        var sb = new StringBuilder();
        int start = _loadPage * 8;
        for (int i = 0; i < 8; i++)
        {
            int index = start + i;
            if (index >= Saves.Inst.List.Count) break;
            Save save = Saves.Inst.List[index];
            sb.Append($"{i + 1} > Day {save.day}, {Helper.FormatTime(save.time)} ({save.players.Count} players)\n");
        }
        return sb.ToString();
    }

    private string RenderConfirm()
    {
        if (_confirmAction == ConfirmAction.Save)
            return "Save the game?\n1 > Yes\n2 > No";
        if (_confirmAction == ConfirmAction.Load)
        {
            Save save = Saves.Inst.List[_confirmIndex];
            return $"Load Day {save.day}, {Helper.FormatTime(save.time)}?\n1 > Yes\n2 > No";
        }
        if (_confirmAction == ConfirmAction.Keybind)
        {
            var (label, _) = KeybindList[_keybindIndex];
            string conflict = FindKeybindConflict(_pendingKeybind);
            return $"\"{_pendingKeybind}\" is already bound to {conflict}.\nOverwrite {label}? \n1 > Yes\n2 > No";
        }
        return "";
    }

    private static IEnumerator Quit()
    {
        ScreenFade.FadeOut(0.5f);
        yield return new WaitForSeconds(0.6f);
        Application.Quit();
    }

    private IEnumerator QuitToMenu()
    {
        ScreenFade.FadeOut(0.5f);
        yield return new WaitForSeconds(0.6f);
        GUIMain.OnGameEnd();
        Main.SceneMode = SceneMode.Menu;
        // Return players to the pool while still hosting (they aren't tracked by
        // EntityDynamicLoad, so UnloadWorld alone won't despawn them).
        if (Save.Inst != null)
            foreach (PlayerInfo player in Save.Inst.players)
                if (player.Machine != null)
                {
                    ObjectPool.ReturnObject(player.Machine.gameObject);
                    player.Machine = null;
                }
        // Unload the world while still hosting so mobs/entities actually despawn
        // (EntityDynamicLoad.UnloadWorld bails if IsHost() is false).
        World.UnloadWorld();
        Server.StopHost();
        // Reset environment to the bright menu default so the screen isn't black.
        Environment.SetStartEnvironment(EnvironmentType.DaySnow);
        Environment.Target = EnvironmentType.DaySnow;
        ShowMain();
        ScreenFade.FadeIn(1f, 0f);
    }
}
