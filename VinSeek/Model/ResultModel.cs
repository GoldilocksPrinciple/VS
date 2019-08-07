using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aga.Controls.Tree;
using System.Collections;
using BeeSchema;
using System.Diagnostics;
using VinSeek.Views;

namespace VinSeek.Model
{
    public class ResultModel : ITreeModel
    {
        private static VinSeekMainTab _currentTab;

        /// <summary>
        /// Create parse result model for TreeListView of current VinSeek tab
        /// </summary>
        /// <param name="currentTab">current VinSeek tab</param>
        /// <param name="collection">template parsing result</param>
        /// <returns></returns>
        public static ResultModel CreateModel(VinSeekMainTab currentTab, ResultCollection collection)
        {
            _currentTab = currentTab;
            var model = new ResultModel();
            var r1 = RetrieveResult(collection);
            Debug.WriteLine(r1);
            model.Root.Children.Add(r1);
            Debug.WriteLine("Done");
            return model;
        }

        /// <summary>
        /// Create TreeView structure for parse result
        /// </summary>
        /// <param name="collection">template parsing result</param>
        /// <returns></returns>
        private static ParseResult RetrieveResult(ResultCollection collection)
        {
            var totalResult = new ParseResult() { Name = "Data" };
            foreach (var result in collection)
            {
                ParseResult r1;
                if (result.Type == NodeType.Struct || result.Value.ToString().Contains("System.Collection") || result.Value.ToString().Contains("ResultCollection"))
                    r1 = new ParseResult() { Name = result.Name.ToString()};
                else
                    r1 = new ParseResult() { Name = result.Name.ToString(), TypeName = result.TypeName.ToString(), Value = result.Value.ToString() };

                _currentTab.Dispatcher.Invoke((Action)(() =>
                    {
                        Debug.WriteLine(result.Type.ToString());
                        _currentTab.HexBox.HighlightBytes(result.Position, result.Size, System.Drawing.Color.Black, TypeColours[result.Type.ToString().ToLower()]);
                    }));

                totalResult.Children.Add(r1);

                if (result.IsList)
                {
                    foreach (var resultCollection in result.ListChildren)
                    {
                        var r2 = new ParseResult() { Name = result.Name.ToString() };
                        r1.Children.Add(r2);
                        foreach (var re in resultCollection)
                        {
                            ParseResult r3;
                            if (re.Type == NodeType.Struct)
                                r3 = new ParseResult() { Name = re.Name.ToString() };
                            else
                                r3 = new ParseResult() { Name = re.Name.ToString(), TypeName = re.TypeName.ToString(), Value = re.Value.ToString() };
                            
                            r2.Children.Add(r3);

                            if (re.HasChildren)
                            {
                                var r4 = RetrieveResult(re.Children);
                                r3.Children.Add(r4);
                            }
                        }
                    }
                }
                else if (result.HasChildren)
                {
                    var r2 = RetrieveResult(result.Children);
                    r1.Children.Add(r2);
                }
            }
            return totalResult;
        }

        public ParseResult Root { get; private set; }

        public ResultModel()
        {
            Root = new ParseResult();
        }

        /// <summary>
        /// Get children of a node
        /// </summary>
        /// <param name="parent">parent node</param>
        /// <returns></returns>
        public IEnumerable GetChildren(object parent)
        {
            if (parent == null)
                parent = Root;
            return (parent as ParseResult).Children;

        }

        /// <summary>
        /// Check if object in TreeView has a children node
        /// </summary>
        /// <param name="parent">parent node</param>
        /// <returns></returns>
        public bool HasChildren(object parent)
        {
            return (parent as ParseResult).Children.Count > 0;
        }
        
        /// <summary>
        /// Color for each data type
        /// </summary>
        public static readonly Dictionary<string, System.Drawing.Color> TypeColours = new Dictionary<string, System.Drawing.Color>
        {
            { "byte", System.Drawing.Color.FromArgb(0x89, 0xd7, 0xb7) },
            { "bool", System.Drawing.Color.Yellow },
            { "int16", System.Drawing.Color.DarkBlue },
            { "int16be", System.Drawing.Color.DarkBlue },
            { "int", System.Drawing.Color.Cyan },
            { "intbe", System.Drawing.Color.Cyan },
            { "int64", System.Drawing.Color.FromArgb(0xab, 0xc8, 0xf4) },
            { "int64be", System.Drawing.Color.FromArgb(0xab, 0xc8, 0xf4) },
            { "varint", System.Drawing.Color.FromArgb(0xd7, 0x89, 0x8c) },
            { "char", System.Drawing.Color.FromArgb(0x7b, 0xc8, 0xf4) },
            { "float", System.Drawing.Color.FromArgb(0x7f, 0xc0, 0xc0) },
            { "array", System.Drawing.Color.RosyBrown },
        };
    }
}
