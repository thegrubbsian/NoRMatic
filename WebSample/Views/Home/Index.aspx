<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" 
    Inherits="System.Web.Mvc.ViewPage<IEnumerable<WebSample.Models.Patient>>" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">
    Patient List
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h2>Patient List</h2>
    <% foreach (var patient in Model) { %>
    <%: patient.LastName %>, <%: patient.FirstName %>
    <% } %>
</asp:Content>
