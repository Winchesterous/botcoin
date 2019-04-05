using BotCoin.BitmexScalper.Models;
using BotCoin.BitmexScalper.Models.State;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace BotCoin.BitmexScalper.Helpers
{
    internal class ApplicationState
    {
        private static string AppStateFilePath;
        private static string PositionWatcherFilePath;

        static ApplicationState()
        {
            PositionWatcherFilePath = ConfigurationManager.AppSettings["PositionWatcherFilePath"];
            AppStateFilePath = ConfigurationManager.AppSettings["AppStateFilePath"];
        }

        private static Dictionary<string, WindowStateData> GetWindowPositionData()
        {
            if (File.Exists(AppStateFilePath))
            {
                using (var fs = File.OpenText(AppStateFilePath))
                {
                    var content = fs.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<WindowStateData[]>(content);
                    return data.ToDictionary(k => k.WndName, v => v);
                }
            }
            return null;
        }

        public static void Load(System.Windows.Window wnd, Action<Dictionary<string, WindowStateData>, string> callback = null)
        {
            var wndPos = GetWindowPositionData();
            if (wndPos == null)
                return;

            var typeName = wnd.GetType().ToString();
            if (!wndPos.ContainsKey(typeName))
                return;

            var pos = wndPos[typeName];
         
            wnd.Left    = pos.Left;
            wnd.Top     = pos.Top;
            wnd.Width   = pos.Width;
            wnd.Height  = pos.Height;
            wnd.WindowState = pos.WndState;

            if (callback != null)
                callback(wndPos, typeName);
        }

        public static void Save(System.Windows.Window wnd, Action<Dictionary<string, WindowStateData>,string> callback = null)
        {
            var wndPos = GetWindowPositionData();
            var typeName = wnd.GetType().ToString();
            var data = new List<WindowStateData>();

            if (wndPos == null)
            {
                wndPos = new Dictionary<string, WindowStateData>();
                wndPos[typeName] = new WindowStateData();
            }
            else
            {
                if (!wndPos.ContainsKey(typeName))
                    wndPos[typeName] = new WindowStateData();
            }
            using (StreamWriter sw = File.CreateText(AppStateFilePath))
            {
                wndPos[typeName].WndName = typeName;
                wndPos[typeName].Top     = wnd.Top;
                wndPos[typeName].Left    = wnd.Left;
                wndPos[typeName].Width   = wnd.Width;
                wndPos[typeName].Height  = wnd.Height;
                wndPos[typeName].WndState   = wnd.WindowState;

                if (callback != null)
                    callback(wndPos, typeName);

                var json = JsonConvert.SerializeObject(wndPos.Select(d => d.Value).ToArray());
                sw.WriteLine(json);
            }
        }
    }
}
