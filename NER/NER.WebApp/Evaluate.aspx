<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Evaluate.aspx.cs" Inherits="NER.WebApp.Evaluate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h4>
            Select File to export MatLab Sheet:</h4>
        <br />
        <div class="col-md-7">
            <asp:FileUpload CssClass="filestyle" ID="FileUpload1" runat="server" />
        </div>
        <div class="col-md-5">
            <asp:Button ID="Button1" runat="server" Text="Download MatLab Sheet" CssClass="btn btn-primary "
                OnClick="Button1_Click" />
        </div>
        <div class="clearfix">
        </div>
    </div>
    <br />
    <br />
    <br />
    <br />
</asp:Content>
