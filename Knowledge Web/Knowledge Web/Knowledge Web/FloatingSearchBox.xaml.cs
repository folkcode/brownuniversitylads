using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;

namespace Knowledge_Web
{
    public delegate void ItemSelectedEvent(object sender, ItemSelectedArgs e);

    public class ItemSelectedArgs : EventArgs
    {
        WebGroup SelectedGroup;

        internal ItemSelectedArgs(WebGroup grp, KnowledgeWeb w)
        {
            SelectedGroup = grp;
            w.addGroup(grp);
        }
    }

    public partial class FloatingSearchBox : UserControl
    {
        KnowledgeWeb w;
        Dictionary<String, List<WebGroup>> _searchKeywords = new Dictionary<string, List<WebGroup>>();  //Keywords to WebGroups
        HashSet<String> _keywords = new HashSet<String>(); // Keywords to search on currently
        private HashSet<String> _resultTitles = new HashSet<string>();
        public List<WebGroup> GroupList = new List<WebGroup>();

        public event ItemSelectedEvent ItemSelected;

        public FloatingSearchBox(List<WebGroup> allGroups, KnowledgeWeb newW)
        {
            InitializeComponent();
            w = newW;
            GroupList.AddRange(allGroups);

            foreach (WebGroup g in allGroups)
            {
                foreach (String k in g.Keywords())
                {
                    List<WebGroup> r = null;
                    if (!_searchKeywords.TryGetValue(k, out r))
                    {
                        r = new List<WebGroup>();
                        _searchKeywords.Add(k, r);
                    }

                    r.Add(g);
                }
            }
        }

        public void SearchOnGroup(WebGroup group) 
        {
            _keywords.Clear();
            keywordList.Items.Clear();

            foreach (String k in group.Keywords())
            {
                if (!_keywords.Contains(k))
                {
                    _keywords.Add(k);
                    keywordList.Items.Add(k);
                }
            }
        }

        public void SearchOnAllGroups()
        {
            _keywords.Clear();
            keywordList.Items.Clear();
            foreach (WebGroup g in GroupList)
            {
                foreach (String k in g.Keywords())
                {
                    if (!_keywords.Contains(k))
                    {
                        _keywords.Add(k);
                        keywordList.Items.Add(k);
                    }
                }
            }
        }

        private void textSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            String t = textSearch.Text;
            int l = t.Length;
            foreach (String s in _keywords)
            {
                int idx = keywordList.Items.IndexOf(s);
                if (t != "" && s.Substring(0, l) != t)
                {
                    // Remove
                    if (idx > -1)
                        keywordList.Items.RemoveAt(idx);
                }
                else
                {
                    // Add
                    if (idx == -1)
                        keywordList.Items.Add(s);
                }
            }
        }

        private void resultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultList.SelectedIndex > -1)
            {
                ItemSelectedArgs a = new ItemSelectedArgs((WebGroup)((StackPanel)ResultList.Items[ResultList.SelectedIndex]).Tag, w);
                ResultList.SelectedIndex = -1;

                if (ItemSelected != null)
                    ItemSelected(this, a);
                e.Handled = true;
            }
        }

        private void keywordList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResultList.Items.Clear();
            foreach (Object o in ResultList.Items)
            {
                StackPanel sp = (StackPanel)o;
                sp.Children.Clear();
            }
            _resultTitles.Clear();

            foreach (Object o in keywordList.SelectedItems)
            {
                String s = (String)o;
                List<WebGroup> grp = null;
                if (_searchKeywords.TryGetValue(s, out grp))
                {
                    foreach (WebGroup g in grp)
                    {
                        if (!_resultTitles.Contains(g.Title))
                        {
                            _resultTitles.Add(g.Title);
                            StackPanel sp = new StackPanel();
                            sp.Orientation = Orientation.Horizontal;
                            if (g.Thumbnail.Parent != null)
                                // TODO: THIS IS A HACK, FIX IT
                                ((StackPanel)g.Thumbnail.Parent).Children.Remove(g.Thumbnail);
                            sp.Children.Add(g.Thumbnail);
                            TextBlock tb = new TextBlock();
                            tb.Text = g.Title;
                            tb.Width = 190;
                            tb.TextWrapping = TextWrapping.Wrap;
                            sp.Children.Add(tb);
                            sp.Tag = g;

                            ResultList.Items.Add(sp);
                        }
                    }
                }
            }
        }
    }
}
