using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using OfficeOpenXml;
using Microsoft.Extensions.Configuration;

namespace app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly string _oltpConnectionString;
        private readonly string _olapConnectionString;

        public ReportsController(IConfiguration configuration)
        {
            _oltpConnectionString = configuration.GetConnectionString("OLTPConnection")!;
            _olapConnectionString = configuration.GetConnectionString("OLAPConnection")!;
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] string metric = "profit")
        {
            var query = BuildAnalyticsSql(metric);
            var results = new List<Dictionary<string, object?>>();

            using var connection = new OracleConnection(_olapConnectionString);
            
            try
            {
                await connection.OpenAsync();

                using var cmd = new OracleCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        
                        try
                        {
                            if (reader.IsDBNull(i))
                            {
                                row[name] = null;
                            }
                            else
                            {
                                // Convert everything to string to avoid type conversion issues
                                var value = reader.GetValue(i);
                                row[name] = value?.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            row[name] = $"Error: {ex.Message}";
                        }
                    }
                    results.Add(row);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Database error: {ex.Message}", query = query });
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    await connection.CloseAsync();
            }

            return Ok(results);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportAnalytics([FromQuery] string metric = "profit")
        {
            // EPPlus licensing
            ExcelPackage.License.SetNonCommercialPersonal("Eric");

            var query = BuildAnalyticsSql(metric);
            var table = new DataTable();

            // Create a new connection for each request
            using var connection = new OracleConnection(_olapConnectionString);
            
            try
            {
                await connection.OpenAsync();

                using var cmd = new OracleCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();
                table.Load(reader);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    await connection.CloseAsync();
            }

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Analytics");

            // headers
            for (int c = 0; c < table.Columns.Count; c++)
                ws.Cells[1, c + 1].Value = table.Columns[c].ColumnName;

            // rows
            for (int r = 0; r < table.Rows.Count; r++)
                for (int c = 0; c < table.Columns.Count; c++)
                    ws.Cells[r + 2, c + 1].Value = table.Rows[r][c];

            // simple chart: last column vs first column (if shape fits)
            if (table.Columns.Count >= 2 && table.Rows.Count > 0)
            {
                var chart = ws.Drawings.AddChart("chart1", OfficeOpenXml.Drawing.Chart.eChartType.ColumnClustered);
                chart.Title.Text = $"{metric} chart";
                chart.SetPosition(table.Rows.Count + 2, 0, 0, 0);
                chart.SetSize(800, 420);

                var values = ws.Cells[2, table.Columns.Count, table.Rows.Count + 1, table.Columns.Count];
                var cats = ws.Cells[2, 1, table.Rows.Count + 1, 1];
                chart.Series.Add(values, cats);
            }

            var fileName = $"{metric}-analytics.xlsx";
            var stream = new System.IO.MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        // Centralized SQL builder so analytics + export always stay in sync
        private static string BuildAnalyticsSql(string metric)
        {
            switch ((metric ?? "profit").ToLower())
            {
                case "collectionrate":
                    return @"
                        WITH Monthly AS (
                            SELECT
                                TRUNC(InvoiceDate, 'MM') AS MonthStart,
                                SUM(TotalAmount) AS TotalInvoiced,
                                SUM(CASE WHEN TRIM(IsPaid) = '1' THEN TotalAmount ELSE 0 END) AS TotalPaid,
                                COUNT(*) AS InvoiceCount,
                                SUM(CASE WHEN TRIM(IsPaid) = '1' THEN 1 ELSE 0 END) AS PaidCount
                            FROM FactInvoice
                            GROUP BY TRUNC(InvoiceDate, 'MM')
                        )
                        SELECT
                            TO_CHAR(MonthStart, 'YYYY-MM') AS Month,
                            InvoiceCount,
                            PaidCount,
                            TotalInvoiced,
                            TotalPaid,
                            TO_CHAR((TotalPaid * 100 / NULLIF(TotalInvoiced, 0)), 'FM9999999990.999999') AS MetricValue
                        FROM Monthly
                        ORDER BY Month
                        ";

                case "staffing":
                    return @"
                SELECT
                    TO_CHAR(sa.ScheduledDate, 'YYYY-MM') AS Month,
                    COUNT(sa.AssignedID) AS TotalScheduledServices,
                    COUNT(DISTINCT sh.ShiftID) AS TotalShifts,
                    (COUNT(sa.AssignedID) / CASE WHEN COUNT(DISTINCT sh.ShiftID)=0 THEN 1 ELSE COUNT(DISTINCT sh.ShiftID) END) AS ServicesPerShift
                FROM FactServiceAssignment sa
                LEFT JOIN FactShifts sh
                    ON TO_CHAR(sa.ScheduledDate,'YYYY-MM') = TO_CHAR(sh.StartTime,'YYYY-MM')
                GROUP BY TO_CHAR(sa.ScheduledDate, 'YYYY-MM')
                ORDER BY Month";

                case "damages":
                    return @"
                SELECT
                    a.AssetType,
                    a.Location,
                    COUNT(dr.ReportID) AS TotalReports,
                    NVL(AVG(dr.RepairCost), 0) AS AvgRepairCost,
                    NVL(SUM(dr.RepairCost), 0) AS MetricValue
                FROM DimAsset a
                LEFT JOIN FactDamageReport dr ON a.AssetID = dr.DimAssetID
                GROUP BY a.AssetType, a.Location
                HAVING COUNT(dr.ReportID) > 0 OR SUM(NVL(dr.RepairCost, 0)) > 0
                ORDER BY MetricValue DESC, TotalReports DESC";

                case "profit":
                default:
                    return @"
                WITH Revenue AS (
                    SELECT
                        s.ServiceID,
                        s.ServiceName,
                        SUM(i.TotalAmount) AS Revenue
                    FROM FactInvoice i
                    JOIN DimService s ON s.ServiceID = i.DimServiceID
                    GROUP BY s.ServiceID, s.ServiceName
                ),
                EmpPayroll AS (
                    SELECT
                        p.DimEmployeeID AS EmployeeID,
                        SUM(p.BaseSalary + p.OverTimePay - p.Deductions) AS PayrollCost
                    FROM FactPayroll p
                    GROUP BY p.DimEmployeeID
                ),
                EmpAssign AS (
                    SELECT
                        sa.DimEmployeeID AS EmployeeID,
                        sa.DimServiceID AS ServiceID,
                        COUNT(*) AS AssignCount
                    FROM FactServiceAssignment sa
                    GROUP BY sa.DimEmployeeID, sa.DimServiceID
                ),
                EmpAssignTotals AS (
                    SELECT
                        EmployeeID,
                        SUM(AssignCount) AS TotalAssigns
                    FROM EmpAssign
                    GROUP BY EmployeeID
                ),
                AllocatedCost AS (
                    SELECT
                        ea.ServiceID,
                        SUM( ep.PayrollCost * (ea.AssignCount / CASE WHEN eat.TotalAssigns = 0 THEN 1 ELSE eat.TotalAssigns END) ) AS AllocatedPayroll
                    FROM EmpAssign ea
                    JOIN EmpAssignTotals eat ON eat.EmployeeID = ea.EmployeeID
                    JOIN EmpPayroll ep ON ep.EmployeeID = ea.EmployeeID
                    GROUP BY ea.ServiceID
                )
                SELECT
                    r.ServiceName,
                    r.Revenue,
                    NVL(ac.AllocatedPayroll,0) AS AllocatedPayroll,
                    (r.Revenue - NVL(ac.AllocatedPayroll,0)) AS MetricValue
                FROM Revenue r
                LEFT JOIN AllocatedCost ac ON ac.ServiceID = r.ServiceID
                ORDER BY MetricValue DESC, r.ServiceName";

                case "payroll_rollup":
                return @"
                SELECT
                    e.Name AS Employee,
                    EXTRACT(YEAR FROM p.PayDate) AS PayYear,
                    SUM(p.NetPay) AS TotalNetPay
                FROM FactPayroll p
                JOIN DimEmployee e ON e.EmployeeID = p.DimEmployeeID
                GROUP BY ROLLUP (e.Name, EXTRACT(YEAR FROM p.PayDate))
                ORDER BY e.Name NULLS LAST, PayYear NULLS LAST";

                case "invoice_cube":
                return @"
                SELECT
                    COALESCE(c.Name, 'ALL CLIENTS') AS Client,
                    COALESCE(s.ServiceName, 'ALL SERVICES') AS Service,
                    COALESCE(f.IsPaid, 'ALL') AS IsPaid,
                    SUM(f.TotalAmount) AS Revenue
                FROM FactInvoice f
                JOIN DimClient c ON c.ClientID = f.DimClientID
                JOIN DimService s ON s.ServiceID = f.DimServiceID
                GROUP BY CUBE (c.Name, s.ServiceName, f.IsPaid)
                ORDER BY Client, Service, IsPaid";

                case "invoice_grouping":
                return @"
                SELECT
                    CASE WHEN GROUPING(c.Name) = 1 THEN 'ALL CLIENTS' ELSE c.Name END AS Client,
                    CASE WHEN GROUPING(s.ServiceName) = 1 THEN 'ALL SERVICES' ELSE s.ServiceName END AS Service,
                    SUM(f.TotalAmount) AS Revenue,
                    GROUPING_ID(c.Name, s.ServiceName) AS GroupingID,
                    GROUP_ID() As GroupID
                FROM FactInvoice f
                JOIN DimClient c ON c.ClientID = f.DimClientID
                JOIN DimService s ON s.ServiceID = f.DimServiceID
                GROUP BY GROUPING SETS ((c.Name, s.ServiceName), (c.Name), (s.ServiceName), ())
                ORDER BY GroupingID, Client NULLS LAST, Service NULLS LAST";

                case "shift_groupingsets":
                return @"
                SELECT
                    e.Name AS Employee,
                    s.IsOnCall,
                    SUM((s.EndTime - s.StartTime) * 24) AS HoursWorked
                FROM FactShifts s
                JOIN DimEmployee e ON e.EmployeeID = s.DimEmployeeID
                GROUP BY GROUPING SETS (
                    (e.Name, s.IsOnCall),   -- detailed
                    (e.Name),               -- subtotal per employee
                    (s.IsOnCall),           -- subtotal per on-call status
                    ()
                )
                ORDER BY Employee NULLS LAST, IsOnCall NULLS LAST";
            }
        }

        [HttpGet("debug/olap-contents")]
        public async Task<IActionResult> DebugOlapContents()
        {
            var results = new Dictionary<string, object>();
            
            using var connection = new OracleConnection(_olapConnectionString);
            await connection.OpenAsync();

            // Check table counts
            var tables = new[] { 
                "DimEmployee", "DimClient", "DimService", "DimAsset", "DimRenter",
                "FactPayroll", "FactShifts", "FactServiceAssignment", "FactInvoice", 
                "FactServiceRegistration", "FactDamageReport", "FactRentalHistory", "FactAttendance"
            };
            
            var tableCounts = new Dictionary<string, int>();
            foreach (var table in tables)
            {
                try
                {
                    using var cmd = new OracleCommand($"SELECT COUNT(*) FROM {table}", connection);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    tableCounts[table] = count;
                }
                catch (Exception ex)
                {
                    tableCounts[table] = -1;
                }
            }
            results["TableCounts"] = tableCounts;

            // Sample some data from key tables
            var sampleData = new Dictionary<string, List<Dictionary<string, object>>>();
            
            // Sample FactInvoice data
            try
            {
                using var cmd = new OracleCommand("SELECT * FROM FactInvoice WHERE ROWNUM <= 5", connection);
                using var reader = await cmd.ExecuteReaderAsync();
                var invoices = new List<Dictionary<string, object>>();
                
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    invoices.Add(row);
                }
                sampleData["FactInvoice"] = invoices;
            }
            catch (Exception ex)
            {
                sampleData["FactInvoice"] = new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", ex.Message } } };
            }

            // Sample DimService data
            try
            {
                using var cmd = new OracleCommand("SELECT * FROM DimService WHERE ROWNUM <= 5", connection);
                using var reader = await cmd.ExecuteReaderAsync();
                var services = new List<Dictionary<string, object>>();
                
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    services.Add(row);
                }
                sampleData["DimService"] = services;
            }
            catch (Exception ex)
            {
                sampleData["DimService"] = new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", ex.Message } } };
            }

            results["SampleData"] = sampleData;

            return Ok(results);
        }

        [HttpGet("debug/oltp-contents")]
        public async Task<IActionResult> DebugOltpContents()
        {
            var results = new Dictionary<string, object>();
            
            using var connection = new OracleConnection(_oltpConnectionString);
            await connection.OpenAsync();

            // Check table counts in OLTP
            var tables = new[] { "Employee", "Client", "ServiceType", "Asset", "Renter", "Payment", "Shift", "Service", "AssetRent" };
            
            var tableCounts = new Dictionary<string, int>();
            foreach (var table in tables)
            {
                try
                {
                    using var cmd = new OracleCommand($"SELECT COUNT(*) FROM {table}", connection);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    tableCounts[table] = count;
                }
                catch (Exception ex)
                {
                    tableCounts[table] = -1; // Error indicator
                }
            }
            results["TableCounts"] = tableCounts;

            return Ok(results);
        }
    }
}