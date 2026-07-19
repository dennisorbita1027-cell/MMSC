<%@ Page Title="Statistician List" Language="C#" MasterPageFile="~/UserModule/HomeMaster.master" AutoEventWireup="true" CodeBehind="Statistician_List.aspx.cs" Inherits="CASApp1.UserModule.Statistician_list" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .container-stat-list {
            background-color: #fff;
            padding: 30px;
            border-radius: 25px;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            max-width: 800px;
            margin: 40px auto;
            box-sizing: border-box;
        }

        .stat-title {
            font-size: 32px;
            font-weight: bold;
            text-align: center;
            color: #2c3e50;
            margin-bottom: 30px;
        }

        .styled-gridview {
            width: 100%;
            border-collapse: collapse;
            font-size: 18px;
        }

            .styled-gridview th, .styled-gridview td {
                padding: 15px;
                text-align: left;
                border-bottom: 1px solid #ddd;
            }

            .styled-gridview tr:hover {
                background-color: #f2f2f2;
            }

            .styled-gridview th {
                background-color: #f8f9fa;
                color: black;
            }

        @media (max-width: 768px) {
            .container-stat-list {
                padding: 15px;
                margin: 20px 10px;
            }

            .stat-title {
                font-size: 24px;
                margin-bottom: 20px;
            }

            .styled-gridview {
                display: block;
                overflow-x: auto;
                -webkit-overflow-scrolling: touch;
                font-size: 14px;
                min-width: 600px; 
            }

                .styled-gridview th,
                .styled-gridview td {
                    padding: 10px 8px;
                }
        }
    </style>
</asp:Content>
<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-stat-list">
        <div class="stat-title">📋 List of Statisticians</div>

        <asp:GridView ID="gvStatisticians" runat="server" AutoGenerateColumns="false" CssClass="styled-gridview">
            <Columns>
                <asp:BoundField DataField="FullName" HeaderText="Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="Affiliation" HeaderText="Affiliation" />
                <asp:BoundField DataField="Specialization" HeaderText="Specialization" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
