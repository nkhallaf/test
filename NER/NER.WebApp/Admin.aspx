<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Admin.aspx.cs" Inherits="NER.WebApp.Admin" %>

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
            <div class="col-md-11">
                <h4>
                    Users</h4>
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
                            <%#((NER.BL.Users)Container.DataItem).Name%></span> <span class="col-md-3">
                                <asp:Button ID="Button1" CommandArgument="<%#((NER.BL.Users)Container.DataItem).ID%>"
                                    CommandName="Update" CssClass="btn btn-primary btn-xs" runat="server" Text="Update" />
                                <asp:Button ID="Button2" CommandArgument="<%#((NER.BL.Users)Container.DataItem).ID%>"
                                    CommandName="Delete" CssClass="btn btn-primary btn-xs" runat="server" Text="Delete" />
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
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">
                        &times;</button>
                    <h4 class="modal-title">
                        <asp:Label ID="LabelUserName" runat="server" Text="Label"></asp:Label>
                    </h4>
                </div>
                <asp:HiddenField ID="HiddenFieldIDD" runat="server" />
                <div class="modal-body">
                    <div class="form-group">
                        <label for="exampleInputEmail1">
                            Email address</label>
                        <input runat="server" type="text" class="form-control" id="InputEmail" placeholder="Enter email" />
                    </div>
                    <div class="form-group">
                        <label for="exampleInputPassword1">
                            Password</label>
                        <input runat="server" type="text" class="form-control" id="InputPassword" placeholder="Password" />
                    </div>
                    <div class="checkbox">
                        <label>
                            <asp:CheckBox ID="CheckBox1" runat="server" Text=" Is Admin" />
                        </label>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="Button3" CssClass="btn btn-primary btn-xs" runat="server" 
                        Text="Submit" onclick="Button3_Click" />
                    <button type="button" class="btn btn-default btn-xs" data-dismiss="modal">
                        Close</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
