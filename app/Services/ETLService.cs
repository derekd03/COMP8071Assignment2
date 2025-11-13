using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

public class ETLService
{
    private readonly string _oltpConnectionString;
    public readonly string _olapConnectionString;

    public ETLService(string oltpConnectionString, string olapConnectionString)
    {
        _oltpConnectionString = oltpConnectionString;
        _olapConnectionString = olapConnectionString;
    }

    public async Task RunETLAsync(List<string> log)
    {
        await using var oltpConn = new OracleConnection(_oltpConnectionString);
        await using var olapConn = new OracleConnection(_olapConnectionString);

        await oltpConn.OpenAsync();
        await olapConn.OpenAsync();

        await ClearOlapTablesAsync(olapConn, log);
        await LoadDimEmployeesAsync(oltpConn, olapConn, log);
        await LoadDimClientsAsync(oltpConn, olapConn, log);
        await LoadDimServicesAsync(oltpConn, olapConn, log);
        await LoadDimAssetsAsync(oltpConn, olapConn, log);
        await LoadDimRentersAsync(oltpConn, olapConn, log);
        await LoadFactPayrollAsync(oltpConn, olapConn, log);
        await LoadFactShiftsAsync(oltpConn, olapConn, log);
        await LoadFactAttendanceAsync(oltpConn, olapConn, log);
        await LoadFactServiceAssignmentAsync(oltpConn, olapConn, log);
        await LoadFactInvoiceAsync(oltpConn, olapConn, log);
        await LoadFactServiceRegistrationAsync(oltpConn, olapConn, log);
        await LoadFactDamageReportAsync(oltpConn, olapConn, log);
        await LoadFactRentalHistoryAsync(oltpConn, olapConn, log);
    }

    public async Task ClearOlapTablesAsync(OracleConnection olapConn, List<string> log)
    {
        var tables = new string[]
        {
            "FactAttendance", "FactPayroll", "FactShifts", "FactServiceAssignment", "FactInvoice",
            "FactServiceRegistration", "FactDamageReport", "FactRentalHistory",
            "DimEmployee", "DimClient", "DimService", "DimAsset", "DimRenter"
        };

        foreach (var table in tables)
        {
            try
            {
                log.Add($"Clearing table {table}...");
                using var cmd = new OracleCommand($"DELETE FROM {table}", olapConn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                log.Add($"Error clearing {table}: {ex.Message}");
            }
        }

        log.Add("All OLAP tables cleared.");
    }

    private async Task LoadDimEmployeesAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading DimEmployees...");
        using var selectCmd = new OracleCommand("SELECT EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo FROM Employee", oltpConn);
        using var reader = await selectCmd.ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var insertCmd = new OracleCommand(
                @"INSERT INTO DimEmployee 
                  (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
                  VALUES (:EmployeeID, :Name, :Address, :JobTitle, :EmployeeType, :SalaryRate, :ReportsTo)", olapConn);

            insertCmd.Parameters.Add("EmployeeID", reader["EmployeeID"]);
            insertCmd.Parameters.Add("Name", reader["Name"]);
            insertCmd.Parameters.Add("Address", reader["Address"]);
            insertCmd.Parameters.Add("JobTitle", reader["JobTitle"]);
            insertCmd.Parameters.Add("EmployeeType", reader["EmployeeType"]);
            insertCmd.Parameters.Add("SalaryRate", reader["SalaryRate"]);
            insertCmd.Parameters.Add("ReportsTo", reader["ReportsTo"] is DBNull ? DBNull.Value : reader["ReportsTo"]);

            await insertCmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"DimEmployees loaded successfully. {count} records inserted.");
    }

    private async Task LoadDimClientsAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading DimClients...");
        using var reader = await new OracleCommand("SELECT ClientID, Name, Address, ContactInfo FROM Client", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO DimClient (ClientID, Name, Address, ContactInfo) 
                  VALUES (:ClientID, :Name, :Address, :ContactInfo)", olapConn);

            cmd.Parameters.Add("ClientID", reader["ClientID"]);
            cmd.Parameters.Add("Name", reader["Name"]);
            cmd.Parameters.Add("Address", reader["Address"]);
            cmd.Parameters.Add("ContactInfo", reader["ContactInfo"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"DimClients loaded successfully. {count} records inserted.");
    }

    private async Task LoadDimServicesAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading DimServices...");
        using var reader = await new OracleCommand("SELECT ServiceTypeID, ServiceName, Rate, RequiresCertification FROM ServiceType", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO DimService (ServiceID, ServiceName, Rate, RequiresCertification) 
                  VALUES (:ServiceID, :ServiceName, :Rate, :RequiresCertification)", olapConn);

            cmd.Parameters.Add("ServiceID", reader["ServiceTypeID"]);
            cmd.Parameters.Add("ServiceName", reader["ServiceName"]);
            cmd.Parameters.Add("Rate", reader["Rate"]);
            cmd.Parameters.Add("RequiresCertification", reader["RequiresCertification"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"DimServices loaded successfully. {count} records inserted.");
    }

    private async Task LoadDimAssetsAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading DimAssets...");
        using var reader = await new OracleCommand("SELECT AssetID, AssetType, Location, MonthlyRent FROM Asset", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO DimAsset (AssetID, AssetType, Location, MonthlyRent) 
                  VALUES (:AssetID, :AssetType, :Location, :MonthlyRent)", olapConn);

            cmd.Parameters.Add("AssetID", reader["AssetID"]);
            cmd.Parameters.Add("AssetType", reader["AssetType"]);
            cmd.Parameters.Add("Location", reader["Location"]);
            cmd.Parameters.Add("MonthlyRent", reader["MonthlyRent"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"DimAssets loaded successfully. {count} records inserted.");
    }

    private async Task LoadDimRentersAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading DimRenters...");
        using var reader = await new OracleCommand("SELECT RenterID, Name, EmergencyContact, FamilyDoctor FROM Renter", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO DimRenter (RenterID, Name, EmergencyContact, FamilyDoctor) 
                  VALUES (:RenterID, :Name, :EmergencyContact, :FamilyDoctor)", olapConn);

            cmd.Parameters.Add("RenterID", reader["RenterID"]);
            cmd.Parameters.Add("Name", reader["Name"]);
            cmd.Parameters.Add("EmergencyContact", reader["EmergencyContact"]);
            cmd.Parameters.Add("FamilyDoctor", reader["FamilyDoctor"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"DimRenters loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactPayrollAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactPayroll...");
        using var reader = await new OracleCommand("SELECT PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay FROM Payment", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            decimal basePay = reader["BasePay"] is DBNull ? 0 : Convert.ToDecimal(reader["BasePay"]);
            decimal overTime = reader["OverTimePay"] is DBNull ? 0 : Convert.ToDecimal(reader["OverTimePay"]);
            decimal deductions = reader["Deductions"] is DBNull ? 0 : Convert.ToDecimal(reader["Deductions"]);
            decimal netPay = basePay + overTime - deductions;

            using var cmd = new OracleCommand(
                @"INSERT INTO FactPayroll 
                  (PayrollID, DimEmployeeID, PayDate, BaseSalary, OverTimePay, Deductions, NetPay)
                  VALUES (:PayrollID, :EmployeeID, :PayDate, :BaseSalary, :OverTimePay, :Deductions, :NetPay)", olapConn);

            cmd.Parameters.Add("PayrollID", reader["PaymentID"]);
            cmd.Parameters.Add("EmployeeID", reader["EmployeeID"]);
            cmd.Parameters.Add("PayDate", reader["PayDate"]);
            cmd.Parameters.Add("BaseSalary", basePay);
            cmd.Parameters.Add("OverTimePay", overTime);
            cmd.Parameters.Add("Deductions", deductions);
            cmd.Parameters.Add("NetPay", netPay);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"FactPayroll loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactShiftsAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactShifts...");
        using var reader = await new OracleCommand("SELECT ShiftID, EmployeeID, StartTime, EndTime, IsOnCall FROM Shift", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO FactShifts (ShiftID, DimEmployeeID, StartTime, EndTime, IsOnCall)
                  VALUES (:ShiftID, :EmployeeID, :StartTime, :EndTime, :IsOnCall)", olapConn);

            cmd.Parameters.Add("ShiftID", reader["ShiftID"]);
            cmd.Parameters.Add("EmployeeID", reader["EmployeeID"]);
            cmd.Parameters.Add("StartTime", reader["StartTime"]);
            cmd.Parameters.Add("EndTime", reader["EndTime"]);
            cmd.Parameters.Add("IsOnCall", reader["IsOnCall"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"FactShifts loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactAttendanceAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactAttendance...");
        using var reader = await new OracleCommand("SELECT ShiftID, EmployeeID, IsOnCall FROM Shift", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO FactAttendance (AttendanceID, DimEmployeeID, FactShiftsID, IsHoliday, IsVacation, IsOnCall)
                  VALUES (:AttendanceID, :EmployeeID, :ShiftID, '0', '0', :IsOnCall)", olapConn);

            cmd.Parameters.Add("AttendanceID", reader["ShiftID"]);
            cmd.Parameters.Add("EmployeeID", reader["EmployeeID"]);
            cmd.Parameters.Add("ShiftID", reader["ShiftID"]);
            cmd.Parameters.Add("IsOnCall", reader["IsOnCall"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"FactAttendance loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactServiceAssignmentAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactServiceAssignment...");
        using var reader = await new OracleCommand("SELECT ServiceID, ServiceTypeID, EmployeeID, ScheduledDate FROM Service", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO FactServiceAssignment (AssignedID, DimEmployeeID, DimServiceID, ScheduledDate)
                  VALUES (:AssignedID, :EmployeeID, :ServiceID, :ScheduledDate)", olapConn);

            cmd.Parameters.Add("AssignedID", reader["ServiceID"]);
            cmd.Parameters.Add("EmployeeID", reader["EmployeeID"]);
            cmd.Parameters.Add("ServiceID", reader["ServiceTypeID"]);
            cmd.Parameters.Add("ScheduledDate", reader["ScheduledDate"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"FactServiceAssignment loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactInvoiceAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactInvoice...");
        using var reader = await new OracleCommand("SELECT ServiceID, ServiceTypeID, ClientID, ScheduledDate, TotalAmount, IsPaid FROM Service", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO FactInvoice (InvoiceID, DimClientID, DimServiceID, InvoiceDate, TotalAmount, IsPaid)
                  VALUES (:InvoiceID, :ClientID, :ServiceID, :InvoiceDate, :TotalAmount, :IsPaid)", olapConn);

            cmd.Parameters.Add("InvoiceID", reader["ServiceID"]);
            cmd.Parameters.Add("ClientID", reader["ClientID"]);
            cmd.Parameters.Add("ServiceID", reader["ServiceTypeID"]);
            cmd.Parameters.Add("InvoiceDate", reader["ScheduledDate"]);
            cmd.Parameters.Add("TotalAmount", reader["TotalAmount"]);
            cmd.Parameters.Add("IsPaid", reader["IsPaid"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"FactInvoice loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactServiceRegistrationAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactServiceRegistration...");
        using var reader = await new OracleCommand("SELECT ServiceID, ServiceTypeID, ClientID, RegistrationDate FROM Service", oltpConn).ExecuteReaderAsync();
        int count = 0;
        
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO FactServiceRegistration (RegistrationID, DimClientID, DimServiceID, RegistrationDate)
                  VALUES (:RegistrationID, :ClientID, :ServiceID, :RegistrationDate)", olapConn);

            cmd.Parameters.Add("RegistrationID", reader["ServiceID"]);
            cmd.Parameters.Add("ClientID", reader["ClientID"]);
            cmd.Parameters.Add("ServiceID", reader["ServiceTypeID"]);
            cmd.Parameters.Add("RegistrationDate", reader["RegistrationDate"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"FactServiceRegistration loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactDamageReportAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactDamageReport...");
            
            using var assetReader = await new OracleCommand("SELECT AssetID FROM Asset", oltpConn).ExecuteReaderAsync();
            int count = 0;
            int reportId = 1;
            
            while (await assetReader.ReadAsync())
            {
                try
                {
                    using var cmd = new OracleCommand(
                        @"INSERT INTO FactDamageReport (ReportID, DimAssetID, ReportDate, RepairCost, Description)
                        VALUES (:ReportID, :AssetID, :ReportDate, :RepairCost, :Description)", olapConn);

                    cmd.Parameters.Add("ReportID", reportId++);
                    cmd.Parameters.Add("AssetID", assetReader["AssetID"]);
                    cmd.Parameters.Add("ReportDate", DateTime.Now.AddDays(-Random.Shared.Next(1, 90)));
                    cmd.Parameters.Add("RepairCost", Random.Shared.Next(50, 500));
                    cmd.Parameters.Add("Description", "Generated damage report for maintenance");

                    await cmd.ExecuteNonQueryAsync();
                    count++;
                }
                catch (Exception ex)
                {
                    log.Add($"Error inserting damage report for asset {assetReader["AssetID"]}: {ex.Message}");
                }
            }
            log.Add($"FactDamageReport loaded successfully. {count} records inserted.");
    }

    private async Task LoadFactRentalHistoryAsync(OracleConnection oltpConn, OracleConnection olapConn, List<string> log)
    {
        log.Add("Loading FactRentalHistory...");
        using var reader = await new OracleCommand(
            @"SELECT ar.AssetRentID, ar.AssetID, ar.RenterID, ar.StartDate, ar.EndDate, a.MonthlyRent
              FROM AssetRent ar JOIN Asset a ON ar.AssetID = a.AssetID", oltpConn).ExecuteReaderAsync();

        int count = 0;
        while (await reader.ReadAsync())
        {
            using var cmd = new OracleCommand(
                @"INSERT INTO FactRentalHistory (HistoryID, DimAssetID, DimRenterID, StartDate, EndDate, RentAmount)
                  VALUES (history_seq.NEXTVAL, :AssetID, :RenterID, :StartDate, :EndDate, :RentAmount)", olapConn);

            cmd.Parameters.Add("AssetID", reader["AssetID"]);
            cmd.Parameters.Add("RenterID", reader["RenterID"]);
            cmd.Parameters.Add("StartDate", reader["StartDate"]);
            cmd.Parameters.Add("EndDate", reader["EndDate"]);
            cmd.Parameters.Add("RentAmount", reader["MonthlyRent"]);

            await cmd.ExecuteNonQueryAsync();
            count++;
        }
        log.Add($"FactRentalHistory loaded successfully. {count} records inserted.");
    }
}