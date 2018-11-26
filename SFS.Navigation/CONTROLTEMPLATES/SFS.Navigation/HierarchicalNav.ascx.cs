using Microsoft.SharePoint;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


// SharePoint Frontier Navigation Menu by Ashok Raja .T
// To get updated on latest happenings around SharePoint world , do visit my blog @ http://www.ashokraja.me
// Check out my free webparts for SharePoint 2013 and 2010 @ https://webpartgallery.codeplex.com/

namespace SFS.Navigation.CONTROLTEMPLATES.SFS.Navigation
{
    public partial class HierarchicalNav : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!this.Page.IsPostBack)
            {
                MenuHelper mnu = new MenuHelper("Top Menu");
                ltMenu.Text = mnu.RendMenuItems();
            }
            
        }
    }
}
