using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// SharePoint Frontier Navigation Menu by Ashok Raja .T
// To get updated on latest happenings around SharePoint world , do visit my blog @ http://www.ashokraja.me
// Check out my free webparts for SharePoint 2013 and 2010 @ https://webpartgallery.codeplex.com/

namespace SFS.Navigation
{
   public class MenuHelper
    {
       private string _ListName = string.Empty;
       public MenuHelper(string ListName)
       {
           _ListName = ListName;
       }
       private List<MenuItem> LoadListItems()
       {
           List<MenuItem> MenuItems = new List<MenuItem>();
           try
           {
               SPList list = SPContext.Current.Site.RootWeb.Lists.TryGetList(_ListName);
               if (list != null)
               {
                   string caml = @"<Where><Eq><FieldRef Name='IsActive' /><Value Type='Boolean'>1</Value></Eq></Where><OrderBy><FieldRef Name='DisplayOrder' Ascending='True' /></OrderBy>";
                   SPQuery qry = new SPQuery();
                   qry.Query = caml;
                   SPListItemCollection items = list.GetItems(qry);
                   foreach (SPListItem item in items)
                   {
                       MenuItems.Add(new MenuItem { Title = item.Title, Parent = ParseLookUpItem(item["ParentLink"]), DisplayOrder = int.Parse(item["DisplayOrder"].ToString()),URL = item["URL"].ToString() });
                   }
               }
               return MenuItems;
           }
           catch
           {
               return null;
           }
       }

        public string RendMenuItems()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                List<MenuItem> AllItems = LoadListItems();

                sb.AppendLine(@"<ul id=""sfsMenu"" class=""menu"">");
                IEnumerable<MenuItem> ParentNodes = AllItems.Where(hd => hd.Parent == null || hd.Parent.Length == 0).OrderBy(h => h.DisplayOrder);
                foreach (MenuItem Node in ParentNodes)
                {
                    if (HasChildItems(AllItems, Node.Title))
                        RenderItems(sb, AllItems, Node);
                    else
                        sb.AppendLine(@"<li><a href=""" + Node.URL + @""">" + Node.Title + "</a></li>");
                }
                sb.AppendLine("</ul>");
                return sb.ToString();
            }
            catch  
            {
                return string.Empty;
            }
        }

        private void RenderItems(StringBuilder sb, List<MenuItem> AllItems, MenuItem Node)
        {
            var Items = AllItems.Where(h => h.Parent == Node.Title).OrderBy(q => q.DisplayOrder);
            if (Items != null && Items.Count() > 0)
            {
                sb.AppendLine("<li>");
                sb.AppendLine(@"<a href=""" + Node.URL + @""">" + Node.Title + "</a>");
                sb.AppendLine("<ul>");
                foreach (var item in Items)
                {
                    if (!HasChildItems(AllItems, item.Title))
                        sb.AppendLine(@" <li><a href=""" + item.URL + @""">" + item.Title + "</a></li>");
                    RenderItems(sb, AllItems, item);
                }
                sb.AppendLine("</ul>");
                sb.AppendLine("</li>");
            }
        }

        private void RenderSubItems()
        {

        }

        private bool HasChildItems(List<MenuItem> Items, string Title)
        {
            var Children = Items.Where(h => h.Parent == Title);
            return (Children == null || Children.Count() == 0) ? false : true;
        }

        private string ParseLookUpItem(object obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
            {
                return obj.ToString().Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            else
                return "";

        }
    }
}
