using MelonLoader;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AstralPartyModManager.MelonLoader
{
    /// <summary>
    /// 星引擎Mod管理器 - 游戏内UI界面
    /// 使用Unity IMGUI系统创建可交互界面
    /// </summary>
    public class InPartyModUI
    {
        #region 配置

        private const float WindowWidth = 500f;
        private const float WindowHeight = 400f;
        private const string WindowTitle = "星引擎Mod管理器";
        private readonly KeyCode ToggleKey = KeyCode.F1;

        #endregion

        #region 状态

        private bool _isVisible = false;
        private bool _isDragging = false;
        private Vector2 _windowPosition;
        private Vector2 _dragOffset;
        private Vector2 _scrollPosition;
        private string _selectedMod = "";
        private bool _showCategories = false;
        private string _statusMessage = "";
        private float _statusMessageTime = 0f;

        #endregion

        #region 引用

        private readonly ModManagerPlugin _plugin;
        private GUIStyle? _windowStyle;
        private GUIStyle? _titleStyle;
        private GUIStyle? _labelStyle;
        private GUIStyle? _buttonStyle;
        private GUIStyle? _toggleStyle;
        private GUIStyle? _boxStyle;
        private GUIStyle? _scrollViewStyle;
        private GUIStyle? _modItemStyle;
        private GUIStyle? _modItemSelectedStyle;
        private GUIStyle? _categoryStyle;
        private GUIStyle? _statsStyle;
        private bool _stylesInitialized = false;

        #endregion

        #region 颜色定义

        private readonly UnityEngine.Color BackgroundColor = new UnityEngine.Color(0.15f, 0.15f, 0.18f, 0.95f);
        private readonly UnityEngine.Color TitleColor = new UnityEngine.Color(0.2f, 0.6f, 1f, 1f);
        private readonly UnityEngine.Color TextColor = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 1f);
        private readonly UnityEngine.Color TextDimColor = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 1f);
        private readonly UnityEngine.Color ButtonColor = new UnityEngine.Color(0.25f, 0.5f, 0.8f, 1f);
        private readonly UnityEngine.Color ButtonHoverColor = new UnityEngine.Color(0.35f, 0.6f, 0.9f, 1f);
        private readonly UnityEngine.Color EnabledColor = new UnityEngine.Color(0.3f, 0.8f, 0.4f, 1f);
        private readonly UnityEngine.Color DisabledColor = new UnityEngine.Color(0.8f, 0.3f, 0.3f, 1f);
        private readonly UnityEngine.Color SelectedColor = new UnityEngine.Color(0.3f, 0.4f, 0.6f, 0.8f);
        private readonly UnityEngine.Color CategoryColor = new UnityEngine.Color(0.25f, 0.25f, 0.3f, 0.9f);

        #endregion

        public InPartyModUI(ModManagerPlugin plugin)
        {
            _plugin = plugin;
            // 初始位置：屏幕中央偏右
            _windowPosition = new Vector2(Screen.width - WindowWidth - 20, Screen.height / 2 - WindowHeight / 2);
        }

        #region 公共方法

        /// <summary>
        /// 更新方法 - 在OnUpdate中调用
        /// </summary>
        public void OnUpdate()
        {
            // 检测热键
            if (Input.GetKeyDown(ToggleKey))
            {
                ToggleVisibility();
            }

            // 清除状态消息
            if (_statusMessageTime > 0 && Time.time > _statusMessageTime)
            {
                _statusMessage = "";
                _statusMessageTime = 0;
            }
        }

        /// <summary>
        /// GUI渲染方法 - 在OnGUI中调用
        /// </summary>
        public void OnGUI()
        {
            if (!_isVisible) return;

            InitializeStyles();
            DrawWindow();
        }

        /// <summary>
        /// 切换UI显示状态
        /// </summary>
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            MelonLogger.Msg(_isVisible ? "Mod管理器UI已打开 (按F1关闭)" : "Mod管理器UI已关闭 (按F1打开)");
        }

        /// <summary>
        /// 显示状态消息
        /// </summary>
        public void ShowStatusMessage(string message, float duration = 3f)
        {
            _statusMessage = message;
            _statusMessageTime = Time.time + duration;
            MelonLogger.Msg($"[UI] {message}");
        }

        #endregion

        #region 私有方法 - 样式初始化

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            // 窗口样式
            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.normal.background = MakeTexture(2, 2, BackgroundColor);
            _windowStyle.normal.textColor = TextColor;
            _windowStyle.border = new RectOffset(8, 8, 8, 8);
            _windowStyle.padding = new RectOffset(10, 10, 10, 10);

            // 标题样式
            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 18;
            _titleStyle.normal.textColor = TitleColor;

            // 标签样式
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 12;
            _labelStyle.normal.textColor = TextColor;

            // 按钮样式
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 12;
            _buttonStyle.normal.background = MakeTexture(2, 2, ButtonColor);
            _buttonStyle.normal.textColor = TextColor;
            _buttonStyle.hover.background = MakeTexture(2, 2, ButtonHoverColor);
            _buttonStyle.hover.textColor = Color.white;
            _buttonStyle.active.background = MakeTexture(2, 2, new Color(0.2f, 0.4f, 0.7f, 1f));
            _buttonStyle.border = new RectOffset(4, 4, 4, 4);
            _buttonStyle.padding = new RectOffset(10, 10, 5, 5);

            // 复选框样式
            _toggleStyle = new GUIStyle(GUI.skin.toggle);
            _toggleStyle.fontSize = 12;
            _toggleStyle.normal.textColor = TextColor;

            // 盒子样式
            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.22f, 0.9f));
            _boxStyle.border = new RectOffset(4, 4, 4, 4);
            _boxStyle.padding = new RectOffset(8, 8, 8, 8);

            // 滚动视图样式
            _scrollViewStyle = new GUIStyle(GUI.skin.scrollView);
            _scrollViewStyle.normal.background = MakeTexture(2, 2, new Color(0.12f, 0.12f, 0.14f, 0.9f));

            // Mod项样式（未选中）
            _modItemStyle = new GUIStyle(GUI.skin.button);
            _modItemStyle.fontSize = 12;
            _modItemStyle.normal.background = MakeTexture(2, 2, new Color(0.18f, 0.18f, 0.2f, 0.9f));
            _modItemStyle.normal.textColor = TextColor;
            _modItemStyle.hover.background = MakeTexture(2, 2, new Color(0.25f, 0.25f, 0.28f, 0.9f));
            _modItemStyle.border = new RectOffset(2, 2, 2, 2);
            _modItemStyle.padding = new RectOffset(8, 8, 6, 6);
            _modItemStyle.margin = new RectOffset(0, 0, 2, 2);

            // Mod项样式（选中）
            _modItemSelectedStyle = new GUIStyle(_modItemStyle);
            _modItemSelectedStyle.normal.background = MakeTexture(2, 2, SelectedColor);
            _modItemSelectedStyle.normal.textColor = Color.white;

            // 分类样式
            _categoryStyle = new GUIStyle(GUI.skin.box);
            _categoryStyle.normal.background = MakeTexture(2, 2, CategoryColor);
            _categoryStyle.border = new RectOffset(4, 4, 4, 4);
            _categoryStyle.padding = new RectOffset(10, 10, 8, 8);
            _categoryStyle.margin = new RectOffset(0, 0, 5, 5);

            // 统计样式
            _statsStyle = new GUIStyle(GUI.skin.label);
            _statsStyle.fontSize = 11;
            _statsStyle.normal.textColor = TextDimColor;

            _stylesInitialized = true;
        }

        private static Texture2D MakeTexture(int width, int height, UnityEngine.Color color)
        {
            UnityEngine.Color[] pixels = new UnityEngine.Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        #endregion

        #region 私有方法 - 窗口绘制

        private void DrawWindow()
        {
            // 处理拖拽
            HandleDragging();

            // 确保窗口在屏幕范围内
            ClampWindowPosition();

            // 绘制窗口背景
            Rect windowRect = new Rect(_windowPosition.x, _windowPosition.y, WindowWidth, WindowHeight);
            GUI.Box(windowRect, "", _windowStyle);

            // 绘制标题栏（可拖拽区域）
            Rect titleBarRect = new Rect(_windowPosition.x, _windowPosition.y, WindowWidth - 30, 30);
            GUI.DrawTexture(titleBarRect, MakeTexture(1, 1, new Color(0.1f, 0.3f, 0.5f, 0.8f)));

            // 检测标题栏点击开始拖拽
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && titleBarRect.Contains(currentEvent.mousePosition))
            {
                _isDragging = true;
                _dragOffset = currentEvent.mousePosition - _windowPosition;
                currentEvent.Use();
            }

            // 绘制标题
            GUI.Label(new Rect(_windowPosition.x, _windowPosition.y + 5, WindowWidth - 30, 25), WindowTitle, _titleStyle);

            // 绘制关闭按钮
            if (GUI.Button(new Rect(_windowPosition.x + WindowWidth - 28, _windowPosition.y + 2, 26, 26), "×", _buttonStyle))
            {
                _isVisible = false;
            }

            // 绘制内容区域
            Rect contentRect = new Rect(_windowPosition.x + 10, _windowPosition.y + 35, WindowWidth - 20, WindowHeight - 45);
            GUILayout.BeginArea(contentRect);
            DrawContent();
            GUILayout.EndArea();
        }

        private void HandleDragging()
        {
            Event currentEvent = Event.current;

            if (_isDragging)
            {
                if (currentEvent.type == EventType.MouseDrag)
                {
                    _windowPosition = currentEvent.mousePosition - _dragOffset;
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.MouseUp)
                {
                    _isDragging = false;
                    currentEvent.Use();
                }
            }
        }

        private void ClampWindowPosition()
        {
            _windowPosition.x = Mathf.Clamp(_windowPosition.x, 0, Screen.width - WindowWidth);
            _windowPosition.y = Mathf.Clamp(_windowPosition.y, 0, Screen.height - WindowHeight);
        }

        private void DrawContent()
        {
            // 顶部统计信息
            DrawStatistics();

            GUILayout.Space(10);

            // 操作按钮
            DrawActionButtons();

            GUILayout.Space(10);

            // Mod列表
            DrawModList();

            GUILayout.Space(10);

            // 状态消息
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                GUI.color = Color.yellow;
                GUILayout.Label(_statusMessage, _labelStyle);
                GUI.color = Color.white;
            }
        }

        private void DrawStatistics()
        {
            var stats = _plugin.GetStatistics();

            GUILayout.BeginHorizontal(_boxStyle);

            GUILayout.BeginVertical();
            GUILayout.Label($"已加载Mod: {stats.EnabledModCount}", _statsStyle);
            GUILayout.Label($"总资源数: {stats.TotalResources}", _statsStyle);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Label($"已替换: {stats.ReplacedCount}次", _statsStyle);
            GUILayout.Label($"可用Mod: {stats.AvailableModCount}", _statsStyle);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawActionButtons()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("刷新", _buttonStyle, GUILayout.Width(80), GUILayout.Height(28)))
            {
                _plugin.ReloadAllMods();
                ShowStatusMessage("已刷新所有Mod");
            }

            if (GUILayout.Button("全部启用", _buttonStyle, GUILayout.Width(90), GUILayout.Height(28)))
            {
                _plugin.EnableAllMods();
                ShowStatusMessage("已启用所有Mod");
            }

            if (GUILayout.Button("全部禁用", _buttonStyle, GUILayout.Width(90), GUILayout.Height(28)))
            {
                _plugin.DisableAllMods();
                ShowStatusMessage("已禁用所有Mod");
            }

            GUILayout.FlexibleSpace();

            // 分类管理开关
            _showCategories = GUILayout.Toggle(_showCategories, "分类管理", _toggleStyle, GUILayout.Width(90));

            GUILayout.EndHorizontal();
        }

        private void DrawModList()
        {
            var mods = _plugin.GetAllMods();

            if (mods.Count == 0)
            {
                GUILayout.Label("未发现任何Mod", _labelStyle);
                return;
            }

            // 列表标题
            GUILayout.Label($"Mod列表 ({mods.Count}个):", _labelStyle);

            // 滚动视图
            float listHeight = _showCategories && !string.IsNullOrEmpty(_selectedMod) ? 150 : 250;
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, _scrollViewStyle, GUILayout.Height(listHeight));

            foreach (var mod in mods)
            {
                DrawModItem(mod);
            }

            GUILayout.EndScrollView();

            // 如果选中Mod且开启分类管理，显示分类详情
            if (_showCategories && !string.IsNullOrEmpty(_selectedMod))
            {
                DrawCategoryDetails();
            }
        }

        private void DrawModItem(ModListItem mod)
        {
            bool isSelected = _selectedMod == mod.Name;
            var style = isSelected ? _modItemSelectedStyle : _modItemStyle;

            GUILayout.BeginHorizontal(style);

            // 启用/禁用复选框
            bool wasEnabled = mod.IsEnabled;
            bool isEnabled = GUILayout.Toggle(mod.IsEnabled, "", _toggleStyle, GUILayout.Width(20));

            if (isEnabled != wasEnabled)
            {
                if (isEnabled)
                {
                    _plugin.EnableMod(mod.Name);
                    ShowStatusMessage($"已启用: {mod.Name}");
                }
                else
                {
                    _plugin.DisableMod(mod.Name);
                    ShowStatusMessage($"已禁用: {mod.Name}");
                }
            }

            // 点击选择Mod
            GUILayout.Space(5);
            if (GUILayout.Button($"{mod.Name} v{mod.Version}", style, GUILayout.ExpandWidth(true)))
            {
                _selectedMod = isSelected ? "" : mod.Name;
            }

            // 状态指示器
            GUILayout.Space(5);
            GUI.color = mod.IsEnabled ? EnabledColor : DisabledColor;
            GUILayout.Label(mod.IsEnabled ? "●" : "○", _labelStyle, GUILayout.Width(15));
            GUI.color = Color.white;

            // 资源数量
            GUILayout.Label($"{mod.ResourceCount}", _statsStyle, GUILayout.Width(40));

            GUILayout.EndHorizontal();
        }

        private void DrawCategoryDetails()
        {
            var categories = _plugin.GetModCategories(_selectedMod);

            if (categories.Count == 0)
            {
                GUILayout.Label("该Mod没有分类配置", _labelStyle);
                return;
            }

            GUILayout.Space(5);
            GUILayout.Label($"{_selectedMod} - 分类管理:", _labelStyle);

            GUILayout.BeginVertical(_categoryStyle);

            foreach (var category in categories)
            {
                GUILayout.BeginHorizontal();

                bool wasEnabled = category.IsEnabled;
                bool isEnabled = GUILayout.Toggle(category.IsEnabled, category.Name, _toggleStyle);

                if (isEnabled != wasEnabled)
                {
                    _plugin.SetCategoryEnabled(_selectedMod, category.Name, isEnabled);
                    ShowStatusMessage($"分类 '{category.Name}' 已{(isEnabled ? "启用" : "禁用")}");
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label(category.Description, _statsStyle);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        #endregion
    }

    /// <summary>
    /// Mod列表项数据
    /// </summary>
    public class ModListItem
    {
        public string Name { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = "";
        public bool IsEnabled { get; set; } = false;
        public int ResourceCount { get; set; } = 0;
    }

    /// <summary>
    /// 分类信息
    /// </summary>
    public class CategoryInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 统计信息
    /// </summary>
    public class UIStatistics
    {
        public int EnabledModCount { get; set; } = 0;
        public int AvailableModCount { get; set; } = 0;
        public int TotalResources { get; set; } = 0;
        public int ReplacedCount { get; set; } = 0;
    }
}
