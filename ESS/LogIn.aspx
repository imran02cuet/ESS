<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LogIn.aspx.cs" Inherits="ESS.LogIn" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" language="javascript">
    function validate() {
        if (document.getElementById("<%=txtUserID.ClientID %>").value == "") {
            alert("Enter User ID");
            document.getElementById("<%=txtUserID.ClientID %>").focus();
            return false;
        }
        if (document.getElementById("<%=txtPassword.ClientID %>").value == "") {
            alert("Enter  Password");
            document.getElementById("<%=txtPassword.ClientID %>").focus();
            return false;
        }
        return true;
    }	             
    </script>

    <table>
    <tr>
        <td>User ID</td>
        <td>
            <asp:TextBox ID="txtUserID" Width="150px" runat="server"></asp:TextBox></td>
    </tr>
    <tr>
        <td>Password</td>
        <td>
            <asp:TextBox ID="txtPassword" TextMode="Password" Width="150px"  runat="server"></asp:TextBox></td>
    </tr>
    <tr>
        <td></td>
        <td>
            <asp:Button ID="btnLogIn" runat="server" Text="LogIn" OnClientClick="return validate();"
                onclick="btnLogIn_Click" /></td>
    </tr>
</table>
</asp:Content>
