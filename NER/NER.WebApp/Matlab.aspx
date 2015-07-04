<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Matlab.aspx.cs" Inherits="NER.WebApp.WebForm1" %>
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
            <asp:Button ID="Button1" runat="server" Text="Download Nes Sheet" CssClass="btn btn-primary "
                OnClick="Button1_Click" />
        </div>
        <div class="clearfix">
        </div>
    </div>
    <br />
    <br />
    <br />
    <br />
    <div class="container-fluid">
        <h4>
            Select File to export MatLab Sheet:</h4>
        <br />
        <div class="col-md-7">
            <asp:FileUpload CssClass="filestyle" ID="FileUpload2" runat="server" />
        </div>
        <div class="col-md-5">
            <asp:Button ID="Button2" runat="server" Text="Download Madamira Sheet" CssClass="btn btn-primary "
                OnClick="Button2_Click" />
        </div>
        <div class="clearfix">
        </div>
    </div>
</asp:Content>
