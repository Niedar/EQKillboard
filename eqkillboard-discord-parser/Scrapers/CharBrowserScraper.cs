using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;

namespace EQKillboard.DiscordParser.Scrapers {
    public class CharBrowserScraper {
        private const string _charBrowserUrl = "https://riseofzek.com/charbrowser/index.php?page=character&char=";
        private const string _npcSearchUrl = "https://riseofzek.com/alla/?a=advanced_npcs&isearch=Search&iname=";
        private IConfiguration _config;
        private string _characterName;
        private string _classLevelResult;
        private bool _isNpc;
        private bool _success;

        public CharBrowserScraper(string characterName) {
            _characterName = characterName;
            _config = Configuration.Default.WithDefaultLoader();
        }

        public async Task Fetch() 
        {
            var address = _charBrowserUrl + _characterName;
            var document = await BrowsingContext.New(_config).OpenAsync(address);
            var cellSelector = "div.InventoryStats table tbody tr:nth-child(5)";
            var cells = document.QuerySelector(cellSelector);
            
            string classLevel = string.Empty;
            if (cells != null)
            {
                classLevel = cells.TextContent;
            }
            _classLevelResult = classLevel;

            address = _npcSearchUrl + _characterName;
            document = await BrowsingContext.New(_config).OpenAsync(address);
            cellSelector = "div.page-content-ajax li a";
            var cellsAll = document.QuerySelectorAll(cellSelector);
            _isNpc = cellsAll.Any(x => x.TextContent.Equals(_characterName, StringComparison.InvariantCultureIgnoreCase));

            _success = true;
        }

        public int? Level
        {
            get
            {
                if (!string.IsNullOrEmpty(CharBrowserParser.ParseLevel(_classLevelResult)))
                {
                    return Convert.ToInt32(CharBrowserParser.ParseLevel(_classLevelResult));
                }
                return null;
            }
        }
        public string Class
        {
            get
            {
                return CharBrowserParser.ParseClass(_classLevelResult);
            }
        }
        public bool IsNpc
        {
            get
            {
                return _isNpc;
            }
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }
    }
}