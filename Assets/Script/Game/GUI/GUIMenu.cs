using System.Collections;
using System.Text;
using UnityEngine;

public class GUIMenu
{
    private enum MenuScreen { Main, Host, Pause, Settings, Load, Confirm }
    private enum ConfirmAction { None, Save, Load }
    private MenuScreen _screen = MenuScreen.Main;
    private int _loadPage;
    private bool _transitioning;
    private ConfirmAction _confirmAction;
    private int _confirmIndex;
    private MenuScreen _returnScreen;
    private MenuScreen _loadReturn = MenuScreen.Main;
    private CoroutineTask _scrollTask;
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
        _screen = MenuScreen.Main;
        Show(true);
    }

    public void ShowPause()
    {
        _screen = MenuScreen.Pause;
        Show(true);
    }

    public void Update()
    {
        if (Main.GUIMenu == null || _transitioning) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Audio.PlaySFX(SfxID.Text);
            if (Showing) Back();
            else ShowPause();
            return;
        }

        if (!Showing) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) Select(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) Select(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) Select(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) Select(5);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) Select(6);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) Select(7);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) Select(8);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) Select(9);
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
            case MenuScreen.Settings:
                TransitionTo(MenuScreen.Pause);
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
                else if (n == 3) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(Quit()); }
                break;

            case MenuScreen.Host:
                if (n == 1) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(LoadingThen(() => { ScreenFade.FadeOut(0.3f); Save.Inst = new Save(GenType.Abyss); _ = new CoroutineTask(Server.StartHost()); Show(false); })); }
                else if (n == 2) { Audio.PlaySFX(SfxID.Text); _loadReturn = MenuScreen.Host; _loadPage = 0; TransitionTo(MenuScreen.Load); }
                break;

            case MenuScreen.Pause:
                if (n == 1) { Audio.PlaySFX(SfxID.Text); _confirmAction = ConfirmAction.Save; _returnScreen = MenuScreen.Pause; TransitionTo(MenuScreen.Confirm); }
                else if (n == 2) { Audio.PlaySFX(SfxID.Text); _loadReturn = MenuScreen.Pause; _loadPage = 0; TransitionTo(MenuScreen.Load); }
                else if (n == 3) { Audio.PlaySFX(SfxID.Text); TransitionTo(MenuScreen.Settings); }
                else if (n == 4) { Audio.PlaySFX(SfxID.Text); _ = new CoroutineTask(QuitToMenu()); }
                break;

            case MenuScreen.Settings:
                if (n == 1) { Audio.PlaySFX(SfxID.Text); ToggleFullscreen(); Render(); }
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
            _ = new CoroutineTask(LoadingThen(() => { Saves.SaveGame(); Show(false); }));
        }
        else if (_confirmAction == ConfirmAction.Load)
        {
            _ = new CoroutineTask(LoadingThen(() =>
            {
                ScreenFade.FadeOut(0.3f);
                Saves.LoadSave(Saves.Inst.List[_confirmIndex]);
                _ = new CoroutineTask(Server.StartHost());
                Show(false);
            }));
        }
    }

    private void Render()
    {
        string text = _screen switch
        {
            MenuScreen.Main => Header("Main Menu") + "1 > Host\n2 > Join\n3 > Exit",
            MenuScreen.Host => Header("Host") + "1 > New\n2 > Load",
            MenuScreen.Pause => Header("Pause") + "1 > Save\n2 > Load\n3 > Settings\n4 > Quit to Menu",
            MenuScreen.Settings => Header("Settings") + "1 > Fullscreen: " + (Screen.fullScreen ? "On" : "Off"),
            MenuScreen.Load => Header("Load") + RenderLoad(),
            MenuScreen.Confirm => Header("Confirm") + RenderConfirm(),
            _ => ""
        };
        Main.GUIMenu.text = text + "\n\nESC > Back";
        _scrollTask?.Stop();
        _scrollTask = TextScroller.HandleScroll(Main.GUIMenu, speed: 230);
    }

    private static string Header(string name) => $"+-= ═.·:·. {name} .·:·.═ =-+\n";

    private void TransitionTo(MenuScreen target)
    {
        if (_transitioning) return;
        _transitioning = true;
        _scrollTask?.Stop();
        _ = new CoroutineTask(Transition(target));
    }

    private IEnumerator Transition(MenuScreen target)
    {
        string[] frames = { ".", ". ·", ". · .", ". · . ·", ". · . · ." };
        for (int i = 0; i < 3; i++)
            foreach (string frame in frames)
            {
                Main.GUIMenu.text = frame;
                yield return new WaitForSeconds(0.03f);
            }
        _screen = target;
        _transitioning = false;
        Render();
    }

    private IEnumerator LoadingThen(System.Action done)
    {
        string[] frames = { ".", ". ·", ". · .", ". · . ·", ". · . · ." };
        for (int i = 0; i < 3; i++)
            foreach (string frame in frames)
            {
                Main.GUIMenu.text = frame;
                yield return new WaitForSeconds(0.03f);
            }
        done();
    }
    private string RenderLoad()
    {
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
        return "";
    }

    private static void ToggleFullscreen()
    {
        if (Screen.fullScreen) Screen.SetResolution(960, 540, false);
        else Screen.SetResolution(1920, 1080, true);
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
        Server.StopHost();
        GUIMain.OnGameEnd();
        Main.SceneMode = SceneMode.Menu;
        _screen = MenuScreen.Main;
        Show(true);
        ScreenFade.FadeIn(1f, 1f);
    }
}
