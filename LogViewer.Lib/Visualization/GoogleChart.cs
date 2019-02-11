using LogViewer.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib.Visualization
{
    public class GoogleChart
    {

        #region Declarations

        public string HTML { get; set; }

        private LogSetMetrics _metrics;
        const string _scriptKey = "<!-- *INSERT SCRIPT* -->";
        const string _htmlKey = "<!-- *INSERT HTML* -->";
        const string _resourceName = "LogViewer.Lib.Resources.HTML.Metrics.html";

        #endregion

        #region Constructors

        private GoogleChart() { }

        public GoogleChart(LogSetMetrics metrics)
        {
            _metrics = metrics;
            this.HTML = GetPageMarkup();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Creates the page markup, both html and javascrupt.
        /// </summary>
        private string GetPageMarkup()
        {
            var chartCount = 0;
            var markup = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            var rsrc = assembly.GetManifestResourceStream(_resourceName);
            using (var strm = new StreamReader(rsrc))
            {
                markup = strm.ReadToEnd();
            }

            //create scripts
            var script = new StringBuilder();
            script.Append("<script type='text/javascript' src='https://www.google.com/jsapi'></script>");
            script.Append("<script type='text/javascript'>");
            script.Append("google.load('visualization', '1.0', { 'packages': ['corechart'] });");
            script.Append("google.setOnLoadCallback(drawChart);");
            script.Append("function drawChart() {");
            if (_metrics.Levels.Count > 0)
            {
                script.Append(GetChartJS(_metrics.Levels, "Levels", ++chartCount, GoogleChartType.Bar));
            }
            if (_metrics.Categories.Count > 0)
            {
                script.Append(GetChartJS(_metrics.Categories, "Categories", ++chartCount, GoogleChartType.Pie));
            }
            if (_metrics.Processes.Count > 0)
            {
                script.Append(GetChartJS(_metrics.Processes, "Processes", ++chartCount, GoogleChartType.Pie));
            }
            if(_metrics.Areas.Count > 0)
            {
                script.Append(GetChartJS(_metrics.Areas, "Areas", ++chartCount, GoogleChartType.Pie));
            }
            script.Append("}");
            script.Append("</script>");
            markup = markup.Replace(_scriptKey, script.ToString());

            //create html
            var html = new StringBuilder();
            for (int i = 1; i < chartCount + 1; i++)
            {
                html.Append(string.Concat("<div id='chart_div", i, "'></div>"));
            }

           return markup.Replace(_htmlKey, html.ToString());
        }

        /// <summary>
        /// Returns the javascript needed to create the Google Chart
        /// </summary>
        /// <param name="area"></param>
        /// <param name="name"></param>
        /// <param name="index">should be unique to the page</param>
        /// <returns></returns>
        private string GetChartJS(Dictionary<string, int> area, string name, int index, GoogleChartType type)
        {
            var jsData = string.Concat("data", index);
            var script = new StringBuilder();
            script.Append(string.Concat("var ", jsData, " = new google.visualization.DataTable();"));
            script.Append(string.Concat(jsData, ".addColumn('string', '", name, "');"));
            script.Append(string.Concat(jsData, ".addColumn('number', 'Count');"));
            script.Append(string.Concat(jsData, ".addRows(["));

            foreach (var grp in area)
            {
                script.Append(string.Concat("['", grp.Key, "',", grp.Value, "],"));
            }
            script.Length = script.Length - 1;//delete last comma
            script.Append("]);");

            var defaults = string.Concat("title: '", name, "', width: 400, height: 300, fontSize:12, fontName: 'Segoe UI', colors: ['#008299', '#AC193D', '#D24726', '#008A00', '#006AC1']");
            if (type == GoogleChartType.Pie)
            {
                var opts = string.Concat("var options = { legend: { position: 'right' }, chartArea: { left:50, top:50, width: '100%', height: '100%'},", defaults, "};");
                script.Append(opts);
                script.Append(string.Concat("var chart = new google.visualization.PieChart(document.getElementById('chart_div", index, "'));"));
            }
            else if (type == GoogleChartType.Bar)
            {
                var opts = string.Concat("var options = { chartArea: { left:50, top:50, width: '90%', height: '90%'},  hAxis: { textPosition: 'in'},", defaults, "};");
                script.Append(opts);
                script.Append(string.Concat("var chart = new google.visualization.BarChart(document.getElementById('chart_div", index, "'));"));
            }

            script.Append(string.Concat("chart.draw(", jsData, ", options);"));
            return script.ToString();
        }

        #endregion
    }
}
