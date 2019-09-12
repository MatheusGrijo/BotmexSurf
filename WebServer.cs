using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using BitBotBackToTheFuture;

public class WebServer
{
    

    public static string SendResponse(HttpListenerRequest request)
    {
        lock (MainClass.data)
        {
            try
            {
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(MainClass.location + "bd.xml");

                StringBuilder sb = new StringBuilder();


                sb.AppendLine(System.IO.File.ReadAllText(MainClass.location + "header.html") );


                double perc = 0;
                try { perc = ((double.Parse(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1][2].ToString()) * 100) / double.Parse(ds.Tables[0].Rows[0][2].ToString())) - 100; }
                catch { }
                sb.AppendLine("<div class='row'><div class='col-sm'>Status: <b>running</b><br/>");
                sb.AppendLine("Version: <b>" + MainClass.version + "</b><br/>");
                sb.AppendLine("Site: <b>" + MainClass.bitmexDomain + "</b><br/>");
                sb.AppendLine("Last update: <b>" + DateTime.Now.ToString() + "</b><br/>");                
                sb.AppendLine("Amount: <b>" + ds.Tables[1].Rows[1]["Value"].ToString() + "</b><br/>");
                sb.AppendLine("Open position: <b>" + MainClass.positionContracts.ToString() + "</b><br/>");                
                sb.AppendLine("Amount: <b>" + ds.Tables[1].Rows[1]["Value"].ToString() + "</b> (" + double.Parse(ds.Tables[1].Rows[1]["Value"].ToString()) / 100000000 + " BTC)<br/>");
                sb.AppendLine("<h3>Profit: <b>" + perc + "%</b><br/></h3>");                
                sb.AppendLine("Qty contracts: <b>" + MainClass.qtdyContacts.ToString() + "</b><br/>");

                try
                {

                    String graph = "";

                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        graph += "['" + ds.Tables[0].Rows[i][0].ToString() + "'," + ds.Tables[0].Rows[i][2].ToString() + "],";
                    }



                    graph = graph.Substring(0, graph.Length - 1);





                    sb.Append("<script>google.charts.load('current', {packages: ['corechart', 'line']}); " +
                                   " google.charts.setOnLoadCallback(drawBasic); " +

                    " function drawBasic() {" +


                        " var data = new google.visualization.DataTable();" +
                       " data.addColumn('string', 'Time');" +
                      "  data.addColumn('number', '" + "BTC" + "');" +

                       " data.addRows([ " +

                          graph +
              "]); " +

              "var options = {" +
               " hAxis: {" +
               "   title: 'Time'" +
               " }," +
               " vAxis: {" +
               "   title: '" + "BTC" + "'" +
               " }" +
              "};" +

                "var chart = new google.visualization.LineChart(document.getElementById('chart_div'));" +

                "chart.draw(data, options);" +
            "}; </script><div id='chart_div'></div>");
                }
                catch
                { }




                sb.AppendLine(System.IO.File.ReadAllText(MainClass.location + "footer.html"));

                sb.AppendLine("</body></html>");

                return sb.ToString();
            }
            catch (Exception ex)
            {

                return "";
    
            }
        }
   }

    private readonly HttpListener _listener = new HttpListener();
    private readonly Func<HttpListenerRequest, string> _responderMethod;

    public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
    {
        if (!HttpListener.IsSupported)
            throw new NotSupportedException(
                "Needs Windows XP SP2, Server 2003 or later.");

        // URI prefixes are required, for example 
        // "http://localhost:8080/index/".
        if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("prefixes");

        // A responder method is required
        if (method == null)
            throw new ArgumentException("method");

        foreach (string s in prefixes)
            _listener.Prefixes.Add(s);

        _responderMethod = method;
        _listener.Start();
    }

    public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
        : this(prefixes, method) { }

    public void Run()
    {
        ThreadPool.QueueUserWorkItem((o) =>
        {
            Console.WriteLine("Webserver running...");
            try
            {
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem((c) =>
                    {
                        var ctx = c as HttpListenerContext;
                        try
                        {
                            string rstr = _responderMethod(ctx.Request);
                            byte[] buf = Encoding.UTF8.GetBytes(rstr);
                            ctx.Response.ContentLength64 = buf.Length;
                            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        }
                        catch { } // suppress any exceptions
                        finally
                        {
                            // always close the stream
                            ctx.Response.OutputStream.Close();
                        }
                    }, _listener.GetContext());
                }
            }
            catch { } // suppress any exceptions
        });
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
    }
}
