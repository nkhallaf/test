<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="NER.WebApp._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <title>ANER</title>
    <script type="text/javascript">
        function LoadDialog() {
            $("#myModal").modal('show');
        }
    </script>
    <style type="text/css">
        .upload
        {
            padding: 3px !important;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <div class="jumbotron title">
        <div class="row">
            <div class="col-md-6">
                <h4>
                    Uploaded Files</h4>
            </div>
            <div class="col-lg-5 upload">
                <asp:FileUpload ID="FileUpload1" CssClass="filestyle" runat="server" data-buttonText="Browse" />
            </div>
            <div class="col-lg-1 upload">
                <asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="~/assets/img/file_add.png"
                    OnClick="ImageButton1_Click" />
            </div>
        </div>
        <asp:Repeater ID="RepeaterFiles" runat="server" OnItemCommand="RepeaterFiles_ItemCommand">
            <HeaderTemplate>
                <ul class="list-group">
            </HeaderTemplate>
            <ItemTemplate>
                <li class="list-group-item">
                    <div class="row">
                        <span class="col-md-9"><span class="glyphicon glyphicon-file" aria-hidden="true"></span>
                            <%#((System.IO.FileInfo)Container.DataItem).Name%></span> 
                            <span class="col-md-3">
                                <asp:Button ID="Button1" CommandArgument="<%#((System.IO.FileInfo)Container.DataItem).FullName%>"
                                    CommandName="Display" CssClass="btn btn-primary btn-xs"
                                    runat="server" Text="Display" />
                                <asp:Button ID="Button2" CommandArgument="<%#((System.IO.FileInfo)Container.DataItem).FullName%>"
                                    CommandName="Download" CssClass="btn btn-primary btn-xs"
                                    runat="server" Text="Download" />
                            </span>
                    </div>
                </li>
            </ItemTemplate>
            <FooterTemplate>
                </ul>
            </FooterTemplate>
        </asp:Repeater>
    </div>
    <div id="myModal" class="modal fade">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">
                        &times;</button>
                    <h4 class="modal-title">
                        <asp:Label ID="LabelFileName" runat="server" Text="Label"></asp:Label>
                    </h4>
                </div>
                <div dir="rtl" class="modal-body">
                    <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default btn-xs" data-dismiss="modal">
                        Close</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
