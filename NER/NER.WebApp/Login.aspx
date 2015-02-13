<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="NER.WebApp.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="" />
    <meta name="author" content="" />
    <title>Login ANER</title>
    <!-- Bootstrap core CSS -->
    <link href="dist/css/bootstrap.min.css" rel="stylesheet">
    <!-- Custom styles for this template -->
    <link href="signin.css" rel="stylesheet">
    <!-- Just for debugging purposes. Don't actually copy these 2 lines! -->
    <!--[if lt IE 9]><script src="../../assets/js/ie8-responsive-file-warning.js"></script><![endif]-->
    <script src="assets/js/ie-emulation-modes-warning.js"></script>
    <!-- IE10 viewport hack for Surface/desktop Windows 8 bug -->
    <script src="assets/js/ie10-viewport-bug-workaround.js"></script>
    <!-- HTML5 shim and Respond.js IE8 support of HTML5 elements and media queries -->
    <!--[if lt IE 9]>
      <script src="https://oss.maxcdn.com/html5shiv/3.7.2/html5shiv.min.js"></script>
      <script src="https://oss.maxcdn.com/respond/1.4.2/respond.min.js"></script>
    <![endif]-->
</head>
<body class="container">
    <form id="form1" runat="server" class="form-signin" role="form">
    <h2 class="form-signin-heading">
        Please sign in</h2>
    <asp:TextBox ID="TextBoxEmail" type="email" runat="server" class="form-control" placeholder="Email address"
        required autofocus></asp:TextBox>
    <br />
    <asp:TextBox ID="TextBoxPassword" runat="server" type="password" class="form-control"
        placeholder="Password" required></asp:TextBox>
    <asp:Label ID="Label1" Visible="false" CssClass="label bg-danger label-danger" runat="server"
        Text="Wrong Email or Password !"></asp:Label>
    <br />
    <br />
    <asp:Button ID="ButtonLogin" runat="server" Text="Sign in" class="btn btn-lg btn-primary btn-block"
        type="submit" OnClick="ButtonLogin_Click" />
    <!-- /container -->
    <!-- Bootstrap core JavaScript
    ================================================== -->
    <!-- Placed at the end of the document so the pages load faster -->
    </form>
</body>
</html>
