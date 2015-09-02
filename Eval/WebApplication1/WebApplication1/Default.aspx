<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        <asp:FileUpload ID="FileUpload1" runat="server" CssClass="filestyle" />
    </h2>
    <h2>
        &nbsp;<asp:Button ID="Button1" runat="server" Height="59px" 
            onclick="Button1_Click" Text="CreatWordFile" Width="161px" />
    </h2>
    </asp:Content>
