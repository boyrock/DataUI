using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using DataUI;
using System.IO;
using UnityEngine.Networking;
using System.Reflection;

namespace DataUI
{
    public class SettingManager : MonoBehaviour
    {
        public static NetworkClient networkClient { get; private set; }

        public void AddExtraGuiFunc(Action func) { extraGuiFunc += func; }

        public GUIStyle BoxStyle { get { return boxStyle; } }

        public KeyCode EditKey = KeyCode.E;

        List<Setting> settings = new List<Setting>();
        bool edit;
        Rect windowRect = new Rect(Screen.width / 2 - (Mathf.Min(Screen.width, 1024f) / 2f), 0, Mathf.Min(Screen.width, 1024f), Mathf.Min(Screen.height, Screen.height * 0.75f));
        Vector2 scroll;
        Action extraGuiFunc;

        GUIStyle boxStyle { get { if (_style == null) { _style = new GUIStyle("box"); } return _style; } }

        [SerializeField]
        GUIStyle _style;

        [SerializeField]
        GUIStyle connectToggleSkin;

        [SerializeField]
        GUIStyle callapseToggleSkin;

        private int selectionGridInt = 0;

        bool selfActive;
        public bool SelfActive
        {
            get
            {
                return selfActive;
            }
        }

        
        void Start() { }

        public void ReceiveMessageFromServer(NetworkMessage message)
        {
            SyncSettingMessage syncSettingMsg = message.ReadMessage<SyncSettingMessage>();

            if (syncSettingMsg != null)
            {
                //Update Settings
                SetSettingData(syncSettingMsg.text, syncSettingMsg.fileNmae);
            }
        }

        public void ReceiveMessageFromClient(NetworkMessage message)
        {
            SyncSettingMessage syncSettingMsg = message.ReadMessage<SyncSettingMessage>();

            if (syncSettingMsg != null)
            {
                //Update Settings
                SetSettingData(syncSettingMsg.text, syncSettingMsg.fileNmae);
                NetworkServer.SendToAll(MsgType.Highest, syncSettingMsg);
            }
        }

        public void SetNetworkClient(NetworkClient client)
        {
            networkClient = client;
        }

        void SetSettingData(string json, string fileName)
        {
            for (int i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];
                if (setting.fileName == fileName)
                {
                    var data = JsonUtility.FromJson(json, setting.GetType());
                    JsonUtils.SaveJsonFile(data, setting.filePath);
                    JsonUtils.LoadJsonFile(setting, setting.filePath);
                }
            }
        }

        public void HideGUI()
        {
            edit = false;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(EditKey))
            {
                edit = !edit;
            }
        }

        public void AddSettingMenu(Setting setting, string fileName, bool onSync = false)
        {
            FieldEditor editor = new FieldEditor();
            editor.SetSetting(setting);

            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            setting.Load(fileName, filePath);

            editor.callapseToggleStyle = callapseToggleSkin;

            setting.dataEditor = editor;
            setting.onSync = onSync;
            settings.Add(setting);
        }

        void OnGUI()
        {
            selfActive = false;

            if (!edit)
                return;

            selfActive = true;
            windowRect = GUI.Window(GetInstanceID(), windowRect, OnWindow, "Settings");
        }

        bool isSyncEnabled;

        void OnWindow(int id)
        {
            isSyncEnabled = false;

            if (NetworkClient.active == true && (networkClient != null && networkClient.isConnected == true))
                isSyncEnabled = true;
            else if (NetworkServer.active == true)
            {
                isSyncEnabled = false;
                for (int i = 0; i < NetworkServer.connections.Count; i++)
                {
                    var connection = NetworkServer.connections[i];
                    if (connection != null && connection.isConnected == true)
                    {
                        isSyncEnabled = true;
                        break;
                    }
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Toggle(isSyncEnabled, "", connectToggleSkin, GUILayout.Width(30), GUILayout.Height(30));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            var settingFileNames = settings.Select(s => s.fileName).ToArray();

            selectionGridInt = GUILayout.SelectionGrid(selectionGridInt, settingFileNames, settings.Count);

            scroll = GUILayout.BeginScrollView(scroll);

            for (int i = 0; i < settings.Count; i++)
            {
                if (i == selectionGridInt)
                {
                    settings[i].edit = true;
                }
                else
                {
                    settings[i].edit = false;
                }
            }

            settings.ForEach(setting =>
            {
                if (setting.edit)
                {
                    GUILayout.Space(5);

                    GUILayout.BeginVertical(boxStyle);
                    GUI.contentColor = Color.yellow;
                    GUILayout.Label(setting.filePath);
                    GUI.contentColor = Color.white;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(16f);

                    GUILayout.BeginVertical();
                    setting.OnGUIFunc();
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    GUILayout.Space(16);
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Save"))
                    {
                        setting.Save();
                    }

                    if (GUILayout.Button("Save and Close"))
                    {
                        setting.SaveAndClose();
                        Close();
                    }

                    GUI.enabled = isSyncEnabled;

                    if (GUILayout.Button("Sync"))
                    {
                        setting.Sync();
                    }

                    GUI.enabled = true;

                    if (GUILayout.Button("Cancle"))
                    {
                        setting.CancelAndClose();
                        Close();
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    GUILayout.Space(16);
                }
            });

            if (extraGuiFunc != null)
                extraGuiFunc.Invoke();

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        void Close()
        {
            this.edit = false;
        }

        void OnRenderObject()
        {
            settings.ForEach(setting =>
            {
                setting.OnRenderObjectFunc(Camera.current);
            });
        }

        [System.Serializable]
        public abstract class Setting
        {
            protected const BindingFlags BINDING = BindingFlags.Public | BindingFlags.Instance;

            object _data;
            public object data
            {
                get
                {
                    return this;
                }
                protected set
                {
                    _data = value;
                }
            }
            public FieldEditor dataEditor { get; set; }

            public string filePath { get; set; }
            public string fileName { get; set; }

            public bool onSync { get; set; }

            public bool edit { get; set; }

            public Setting()
            {
                var fields = this.GetType().GetFields(BINDING);

                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var fieldType = field.FieldType;

                    if (fieldType.IsArray == true)
                    {
                        Type elementType = fieldType.GetElementType();

                        int arrayLength = 1;

                        var arrayLengthAtt = field.GetCustomAttributes(typeof(ArrayLength), false);

                        if (arrayLengthAtt != null && arrayLengthAtt.Length > 0)
                            arrayLength = ((ArrayLength)arrayLengthAtt[0]).Length;

                        Array my1DArray = Array.CreateInstance(elementType, arrayLength);

                        for (int j = 0; j < my1DArray.Length; j++)
                        {
                            my1DArray.SetValue(Activator.CreateInstance(elementType), j);
                        }

                        field.SetValue(this, my1DArray);
                    }
                    else if (fieldType.IsClass == true)
                    {
                        field.SetValue(this, Activator.CreateInstance(fieldType));
                    }
                }
            }

            void LoadSettingFromFile(string fileName, string filePath)
            {
                this.fileName = fileName;
                this.filePath = filePath;

                JsonUtils.LoadJsonFile(data, filePath);
                OnLoad();
            }

            void LoadSettingFromFile()
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    JsonUtils.LoadJsonFile(data, filePath);
                    OnLoad();
                }
            }

            public virtual void Save()
            {
                JsonUtils.SaveJsonFile(data, filePath);
            }

            public virtual void Load(string fileName, string filePath)
            {
                LoadSettingFromFile(fileName, filePath);
            }

            public void SaveAndClose()
            {
                Save();
                OnClose();
            }

            public virtual void CancelAndClose()
            {
                LoadSettingFromFile();
                OnClose();
            }

            public void Sync()
            {
                SyncSettingMessage syncMsg = new SyncSettingMessage();
                syncMsg.text = this.ToString();
                syncMsg.fileNmae = fileName;

                if (NetworkClient.active && networkClient != null)
                {
                    if(networkClient.isConnected)
                    {
                        //クライアントでSyncボタンを押した場合は、まずはサーバに同期するメッセージを送る。
                        networkClient.Send(MsgType.Highest, syncMsg);
                    }
                    else
                    {
                        Debug.Log("<color=red>not connected to server</color>");   
                    }
                }

                if (NetworkServer.active)
                {
                    NetworkServer.SendToAll(MsgType.Highest, syncMsg);
                }
            }

            public virtual void OnGUIFunc()
            {
                dataEditor.OnGUI();
            }

            public override string ToString()
            {
                return JsonUtility.ToJson(data);
            }

            public virtual void OnRenderObjectFunc(Camera cam) { }

            protected virtual void OnLoad() { }

            protected virtual void OnClose() { }
        }
    }
    public class SyncSettingMessage : MessageBase
    {
        public string text;
        public string fileNmae;
    }
}
