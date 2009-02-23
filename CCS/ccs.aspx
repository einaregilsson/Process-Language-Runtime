<%@ Page Language="C#" %>
<%@ Import Namespace="CCS" %>
<%@ Assembly Name="CCS" %>
<%@ Import Namespace="CCS.Parsing" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Text.RegularExpressions" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>CCS Interpreter</title>
    <style type="text/css">
        a, a:visited { text-decoration:none; color:white;}
        body { background-color:black; color:white; font-weight:bold; font-size:12pt;}
    </style>
</head>
<body>
    <%
        string code = "";
        if (Request.Form["code"] != null) {
            code = (string)Request.Form["code"];
        } else if (Session["code"] != null) {
            code = (string)Session["code"];
        }
         %>
    <form method="POST" action="ccs.aspx?iter=1#end">
    <textarea name="code" rows="18" cols="80"><%=code %></textarea><br />
    <input type="submit" value="Start system" />
    <script runat="server" type="text/C#">
        public void InterpretStep(int? choice) {
            Interpreter interpreter = (Interpreter)Session["interpreter"];
            string history = (string) (Session["history"] ?? "");
            Response.Write("<pre>");
            StringWriter sw = new StringWriter();
            interpreter.Iterate(sw, choice);
            int iter = 1;
            if (Request.QueryString["iter"] != null) {
                iter = int.Parse(Request.QueryString["iter"]);
            }
            iter++;
            Response.Write(@"<span style=""color:#bbb;"">" + history + Request.QueryString["choice"] + "</span>");
            string link = Regex.Replace(sw.ToString(), @"[^P]((\d+): .*?\n)", @"<a href=""?choice=$2&iter=" + iter + @"#end"">$1</a>");
            link = link.Replace("<sel>", "<span style=\"color:#00ff00;\">").Replace("</sel>", "</span>");
            Response.Write(link);
            history += Request.QueryString["choice"]+sw.ToString();
            Session["history"] = history;
            Response.Write("</pre>");
        }
    </script>
    <%
        if (Request.RequestType == "POST") {
            MemoryStream ms = new MemoryStream();
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(code);
            Session["code"] = code;
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            Parser p = new Parser(new Scanner(ms));
            p.errors.errorStream = Response.Output;
            Response.Write("<pre style=\"color:red\">");
            p.errors.errorStream = Response.Output;
            p.Parse();
            Response.Output.Flush();
            Response.Write("</pre>");
            if (p.errors.count==0) {
                Session.Add("interpreter", new Interpreter(p.System, true));
                InterpretStep(null);
            }
        } else if (Session["interpreter"] != null && Request.QueryString["choice"] != null) {
            InterpretStep(int.Parse(Request.QueryString["choice"]));
        }
    %>
    </form>
    <a name="end"></a>
</body>
</html>
