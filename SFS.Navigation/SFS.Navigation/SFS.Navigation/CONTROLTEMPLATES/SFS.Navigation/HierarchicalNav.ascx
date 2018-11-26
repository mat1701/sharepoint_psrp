<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HierarchicalNav.ascx.cs" Inherits="SFS.Navigation.CONTROLTEMPLATES.SFS.Navigation.HierarchicalNav" %>
<style type="text/css">
    .menu, .menu ul {
        margin: 0;
        padding: 0;
        list-style: none;
    }

        .menu li, .menu ul a {
            position: relative;
        }

        .menu > li {
            float: left;
        }

            .menu > li.floatr {
                float: right;
            }

        .menu li > a {
            display: block;
        }

        .menu ul {
            position: absolute;
            display: none;
            width: 170px;
        }

            .menu ul ul {
                top: 0;
                left: 170px;
            }

        .menu li:hover > ul {
            display: block;
        }

        .menu a {
            text-decoration: none;
        }

        .menu > li > a {
            color: #fff;
            /*font-weight: 400;
    font-size: 13px;*/
            line-height: 18px;
            padding: 6px 15px;
        }

        .menu > li:hover > a {
            background-color: #005198;
            color: #ffffff;
            border-left: none;
            padding-left: 15px;
            border-right: 0px solid #707070;
            margin: 0px 0 0 0px;
        }

    ul.menu li a {
        -webkit-transition: background-color 80ms ease-in-out;
        -moz-transition: background-color 80ms ease-in-out;
        -o-transition: background-color 80ms ease-in-out;
        -ms-transition: background-color 80ms ease-in-out;
        transition: background-color 80ms ease-in-out;
    }

    .menu ul li a {
        -webkit-transition: background-color 20ms ease-in-out, color 20ms ease-in-out;
        -moz-transition: background-color 20ms ease-in-out, color 20ms ease-in-out;
        -o-transition: background-color 20ms ease-in-out, color 20ms ease-in-out;
        -ms-transition: background-color 20ms ease-in-out, color 20ms ease-in-out;
        transition: background-color 20ms ease-in-out, color 20ms ease-in-out;
    }



    /* Sub Menu */
    .menu ul {
        background-color: #1BA1E2;
        border: 1px solid #e0e0e0;
        border-top: none;
        left: -1px;
        z-index: 999;
        border-radius: 0 0 2px 2px;
        -webkit-box-shadow: 0 1px 1px rgba(0,0,0,0.04);
        box-shadow: 0 1px 1px rgba(0,0,0,0.04);
    }

        .menu ul a {
            color: #ffffff;
            /*font-size: 12px;*/
            line-height: 15px;
            padding: 9px 12px;
            border-top: 0px solid #e6e6e6;
        }

            .menu ul a:hover {
                background-color: #005198;
                color: #fff;
            }
</style>
<div>
    <asp:Literal ID="ltMenu" runat="server"></asp:Literal>
</div>
