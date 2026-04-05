using UnityEngine;
using System;
using System.Collections.Generic;

public class Console : MonoBehaviour
{
    private readonly LineRenderer[] _lines = new LineRenderer[13];
    public static GUIStyle GUIStyle; 
    public static bool IsTyping = false;
    private string _input = "";
    private int _cursorPos = 0;
    private string _output = "";
    private bool _showInfo = false;
    private int _outputTimer = 0;
    private string[] _command;  

    private readonly List<string> _history = new ();
    private int _historyIndex = -1;
    
    private float _fps = 0.0f;
    private int _frameCount = 0;
    private float _elapsedTime = 0.0f;
    private void FPS()
    {
        _frameCount++;
        _elapsedTime += Time.unscaledDeltaTime;

        if (_elapsedTime >= 1.0f)
        {
            _fps = _frameCount / _elapsedTime;
            _frameCount = 0;
            _elapsedTime = 0.0f;
        }
    }
    void Start()
    {
        GUIStyle = new GUIStyle();
        GUIStyle.fontSize = 24;
        GUIStyle.font = Resources.Load<Font>("Font/OrangeKid/OrangeKid");
        GUIStyle.normal.textColor = Color.white;
        for (int i = 0; i < _lines.Length; i++)
        {
            GameObject edgeObj = new GameObject("WireEdge" + i);
            LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Helper.GetColor(101, 103, 155);
            lr.endColor = Helper.GetColor(101, 103, 155);
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.positionCount = 2;
            _lines[i] = lr;
        }

        HideLines();
    }
    private void HideLines()
    {
        foreach (LineRenderer line in _lines)
        {
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.zero);
        }
    }
    void Update()
    { 
        if (_showInfo) FPS();
        if (IsTyping)
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (_cursorPos > 0)
                    {
                        _input = _input.Remove(_cursorPos - 1, 1);
                        _cursorPos--;
                    }
                }
                else if (c == '\n' || c == '\r')
                {
                    ProcessInput();
                    IsTyping = false;
                }
                else
                {
                    _input = _input.Insert(_cursorPos, c.ToString());
                    _cursorPos++;
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _cursorPos = Mathf.Max(0, _cursorPos - 1);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _cursorPos = Mathf.Min(_input.Length, _cursorPos + 1);
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (_cursorPos < _input.Length)
                    _input = _input.Remove(_cursorPos, 1);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_history.Count > 0)
                {
                    if (_historyIndex == -1)
                        _historyIndex = _history.Count;
                    if (_historyIndex > 0)
                        _historyIndex--;
                    _input = _history[_historyIndex];
                    _cursorPos = _input.Length;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_history.Count > 0 && _historyIndex != -1)
                {
                    _historyIndex++;
                    if (_historyIndex >= _history.Count)
                    {
                        _historyIndex = -1;
                        _input = string.Empty;
                    }
                    else
                    {
                        _input = _history[_historyIndex];
                    }
                    _cursorPos = _input.Length;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                IsTyping = false;
                _input = string.Empty;
                _cursorPos = 0;
                _historyIndex = -1;
            }
        }
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            IsTyping = true;
            _input = "";
            _cursorPos = 0;
        }

        if (Input.GetKeyDown(KeyCode.F1)) _showInfo = !_showInfo;
    }

    void OnGUI()
    {
        if (IsTyping)
        {
            // show cursor in the input string
            string displayInput = _input.Insert(_cursorPos, "|");
            UnityEngine.GUI.Label(new Rect(10, 900, 1000, 30), displayInput + "\n"+ _output, GUIStyle);
        }
        else if (_outputTimer != 0)
        {
            _outputTimer--;
            UnityEngine.GUI.Label(new Rect(10, 900, 1000, 30), _output, GUIStyle);
        }

        if (_showInfo)
        {
            UnityEngine.GUI.Label(new Rect(10, 10, 100, 20), 
                "FPS: " + Mathf.Ceil(_fps) + "\nPosition: " + Main.PlayerInfo.position + "\nDay " + SaveData.Inst.day, GUIStyle);
        }
        
        if (!Main.BuildMode || !Main.Player) return;

        _lines[12].SetPosition(0, Main.PlayerInfo.position);
        _lines[12].SetPosition(1, Main.PlayerInfo.position + new Vector3(1, 0, 1));
        
        if (SetPiece.Pos1 == Vector3Int.zero || SetPiece.Pos2 == Vector3Int.zero) return;
        
        Vector3 min = Vector3.Min(SetPiece.Pos1, SetPiece.Pos2);
        Vector3 max = Vector3.Max(SetPiece.Pos1, SetPiece.Pos2) + Vector3Int.one;
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(max.x, min.y, max.z);
        corners[3] = new Vector3(min.x, min.y, max.z);  
        corners[4] = new Vector3(min.x, max.y, min.z);
        corners[5] = new Vector3(max.x, max.y, min.z);
        corners[6] = new Vector3(max.x, max.y, max.z);
        corners[7] = new Vector3(min.x, max.y, max.z);
        int[][] edgeIndices = new[] { new int[] {0,1}, new int[] {1,2}, new int[] {2,3}, new int[] {3,0}, new int[] {4,5}, new int[] {5,6}, new int[] {6,7}, new int[] {7,4}, new int[] {0,4}, new int[] {1,5}, new int[] {2,6}, new int[] {3,7} };
        for (int i = 0; i < _lines.Length - 1; i++)
        {
            _lines[i].SetPosition(0, corners[edgeIndices[i][0]]);
            _lines[i].SetPosition(1, corners[edgeIndices[i][1]]);
        }
    }
 
    private void ProcessInput()
    {
        // add to history
        if (!string.IsNullOrWhiteSpace(_input))
        {
            _history.Add(_input);
            _historyIndex = -1;
        }

        _command = _input.Split(' '); 
        Audio.PlaySFX(SfxID.Notification);
        switch (_command[0])
        {
            case "e":
            case "i":
                SpawnEntityOrItem();
                break; 
            case "tp":
                Teleport();
                break; 
            case "t":
                Gather();
                break; 
            case "time":
                if (_command.Length > 1 && int.TryParse(_command[1], out int units))
                    Environment.MoveTime(units * 60);
                break; 
            case "set":
                SetPieceCommands();
                break; 
            case "air":
                Output(NavMap.Get(Vector3Int.FloorToInt(Main.PlayerInfo.position))? "is air" : "not air");
                break;  
            case "fly":
                Main.Fly = !Main.Fly;
                break;   
            case "flat":
                Save.NewSave(GenType.SuperFlat);  
                GUIMain.GUIMenu.Show(false);
                break;   
            case "fps": 
                Application.targetFrameRate = int.Parse(_command[1]);
                break;  
            case "shake": 
                ViewPort.StartScreenShake(int.Parse(_command[1]), float.Parse(_command[2]));
                break;   
            case "save": 
                Output("unloaded");  
                Save.CloneSave(); 
                break;     
            default: 
                Output("Invalid command");
                break;  
        } 

        // reset input and cursor so next invocation starts fresh
        _input = string.Empty;
        _cursorPos = 0;
    }
  
    private void Output(string output)
    {
        _output = output + "\n" + _output;
        _outputTimer = 600;
    }
    
    private void SpawnEntityOrItem()
    {
        if (_command.Length < 2 || !Enum.TryParse(_command[1], out ID id)) return;
        
        int count;
        if (_command.Length < 3) count = 1;
        else if (!int.TryParse(_command[2], out count)) return;

        if (_command[0] == "i")
        {
            Main.PlayerInfo.Storage.CreateAndAddItem(id, count);
            Inventory.RefreshInventory(); 
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Entity.Spawn(id, Vector3Int.FloorToInt(Main.PlayerInfo.position));
            } 
        }
    }

    private void Teleport()
    {
        if (_command.Length < 4 ||
            !int.TryParse(_command[1], out int x) ||
            !int.TryParse(_command[2], out int y) ||
            !int.TryParse(_command[3], out int z)) return;
        
        Main.PlayerInfo.Machine.transform.position += new Vector3(x, y, z);
    }
    private void Gather()
    {
        foreach (PlayerInfo info in World.Inst.target)
        {
            info.position = Main.PlayerInfo.Machine.transform.position;
            if (info.Machine) info.Machine.transform.position = Main.PlayerInfo.Machine.transform.position;
        }
    } 

    private Chunk _chunk;
    private void SetPieceCommands()
    {
        if (_command.Length < 2)
        {
            Main.BuildMode = !Main.BuildMode;
            if (!Main.BuildMode) HideLines();
            Output("build mode: " + Main.BuildMode);
        }
        else if (_command[1] == "1")
        { 
            SetPiece.Pos1 = SetPosition(SetPiece.Pos1,  new Vector3Int(1, 0, 1));
            Output("set as corner 1" + SetPiece.Pos1); 
        }  
        else if (_command[1] == "2")
        { 
            SetPiece.Pos2 = SetPosition(SetPiece.Pos2,  new Vector3Int(-1, 0, -1));
            Output("set as corner 2:" + SetPiece.Pos2); 
        }  
        else if (_command[1] == "copy")
        {
            World.UnloadWorld();
            _chunk = SetPiece.Copy();   
            World.LoadWorld();
            Output("copied"); 
        } 
        else if (_command[1] == "save")
        {
            SetPiece.SaveSetPieceFile(_chunk, _command[2]);
            Output("saved as " + _command[2]);
        }
        else if (_command[1] == "paste")
        {
            if (_command.Length < 3) return;

            bool setCorners = _command.Length > 3 && _command[3] == "c";
            if (_command[2] != ".")
            {
                _chunk = SetPiece.LoadSetPieceFile(_command[2]);
                Output("loaded" + _command[2]);
            }

            Vector3Int pastePosition = Vector3Int.FloorToInt(Main.Player.transform.position) + new Vector3Int(1, 0, 1);
            SetPiece.Paste(pastePosition, _chunk, setCorners);

            World.UnloadWorld();
            World.LoadWorld();
            Output("pasted");
        }

        return;

        Vector3Int SetPosition(Vector3Int pos, Vector3Int offset)
        {
            if (_command.Length < 3)
            {
                pos = Vector3Int.FloorToInt(Main.Player.transform.position) + offset;
            }
            else if (!(!int.TryParse(_command[2], out int x) ||
                       !int.TryParse(_command[3], out int y) ||
                       !int.TryParse(_command[4], out int z))) {
                pos += new Vector3Int(x, y, z);
            } 
            return pos;
        }
    } 
}
