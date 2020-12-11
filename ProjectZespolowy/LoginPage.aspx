<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginPage.aspx.cs" MasterPageFile="~/Site.Master" Inherits="ProjectZespolowy.LoginPage" %>



<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

          <style>
          .box{
                max-width: 50%;
                width: 50%;
                background: gray;
                left: 25%;
                max-height: 40%;
                height: 50%;
                position: absolute;
                top: 30%;
          }

          .input{
              max-width : 100%;
              width:90%;
              margin-left:5%;
          }

          .SubmitButton{
              max-width : 100%;
              width : 90%;
              margin-left : 5%;
          }
      </style>

            <div class="box">
                <div class="container" style="display: flex; flex-direction: column; justify-content: center; text-align: center;">
                      <div class="form-group" style="margin-top: 5%;">
                         <h1 style=" color:white; ">Login</h1>
                      </div>
                    <div class="form-group">
                        <asp:TextBox ID="inputUsername" CssClass="input form-control" runat="server"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:TextBox ID="inputPassword" CssClass="input form-control" type="password " runat="server"></asp:TextBox>
                    </div>
                    <asp:Button ID="SubmitButton" runat="server"  class="btn btn-primary SubmitButton" Text="Login" OnClick="SubmitButton_Click" />
                    <div  style="margin-left: 140px;">
                        <asp:Label  ID="ErrLabel" runat="server" Text=""></asp:Label>
                    </div>
                </div>
            </div>

</asp:Content>
    





<%--      </body>
    </html>--%>


