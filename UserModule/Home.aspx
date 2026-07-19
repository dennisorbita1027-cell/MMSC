<%@ Page Title="User Home" Language="C#" MasterPageFile="~/UserModule/HomeMaster.master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="CASApp1.UserModule.Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="center-container">
        <div class="button-list">
            <asp:Label ID="lblWelcome" runat="server" Text="Welcome!" Font-Size="Larger" />
        </div>
    </div>
</asp:Content>
