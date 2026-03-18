using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AstralPartyModManager
{
    // Mod 状态管理器 - 跟踪和管理 Mod 的启用状态
    public class ModStateManager
    {
        private readonly string _stateFilePath;
        private Dictionary<string, bool> _modStates = new();

        public ModStateManager(string stateFilePath)
        {
            _stateFilePath = stateFilePath;
            _modStates = LoadStates();
        }

        // 获取 Mod 启用状态
        public bool IsEnabled(string modName)
        {
            return _modStates.TryGetValue(modName, out bool enabled) && enabled;
        }

        // 设置 Mod 启用状态
        public void SetEnabled(string modName, bool enabled)
        {
            _modStates[modName] = enabled;
            SaveStates();
        }

        // 获取所有启用的 Mod 名称
        public List<string> GetEnabledMods()
        {
            return _modStates.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }

        // 加载状态
        private Dictionary<string, bool> LoadStates()
        {
            if (File.Exists(_stateFilePath))
            {
                try
                {
                    var lines = File.ReadAllLines(_stateFilePath);
                    var states = new Dictionary<string, bool>();
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2 && bool.TryParse(parts[1], out bool enabled))
                        {
                            states[parts[0]] = enabled;
                        }
                    }
                    return states;
                }
                catch { }
            }

            return new Dictionary<string, bool>();
        }

        // 保存状态
        private void SaveStates()
        {
            try
            {
                var lines = _modStates.Select(kvp => $"{kvp.Key}={kvp.Value}");
                File.WriteAllLines(_stateFilePath, lines);
            }
            catch { }
        }
    }
}
