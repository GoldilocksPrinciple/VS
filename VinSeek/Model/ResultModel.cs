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

        private static ParseResult RetrieveResult(ResultCollection collection)
        {
            var totalResult = new ParseResult() { Name = "Data" };
            foreach (var result in collection)
            {
                ParseResult r1;
                if (result.Type == NodeType.Struct)
                    r1 = new ParseResult() { Name = result.Name.ToString()};
                else if (result.Value.ToString().Contains("System.Collection"))
                    r1 = new ParseResult() { Name = result.Name.ToString() };
                else
                    r1 = new ParseResult() { Name = result.Name.ToString(), TypeName = result.TypeName.ToString(), Value = result.Value.ToString() };

                _currentTab.Dispatcher.Invoke((Action)(() =>
                    {
                        _currentTab.HexBox.HighlightBytes(result.Position, result.Size, System.Drawing.Color.Black, TypeColours[result.Type.ToString().ToLower()]);
                    }));

                totalResult.Children.Add(r1);

                if (result.IsList)
                {
                    foreach (var resultCollection in result.ListChildren)
                    {
                        foreach (var re in resultCollection)
                        {
                            ParseResult r2;
                            if (re.Type == NodeType.Struct)
                                r2 = new ParseResult() { Name = re.Name.ToString() };
                            else
                                r2 = new ParseResult() { Name = re.Name.ToString(), TypeName = re.TypeName.ToString(), Value = re.Value.ToString() };
                            
                            r1.Children.Add(r2);

                            if (re.HasChildren)
                            {
                                var r3 = RetrieveResult(re.Children);
                                r2.Children.Add(r3);
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

        public IEnumerable GetChildren(object parent)
        {
            if (parent == null)
                parent = Root;
            return (parent as ParseResult).Children;

        }

        public bool HasChildren(object parent)
        {
            return (parent as ParseResult).Children.Count > 0;
        }
        
        public static readonly Dictionary<string, System.Drawing.Color> TypeColours = new Dictionary<string, System.Drawing.Color>
        {
            { "long", System.Drawing.Color.FromArgb(0xab, 0xc8, 0xf4) },
            { "varint", System.Drawing.Color.FromArgb(0xd7, 0x89, 0x8c) },
            { "byte", System.Drawing.Color.FromArgb(0x89, 0xd7, 0xb7) },
            { "int", System.Drawing.Color.Cyan },
            { "char", System.Drawing.Color.FromArgb(0x7b, 0xc8, 0xf4) },
            { "float", System.Drawing.Color.FromArgb(0x7f, 0xc0, 0xc0) },
            { "array", System.Drawing.Color.RosyBrown },
        };
    }
}
