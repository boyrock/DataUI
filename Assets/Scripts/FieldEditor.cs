using UnityEngine;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

namespace DataUI
{
    public class FieldEditor
    {
        public enum FieldKindEnum { Int, Float, Bool, Vector2, Vector3, Vector4, Matrix, Color, Enum, String, Unknown, StringArr }
        public const BindingFlags BINDING = BindingFlags.Public | BindingFlags.Instance;

        //protected System.Object data;
        //public readonly List<BaseGUIField> GuiFields = new List<BaseGUIField>();
        public readonly Dictionary<string, GUIFieldGroup> GuiFieldsDic = new Dictionary<string, GUIFieldGroup>();

        public GUIStyle callapseToggleStyle { get; set; }

        //public event System.Action EventValueChanged;
        //FieldInfo[] fieldInfos;

        public enum FieldGroupType { Class, Array, None }
        public class GUIFieldGroup
        {
            public FieldGroupType type;
            public bool toggle;
            public List<BaseGUIField> GuiFields;
        }

        GUIContent gui = new GUIContent();
        public void OnGUI()
        {
            foreach (var key in GuiFieldsDic.Keys)
            {
                var guiGroup = GuiFieldsDic[key];
                var guiFields = guiGroup.GuiFields;

                var limit = 0;
                switch (guiGroup.type)
                {
                    case FieldGroupType.Class:

                        GUI.contentColor = Color.cyan;
                        GUILayout.Label(key);
                        GUI.contentColor = Color.white;
                        GUILayout.BeginVertical("Box");
                        limit = guiFields.Count;
                        for (var i = 0; i < limit; i++)
                            guiFields[i].OnGUI();
                        GUILayout.EndVertical();

                        break;
                    case FieldGroupType.Array:

                        GUI.contentColor = Color.green;

                        gui.text = key;
                        gui.image = callapseToggleStyle.normal.background;

                        GUILayout.BeginHorizontal();
                        guiGroup.toggle = GUILayout.Toggle(guiGroup.toggle, key, callapseToggleStyle, GUILayout.Width(200), GUILayout.Height(20));
                        GUILayout.EndHorizontal();

                        GUI.contentColor = Color.white;
                        if (guiGroup.toggle == true)
                        {
                            GUILayout.BeginVertical("Box");
                            limit = guiFields.Count;
                            for (var i = 0; i < limit; i++)
                                guiFields[i].OnGUI();
                            GUILayout.EndVertical();
                        }

                        break;
                    case FieldGroupType.None:
                        limit = guiFields.Count;
                        for (var i = 0; i < limit; i++)
                            guiFields[i].OnGUI();
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetSetting(object settingData)
        {
            if (settingData == null)
                return;

            SetupGuiField(settingData);
        }

        void AddGuiField(List<BaseGUIField> list, object item, FieldInfo field)
        {
            var nonSerializedAtt = field.GetCustomAttributes(typeof(NonSerializedAttribute), false);
            if(nonSerializedAtt.Length == 0)
                list.Add(GenerateGUI(item, field));
        }

        void SetupGuiField(object settingData, FieldGroupType type = FieldGroupType.None)
        {
            if (settingData.GetType().IsArray)
            {
                int count = 1;

                foreach (var item in settingData as IEnumerable)
                {
                    var fields = item.GetType().GetFields(BINDING);

                    GUIFieldGroup fieldGroup = new GUIFieldGroup();
                    fieldGroup.type = FieldGroupType.Array;
                    List<BaseGUIField> guiFields = new List<BaseGUIField>();

                    for (var i = 0; i < fields.Length; i++)
                    {
                        AddGuiField(guiFields, item, fields[i]);
                        //guiFields.Add(GenerateGUI(item, fields[i]));
                    }

                    fieldGroup.toggle = false;
                    fieldGroup.GuiFields = guiFields;

                    GuiFieldsDic[string.Format("{0}_{1}", item.GetType().Name, count++)] = fieldGroup;
                }
            }
            else
            {
                var fieldInfos = settingData.GetType().GetFields(BINDING);

                GUIFieldGroup fieldGroup = new GUIFieldGroup();
                fieldGroup.type = type;
                List<BaseGUIField> guiFields = new List<BaseGUIField>();

                for (var i = 0; i < fieldInfos.Length; i++)
                {
                    var field = fieldInfos[i];
                    var fieldType = field.FieldType;

                    if(fieldType.IsClass == true && fieldType != typeof(string))
                    {
                        var obj = field.GetValue(settingData);
                        SetupGuiField(obj, FieldGroupType.Class);
                    }
                    else
                    {
                        AddGuiField(guiFields, settingData, field);
                        //guiFields.Add(GenerateGUI(settingData, field));

                        fieldGroup.toggle = false;
                        fieldGroup.GuiFields = guiFields;
                    }
                }

                if(fieldGroup.GuiFields != null && fieldGroup.GuiFields.Count > 0)
                    GuiFieldsDic[settingData.GetType().Name] = fieldGroup;
            }
        }

        public void ValueChanged()
        {
            //if(EventValueChanged != null)
            //    EventValueChanged();
        }

        public virtual BaseGUIField GenerateGUI(object data, FieldInfo fi)
        {
            var fieldKind = EstimateFieldKind(fi);
            switch (fieldKind)
            {
                case FieldKindEnum.Int:
                    return new GUIInt(data, fi, ValueChanged);
                case FieldKindEnum.Float:
                    return new GUIFloat(data, fi, ValueChanged);
                case FieldKindEnum.Vector2:
                    return new GUIVector2(data, fi, 2, ValueChanged);
                case FieldKindEnum.Vector3:
                    return new GUIVector3(data, fi, 3, ValueChanged);
                case FieldKindEnum.Vector4:
                    return new GUIVector4(data, fi, 4, ValueChanged);
                case FieldKindEnum.Matrix:
                    return new GUIMatrix(data, fi, ValueChanged);
                case FieldKindEnum.Color:
                    return new GUIColor(data, fi, ValueChanged);
                case FieldKindEnum.Bool:
                    return new GUIBool(data, fi, ValueChanged);
                case FieldKindEnum.Enum:
                    return new GUIEnum(data, fi, ValueChanged);
                case FieldKindEnum.String:
                    return new GUIText(data, fi, ValueChanged);
                case FieldKindEnum.StringArr:
                    return new GUICheckbox(data, fi, ValueChanged);
                default:
                    return new GUIUnsupported(data, fi);
            }
        }

        public FieldKindEnum EstimateFieldKind(FieldInfo fi)
        {
            var fieldType = fi.FieldType;
            if (fieldType.IsPrimitive)
            {
                if (fieldType == typeof(int))
                    return FieldKindEnum.Int;
                if (fieldType == typeof(float))
                    return FieldKindEnum.Float;
                if (fieldType == typeof(bool))
                    return FieldKindEnum.Bool;
                return FieldKindEnum.Unknown;
            }
            if (fieldType.IsEnum)
                return FieldKindEnum.Enum;
            if (fieldType.IsValueType)
            {
                if (fieldType == typeof(Color))
                    return FieldKindEnum.Color;
                if (fieldType == typeof(Vector2))
                    return FieldKindEnum.Vector2;
                if (fieldType == typeof(Vector3))
                    return FieldKindEnum.Vector3;
                if (fieldType == typeof(Vector4))
                    return FieldKindEnum.Vector4;
                if (fieldType == typeof(Matrix4x4))
                    return FieldKindEnum.Matrix;
            }
            if (fieldType == typeof(string))
                return FieldKindEnum.String;

            if (fieldType == typeof(string[]))
                return FieldKindEnum.StringArr;

            return FieldKindEnum.Unknown;
        }

        public abstract class BaseGUIField
        {
            public readonly System.Object Data;
            public readonly FieldInfo Fi;

            protected System.Action _onGUI;

            public BaseGUIField(System.Object data, FieldInfo fi)
            {
                this.Data = data;
                this.Fi = fi;
            }

            public virtual void OnGUI()
            {
                _onGUI();
            }
            public abstract void Load();
            public abstract void Save();

        }

        public class GUIGroupBegin : BaseGUIField
        {
            public GUIGroupBegin(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                _onGUI = () =>
                {
                };
            }
            public override void Load() { }
            public override void Save() { }
        }

        public class GUIInt : BaseGUIField
        {
            public readonly TextInt TextInt;

            public GUIInt(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                TextInt = new TextInt((int)fi.GetValue(data));
                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));

                    var temp_value = TextInt.Value;
                    TextInt.StrValue = GUILayout.TextField(TextInt.StrValue, GUILayout.ExpandWidth(false), GUILayout.MinWidth(70f));
                    GUILayout.EndHorizontal();
                    Save();

                    if (temp_value != TextInt.Value)
                        valueChangedCallBack();
                };
            }

            public override void Load()
            {
                TextInt.Value = (int)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, TextInt.Value);
            }
        }
        public class GUIFloat : BaseGUIField
        {
            public readonly TextFloat TextFloat;

            public GUIFloat(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                TextFloat = new TextFloat((float)fi.GetValue(data));

                TextFloat backup = TextFloat;
                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));
                    var sliderAtt = fi.GetCustomAttributes(typeof(Slider), false);

                    var temp_value = TextFloat.Value;
                    if (sliderAtt.Length > 0)
                    {
                        var range_from = ((Slider)sliderAtt[0]).range_from;
                        var range_to = ((Slider)sliderAtt[0]).range_to;
                        GUILayout.BeginVertical(GUILayout.ExpandHeight(false), GUILayout.Height(30));
                        GUILayout.FlexibleSpace();
                        TextFloat.Value = GUILayout.HorizontalSlider(TextFloat.Value, range_from, range_to);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndVertical();
                        TextFloat.StrValue = GUILayout.TextField(TextFloat.StrValue, GUILayout.ExpandWidth(false), GUILayout.MinWidth(70f));
                    }
                    else
                    {
                        TextFloat.StrValue = GUILayout.TextField(TextFloat.StrValue, GUILayout.ExpandWidth(false), GUILayout.MinWidth(70f));
                    }

                    GUILayout.EndHorizontal();
                    Save();
                    if (temp_value != TextFloat.Value)
                        valueChangedCallBack();
                };
            }
            public override void Load()
            {
                TextFloat.Value = (float)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, TextFloat.Value);
            }
        }
        public abstract class BaseGUIVector : BaseGUIField
        {
            public readonly TextVector TextVector;

            public BaseGUIVector(System.Object data, FieldInfo fi, int dimention) : base(data, fi)
            {
                TextVector = GetTextVector(data, fi);
                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));
                    for (var i = 0; i < dimention; i++)
                        TextVector[i] = GUILayout.TextField(TextVector[i], GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
                    GUILayout.EndHorizontal();
                    Save();
                };
            }
            public abstract TextVector GetTextVector(System.Object data, FieldInfo fi);
        }
        public class GUIVector2 : BaseGUIVector
        {
            public GUIVector2(System.Object data, FieldInfo fi, int dimention, Action valueChangedCallBack) : base(data, fi, dimention) { }
            #region implemented abstract members of BaseGUIVector
            public override TextVector GetTextVector(object data, FieldInfo fi)
            {
                return new TextVector((Vector2)fi.GetValue(data));
            }
            public override void Load()
            {
                TextVector.Value = (Vector2)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, (Vector2)TextVector.Value);
            }
            #endregion
        }
        public class GUIVector3 : BaseGUIVector
        {
            public GUIVector3(System.Object data, FieldInfo fi, int dimention, Action valueChangedCallBack) : base(data, fi, dimention) { }
            #region implemented abstract members of BaseGUIVector
            public override TextVector GetTextVector(object data, FieldInfo fi)
            {
                return new TextVector((Vector3)fi.GetValue(data));
            }
            public override void Load()
            {
                TextVector.Value = (Vector3)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, (Vector3)TextVector.Value);
            }
            #endregion
        }
        public class GUIVector4 : BaseGUIVector
        {
            public GUIVector4(System.Object data, FieldInfo fi, int dimention, Action valueChangedCallBack) : base(data, fi, dimention) { }
            #region implemented abstract members of BaseGUIVector
            public override TextVector GetTextVector(object data, FieldInfo fi)
            {
                return new TextVector((Vector4)fi.GetValue(data));
            }
            public override void Load()
            {
                TextVector.Value = (Vector4)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, TextVector.Value);
            }
            #endregion
        }
        public class GUIColor : BaseGUIField
        {
            public readonly TextVector TextVector;

            public GUIColor(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                TextVector = new TextVector((Color)fi.GetValue(data));
                _onGUI = () =>
                {
                    Load();
                    var c = (Color)TextVector.Value;
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));
                    var prevColor = GUI.color;
                    GUI.color = new Color(c.r, c.g, c.b);
                    GUILayout.Label("■■■■■■", GUILayout.ExpandWidth(false));
                    GUI.color = new Color(c.a, c.a, c.a);
                    GUILayout.Label("■■", GUILayout.ExpandWidth(false));
                    GUI.color = prevColor;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    for (var i = 0; i < 4; i++)
                        TextVector[i] = GUILayout.TextField(TextVector[i], GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    Save();
                };
            }
            public override void Load()
            {
                TextVector.Value = (Color)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, (Color)TextVector.Value);
            }
        }
        public class GUIMatrix : BaseGUIField
        {
            public readonly TextMatrix TextMatrix;

            public GUIMatrix(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                TextMatrix = new TextMatrix((Matrix4x4)fi.GetValue(data));
                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));
                    GUILayout.BeginVertical();
                    for (var y = 0; y < 4; y++)
                    {
                        GUILayout.BeginHorizontal();
                        for (var x = 0; x < 4; x++)
                        {
                            TextMatrix[x + y * 4] = GUILayout.TextField(
                                TextMatrix[x + y * 4], GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    Save();
                };
            }
            public override void Load()
            {
                TextMatrix.Value = (Matrix4x4)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, (Matrix4x4)TextMatrix.Value);
            }
        }
        public class GUIBool : BaseGUIField
        {
            bool _toggle;

            public GUIBool(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                _toggle = (bool)fi.GetValue(data);
                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    _toggle = GUILayout.Toggle(_toggle, string.Format(" {0}", fi.Name));
                    GUILayout.EndHorizontal();
                    Save();
                };
            }
            public override void Load()
            {
                _toggle = (bool)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, _toggle);
            }
        }

        public class GUIEnum : BaseGUIField
        {
            public readonly TextInt TextInt;

            public class EnumItem
            {
                public bool toggle;
                public string name;
                public int index;
            }

            public GUIEnum(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                var enumType = fi.FieldType;
                var list = new StringBuilder();

                EnumItem[] enumItems = new EnumItem[System.Enum.GetValues(enumType).Length];

                string selectedItem = "";
                int index = 0;
                foreach (var selection in System.Enum.GetValues(enumType))
                {
                    EnumItem ei = new EnumItem();
                    ei.toggle = false;
                    ei.name = selection.ToString();
                    ei.index = (int)selection;
                    enumItems[index++] = ei;
                }
                    //list.AppendFormat("{0}({1}) ", selection, (int)selection);
                TextInt = new TextInt((int)fi.GetValue(data));
                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));
                    TextInt.StrValue = GUILayout.TextField(TextInt.StrValue, GUILayout.ExpandWidth(false), GUILayout.Width(50));

                    for (int i = 0; i < enumItems.Length; i++)
                    {
                        var text = string.Format("{0}_{1}", enumItems[i].name, enumItems[i].index);

                        if (enumItems[i].index == TextInt.Value)
                        {
                            GUI.contentColor = Color.red;
                            GUILayout.Toggle(true, text, GUILayout.ExpandWidth(false));
                            GUI.contentColor = Color.white;
                        }
                        else
                            GUILayout.Toggle(false, text, GUILayout.ExpandWidth(false));
                    }
                    GUILayout.EndHorizontal();
                    Save();
                };
            }
            public override void Load()
            {
                TextInt.Value = (int)Fi.GetValue(Data);
            }
            public override void Save()
            {
                Fi.SetValue(Data, GetEnumValue());
            }
            public System.Object GetEnumValue()
            {
                return System.Enum.ToObject(Fi.FieldType, TextInt.Value);
            }
        }
        public class GUIText : BaseGUIField
        {
            public string text;

            public GUIText(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));
                    var temp_value = text;
                    text = GUILayout.TextField(text, GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
                    GUILayout.EndHorizontal();
                    Save();
                    if (temp_value != text)
                        valueChangedCallBack();
                };
            }

            public override void Load()
            {
                string str = (string)Fi.GetValue(Data);
                text = str == null ? "" : str;
            }
            public override void Save()
            {
                Fi.SetValue(Data, text);
            }
        }
        public class GUIUnsupported : BaseGUIField
        {
            public GUIUnsupported(System.Object data, FieldInfo fi) : base(data, fi)
            {
                _onGUI = () =>
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("Unsupported Field : {0} of {1}", fi.Name, fi.FieldType.Name));
                    GUILayout.EndHorizontal();
                };
            }
            public override void Load() { }
            public override void Save() { }
        }

        public class GUICheckbox : BaseGUIField
        {
            public readonly TextInt TextInt;
            bool[] _toggles;
            string[] _items;
            string[] _selectedIndex;

            public GUICheckbox(System.Object data, FieldInfo fi, Action valueChangedCallBack) : base(data, fi)
            {
                var checkbox = (Checkbox)fi.GetCustomAttributes(typeof(Checkbox), false)[0];

                _items = checkbox.Items;
                _toggles = new bool[_items.Length];

                _onGUI = () =>
                {
                    Load();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} : ", fi.Name), GUILayout.ExpandWidth(false));
                    for (int i = 0; i < checkbox.Items.Length; i++)
                    {
                        _toggles[i] = GUILayout.Toggle(_toggles[i], checkbox.Items[i]);
                    }
                    GUILayout.EndHorizontal();
                    Save();
                };
            }

            public override void Load()
            {
                _selectedIndex = (string[])Fi.GetValue(Data);

                for (int i = 0; i < _items.Length; i++)
                {
                    for (int j = 0; j < _selectedIndex.Length; j++)
                    {
                        if (i.ToString().Equals(_selectedIndex[j]))
                        {
                            _toggles[i] = true;
                        }
                    }
                }
            }

            public override void Save()
            {
                var trueValues = _toggles.Where(t => t == true);
                _selectedIndex = new string[trueValues.Count()];

                int j = 0;
                for (int i = 0; i < _toggles.Length; i++)
                {
                    if (_toggles[i] == true)
                    {
                        _selectedIndex[j] = i.ToString();
                        j++;
                    }
                }

                Fi.SetValue(Data, _selectedIndex);
            }
        }
    }
}
