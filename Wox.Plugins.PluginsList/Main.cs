using System;
using System.Collections.Generic;
using Wox.Plugin;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Wox.Plugins.PluginsList
{
    class Main : IPlugin
    {
        private static PluginInitContext _context;
        private static readonly string PluginsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wox", "app-1.3.183", "Plugins");

        private static readonly string DownloadedPluginsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wox", "Plugins");

        private readonly List<PluginInfo> _plugins = new List<PluginInfo>();

        public List<Result> Query(Query query) => 
            _plugins.Where(x => x.Name.ToLower().StartsWith(query.Search.ToLower()) || 
                          (x.ActionKeyword != null && x.ActionKeyword.ToLower().StartsWith(query.Search.ToLower())))
            .Select(Bind)
            .ToList();

        public void Init(PluginInitContext context)
        {
            _context = context;
            LoadPluginInfo(PluginsPath);
            LoadPluginInfo(DownloadedPluginsPath);
        }

        private void LoadPluginInfo(string path)
        {
            string[] files = Directory.GetFiles(path, "plugin.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var data = JObject.Parse(File.ReadAllText(file));
                _plugins.Add(new PluginInfo
                {
                    Name = (string)data["Name"],
                    ActionKeyword = (string)data["ActionKeyword"],
                    Author = (string)data["Author"],
                    Description = (string)data["Description"],
                    Icon = (string)data["IcoPath"],
                    Version = (string)data["Version"],
                    Folder = Path.GetDirectoryName(file)
                });
            }
        }

        private Result Bind(PluginInfo x) => new Result
        {
            Title = x.ActionKeyword == "*" ? x.Name : $"{x.Name} ({x.ActionKeyword})",
            SubTitle = $"v{x.Version} | Author: {x.Author} | {x.Description}",
            IcoPath = x.Icon != null ? Path.Combine(x.Folder, x.Icon) : null,
            Action = ctx =>
            {
                _context.API.ChangeQuery(x.ActionKeyword, true);
                return false;
            }
        };
    }
}
