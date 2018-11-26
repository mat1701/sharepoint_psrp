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
    public class MenuItem
    {
        public string Title { get; set; }
        public string Parent { get; set; }
        public string URL { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
