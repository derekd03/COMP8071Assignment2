-- Employee
INSERT INTO Employee (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
VALUES (1, 'Alice Johnson', '123 Maple St', 'Registered Nurse', 'FullTime', 35.5, 4);

INSERT INTO Employee (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
VALUES (2, 'Bob Smith', '456 Oak Ave', 'Registered Nurse', 'FullTime', 34.75, 4);

INSERT INTO Employee (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
VALUES (3, 'Carol Lee', '789 Pine Rd', 'Physiotherapist', 'PartTime', 28.0, 4);

INSERT INTO Employee (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
VALUES (4, 'David Brown', '10 Center Plaza', 'Clinic Manager', 'Management', 55.0, NULL);

INSERT INTO Employee (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
VALUES (5, 'Eve Davis', '22 Lake View', 'Receptionist', 'PartTime', 20.0, 4);

INSERT INTO Employee (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
VALUES (6, 'Frank Miller', '31 Grove St', 'Home Health Aide', 'FullTime', 22.5, 4);

INSERT INTO Employee (EmployeeID, Name, Address, JobTitle, EmployeeType, SalaryRate, ReportsTo)
VALUES (7, 'Grace Kim', '17 Elm St', 'Nurse Practitioner', 'FullTime', 45.0, 4);

-- Payment
INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1001, 1, DATE '2025-10-31', 300.00, 450.00, 5200.00);

INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1002, 2, DATE '2025-10-31', 250.00, 380.00, 4850.00);

INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1003, 4, DATE '2025-10-31', 120.00, 150.00, 2100.00);

INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1004, 5, DATE '2025-10-31', 0.00, 700.00, 7300.00);

INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1005, 5, DATE '2025-10-31', 60.00, 80.00, 1600.00);

INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1006, 4, DATE '2025-10-31', 90.00, 120.00, 3100.00);

INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1007, 2, DATE '2025-11-15', 50.00, 100.00, 2500.00);

INSERT INTO Payment (PaymentID, EmployeeID, PayDate, OverTimePay, Deductions, BasePay)
VALUES (1008, 1, DATE '2025-11-15', 200.00, 300.00, 4700.00);

-- Client
INSERT INTO Client (ClientID, Name, Address, ContactInfo)
VALUES (5001, 'Green Valley Home', '12 Green Way', '555-1001; gv@example.com');

INSERT INTO Client (ClientID, Name, Address, ContactInfo)
VALUES (5002, 'Oakridge Apartments', '88 Oak Blvd', '555-1002; oak@example.com');

INSERT INTO Client (ClientID, Name, Address, ContactInfo)
VALUES (5003, 'Sunrise Care', '5 Dawn St', '555-1003; sunrise@example.com');

INSERT INTO Client (ClientID, Name, Address, ContactInfo)
VALUES (5004, 'Lakeside Family', '77 Lake Rd', '555-1004; lakeside@example.com');

INSERT INTO Client (ClientID, Name, Address, ContactInfo)
VALUES (5005, 'Mountain View Clinic', '3 Summit Ave', '555-1005; mv@example.com');

INSERT INTO Client (ClientID, Name, Address, ContactInfo)
VALUES (5006, 'Brookfield Community', '21 River Dr', '555-1006; brook@example.com');

-- ServiceType
INSERT INTO ServiceType (ServiceTypeID, ServiceName, Rate, RequiresCertification)
VALUES (1, 'Physiotherapy Session', 85.00, '1');

INSERT INTO ServiceType (ServiceTypeID, ServiceName, Rate, RequiresCertification)
VALUES (2, 'Home Nursing Visit', 120.00, '1');

INSERT INTO ServiceType (ServiceTypeID, ServiceName, Rate, RequiresCertification)
VALUES (3, 'Initial Consultation', 60.00, '0');

INSERT INTO ServiceType (ServiceTypeID, ServiceName, Rate, RequiresCertification)
VALUES (4, 'Emergency On-Call', 150.00, '1');

INSERT INTO ServiceType (ServiceTypeID, ServiceName, Rate, RequiresCertification)
VALUES (5, 'Wellness Check', 75.00, '0');

INSERT INTO ServiceType (ServiceTypeID, ServiceName, Rate, RequiresCertification)
VALUES (6, 'Post-op Care', 140.00, '1');

-- Asset
INSERT INTO Asset (AssetID, AssetType, Location, MonthlyRent)
VALUES (6001, 'Vehicle', 'Garage A', 800.00);

INSERT INTO Asset (AssetID, AssetType, Location, MonthlyRent)
VALUES (6002, 'Office Suite', 'Downtown Plaza Suite 3', 2500.00);

INSERT INTO Asset (AssetID, AssetType, Location, MonthlyRent)
VALUES (6003, 'Ultrasound Machine', 'Storage Room 2', 350.00);

INSERT INTO Asset (AssetID, AssetType, Location, MonthlyRent)
VALUES (6004, 'Laptop', 'IT Closet', 45.00);

INSERT INTO Asset (AssetID, AssetType, Location, MonthlyRent)
VALUES (6005, 'Rehab Bench', 'Therapy Room 1', 60.00);

INSERT INTO Asset (AssetID, AssetType, Location, MonthlyRent)
VALUES (6006, 'Storage Unit', 'Offsite Storage', 120.00);

-- Renter
INSERT INTO Renter (RenterID, Name, EmergencyContact, FamilyDoctor)
VALUES (7001, 'Sunrise Clinic', '555-5892', 'Dr. Chen');

INSERT INTO Renter (RenterID, Name, EmergencyContact, FamilyDoctor)
VALUES (7002, 'John Tenant', '555-0068', 'Dr. Patel');

INSERT INTO Renter (RenterID, Name, EmergencyContact, FamilyDoctor)
VALUES (7003, 'Oakridge HOA', '555-1122', 'Dr. James');

INSERT INTO Renter (RenterID, Name, EmergencyContact, FamilyDoctor)
VALUES (7004, 'Green Valley Co-Op', '555-7788', 'Dr. Nguyen');

INSERT INTO Renter (RenterID, Name, EmergencyContact, FamilyDoctor)
VALUES (7005, 'Brookfield PTA', '555-9090', 'Dr. Gomez');

INSERT INTO Renter (RenterID, Name, EmergencyContact, FamilyDoctor)
VALUES (7006, 'City Health Dept', '555-1234', 'Dr. Stone');

-- Shift
INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2001, 1, DATE '2025-10-01', DATE '2025-10-01', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2002, 1, DATE '2025-10-10', DATE '2025-10-10', '1');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2003, 2, DATE '2025-10-02', DATE '2025-10-02', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2004, 2, DATE '2025-10-15', DATE '2025-10-15', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2005, 3, DATE '2025-10-05', DATE '2025-10-05', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2006, 3, DATE '2025-10-20', DATE '2025-10-20', '1');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2007, 4, DATE '2025-10-01', DATE '2025-10-01', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2008, 4, DATE '2025-10-31', DATE '2025-10-31', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2009, 5, DATE '2025-10-12', DATE '2025-10-12', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2010, 5, DATE '2025-11-01', DATE '2025-11-01', '1');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2011, 6, DATE '2025-10-18', DATE '2025-10-18', '0');

INSERT INTO Shift (ShiftID, EmployeeID, StartTime, EndTime, IsOnCall)
VALUES (2012, 7, DATE '2025-11-10', DATE '2025-11-10', '0');

-- Service
INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9001, 1, 3, 5001, DATE '2025-10-05', DATE '2025-10-08', 170.00, '1');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9002, 2, 1, 5002, DATE '2025-10-10', DATE '2025-10-12', 120.00, '0');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9003, 3, 5, 5003, DATE '2025-10-07', DATE '2025-10-07', 60.00, '1');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9004, 4, 2, 5004, DATE '2025-10-28', DATE '2025-10-28', 150.00, '1');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9005, 5, 6, 5005, DATE '2025-10-20', DATE '2025-10-22', 75.00, '0');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9006, 2, 1, 5001, DATE '2025-11-01', DATE '2025-11-03', 120.00, '1');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9007, 6, 7, 5006, DATE '2025-11-05', DATE '2025-11-06', 280.00, '0');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9008, 1, 3, 5002, DATE '2025-11-02', DATE '2025-11-02', 85.00, '1');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9009, 5, 6, 5004, DATE '2025-11-07', DATE '2025-11-07', 75.00, '1');

INSERT INTO Service (ServiceID, ServiceTypeID, EmployeeID, ClientID, RegistrationDate, ScheduledDate, TotalAmount, IsPaid)
VALUES (9010, 4, 2, 5003, DATE '2025-11-10', DATE '2025-11-10', 150.00, '0');

-- AssetRent
INSERT INTO AssetRent (AssetRentID, AssetID, RenterID, StartDate, EndDate)
VALUES (7501, 6002, 7002, DATE '2025-09-01', DATE '2025-12-31');

INSERT INTO AssetRent (AssetRentID, AssetID, RenterID, StartDate, EndDate)
VALUES (7502, 6001, 7001, DATE '2025-10-01', DATE '2025-10-31');

INSERT INTO AssetRent (AssetRentID, AssetID, RenterID, StartDate, EndDate)
VALUES (7503, 6003, 7006, DATE '2025-10-15', DATE '2026-10-14');

INSERT INTO AssetRent (AssetRentID, AssetID, RenterID, StartDate, EndDate)
VALUES (7504, 6004, 7003, DATE '2025-11-01', DATE '2025-11-30');

INSERT INTO AssetRent (AssetRentID, AssetID, RenterID, StartDate, EndDate)
VALUES (7505, 6005, 7004, DATE '2025-09-15', DATE '2026-03-14');

INSERT INTO AssetRent (AssetRentID, AssetID, RenterID, StartDate, EndDate)
VALUES (7506, 6006, 7005, DATE '2025-08-01', DATE '2025-12-31');

INSERT INTO AssetRent (AssetRentID, AssetID, RenterID, StartDate, EndDate)
VALUES (7507, 6001, 7006, DATE '2025-11-01', DATE '2025-11-30');

COMMIT;