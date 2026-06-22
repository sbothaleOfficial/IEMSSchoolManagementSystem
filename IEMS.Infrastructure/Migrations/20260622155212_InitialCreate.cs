using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IEMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectricityBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BillNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BillMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    BillYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Units = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitsRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: true),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectricityBills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtherExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpenseType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    VendorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherExpenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AadharNumber = table.Column<string>(type: "TEXT", maxLength: 12, nullable: true),
                    PANNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MonthlySalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "TEXT", nullable: true),
                    AadharNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PANNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    VehicleType = table.Column<int>(type: "INTEGER", nullable: false),
                    DriverName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DriverPhone = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Route = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Section = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransportExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    FuelType = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DriverName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransportExpenses_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeType = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AcademicYearId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcademicYearString = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeStructures_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeStructures_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SerialNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Standard = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClassDivision = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FatherName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    MotherName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StudentNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CasteCategory = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Religion = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsBPL = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSemiEnglish = table.Column<bool>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CityVillage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ParentMobileNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    AadhaarNumber = table.Column<string>(type: "TEXT", maxLength: 12, nullable: true),
                    AdmissionAcademicYearId = table.Column<int>(type: "INTEGER", nullable: true),
                    AdmissionAcademicYearString = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_AcademicYears_AdmissionAcademicYearId",
                        column: x => x.AdmissionAcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceiptNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeType = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PaymentNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PreviousBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcademicYearId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcademicYearString = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    GeneratedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeePayments_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeePayments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentPromotionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FromClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromClassName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ToClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToClassName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AcademicYearId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcademicYearString = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PromotionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PromotedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPromotionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPromotionHistory_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentPromotionHistory_Classes_FromClassId",
                        column: x => x.FromClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentPromotionHistory_Classes_ToClassId",
                        column: x => x.ToClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentPromotionHistory_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AcademicYears",
                columns: new[] { "Id", "CreatedAt", "EndDate", "IsCurrent", "StartDate", "UpdatedAt", "Year" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2022, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2022-23" },
                    { 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2023-24" },
                    { 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2024-25" },
                    { 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2025-26" }
                });

            migrationBuilder.InsertData(
                table: "ElectricityBills",
                columns: new[] { "Id", "Amount", "BankName", "BillMonth", "BillNumber", "BillYear", "ChequeNumber", "CreatedAt", "DueDate", "IsPaid", "Notes", "PaidDate", "PaymentMethod", "TransactionId", "Units", "UnitsRate", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 7200m, null, 1, "EB001", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "January 2024 electricity bill", new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, 150m, 4.8m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 6800m, null, 2, "EB002", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "February 2024 electricity bill", new DateTime(2024, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "TXN123456", 140m, 4.8m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 7500m, "SBI Bank", 3, "EB003", 2024, "CH001", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "March 2024 electricity bill", new DateTime(2024, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, null, 155m, 4.8m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 8200m, null, 4, "EB004", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "April 2024 electricity bill", new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, 170m, 4.8m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 9500m, null, 5, "EB005", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "May 2024 electricity bill - High consumption due to summer", new DateTime(2024, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "TXN234567", 200m, 4.75m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 10200m, null, 6, "EB006", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "June 2024 electricity bill - Peak summer consumption", new DateTime(2024, 7, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, 220m, 4.6m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 7800m, null, 7, "EB007", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "July 2024 electricity bill", new DateTime(2024, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "TXN789123", 165m, 4.7m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 8500m, null, 8, "EB008", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "August 2024 electricity bill", new DateTime(2024, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, 180m, 4.7m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 9200m, null, 9, "EB009", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "September 2024 electricity bill - Pending payment", null, null, null, 195m, 4.7m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 8800m, null, 10, "EB010", 2024, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "October 2024 electricity bill - Current month", null, null, null, 185m, 4.75m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "OtherExpenses",
                columns: new[] { "Id", "Amount", "BankName", "Category", "ChequeNumber", "CreatedAt", "Description", "ExpenseDate", "ExpenseType", "InvoiceNumber", "Notes", "PaymentMethod", "TransactionId", "UpdatedAt", "VendorName" },
                values: new object[,]
                {
                    { 1, 2500m, null, 0, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Books, pens, papers for office use", new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Office Supplies", "INV001", null, 0, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Shree Stationery Mart" },
                    { 2, 15000m, null, 1, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Decorations, refreshments, and prizes for Independence Day", new DateTime(2024, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Independence Day Celebration", "INV002", null, 1, "TXN456789", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Event Decorators" },
                    { 3, 5500m, "SBI Bank", 2, "123456", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Repair of desks and chairs in Class 10 classroom", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Classroom Repair", "INV003", null, 2, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Repair Services" },
                    { 4, 8200m, null, 0, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Charts, models, and laboratory equipment", new DateTime(2024, 5, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Teaching Materials", "INV004", null, 0, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Educational Supplies Co" },
                    { 5, 22000m, null, 1, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sports equipment, prizes, and refreshments for annual sports day", new DateTime(2024, 4, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Sports Day", "INV005", null, 1, "TXN567890", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sports Events Org" },
                    { 6, 12000m, "HDFC Bank", 2, "234567", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Repair of washroom facilities and water pipeline", new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Plumbing Work", "INV006", null, 2, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "City Plumbers" },
                    { 7, 4500m, null, 0, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Keyboards, mouse, cables for computer lab", new DateTime(2024, 5, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Computer Accessories", "INV007", null, 0, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tech Solutions" },
                    { 8, 3500m, null, 1, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Flowers, gifts, and refreshments for teachers day", new DateTime(2024, 4, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers Day Celebration", "INV008", null, 0, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Local Florist" },
                    { 9, 6800m, null, 2, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Plant care, fertilizers, and gardening tools", new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Garden Maintenance", "INV009", null, 1, "TXN678901", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Green Gardens" },
                    { 10, 18500m, "SBI Bank", 0, "345678", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "New textbooks and reference books for library", new DateTime(2024, 4, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Library Books", "INV010", null, 2, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Academic Publishers" }
                });

            migrationBuilder.InsertData(
                table: "Staff",
                columns: new[] { "Id", "AadharNumber", "Address", "BankAccountNumber", "CreatedAt", "Email", "EmployeeId", "FirstName", "JoiningDate", "LastName", "MonthlySalary", "PANNumber", "PhoneNumber", "Position", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "123456789012", "101 Transport Ave, Mumbai", "1234567890", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rajesh.kumar@school.edu", "ST001", "Rajesh", new DateTime(2020, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar", 25000m, "ABCDE1234F", "9876543213", "Driver", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "234567890123", "202 Clean St, Delhi", "2345678901", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "kamala.devi@school.edu", "ST002", "Kamala", new DateTime(2019, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Devi", 18000m, "BCDEF2345G", "9876543215", "Peon", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "345678901234", "303 Office Lane, Pune", "3456789012", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "suresh.singh@school.edu", "ST003", "Suresh", new DateTime(2021, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh", 22000m, "CDEFG3456H", "9876543217", "Clerk", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "456789012345", "404 Support St, Bangalore", "4567890123", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "priya.mehta@school.edu", "ST004", "Priya", new DateTime(2020, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mehta", 20000m, "DEFGH4567I", "9876543218", "Lab Assistant", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "567890123456", "505 Maintenance Rd, Chennai", "5678901234", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mohan.rao@school.edu", "ST005", "Mohan", new DateTime(2018, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rao", 28000m, "EFGHI5678J", "9876543219", "Electrician", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "678901234567", "606 Admin Block, Kolkata", "6789012345", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sunita.nair@school.edu", "ST006", "Sunita", new DateTime(2021, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nair", 19000m, "FGHIJ6789K", "9876543220", "Office Assistant", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "789012345678", "707 Security Gate, Hyderabad", "7890123456", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "vinod.patil@school.edu", "ST007", "Vinod", new DateTime(2019, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patil", 16000m, "GHIJK7890L", "9876543221", "Security Guard", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "890123456789", "808 Library Block, Ahmedabad", "8901234567", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lata.desai@school.edu", "ST008", "Lata", new DateTime(2020, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Desai", 24000m, "HIJKL8901M", "9876543222", "Librarian", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "901234567890", "909 Transport Yard, Jaipur", "9012345678", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ravi.iyer@school.edu", "ST009", "Ravi", new DateTime(2017, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Iyer", 26000m, "IJKLM9012N", "9876543223", "Mechanic", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "012345678901", "101 Canteen Block, Lucknow", "0123456789", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "geeta.sharma@school.edu", "ST010", "Geeta", new DateTime(2022, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma", 17000m, "JKLMN0123O", "9876543224", "Cook", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Key", "Category", "CreatedAt", "DataType", "DefaultValue", "Description", "IsReadOnly", "ModifiedAt", "Value" },
                values: new object[,]
                {
                    { "Backup.AutoBackupEnabled", "Backup", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Boolean", "true", "Enable automatic backup", false, null, "true" },
                    { "Backup.BackupPath", "Backup", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DirectoryPath", "C:\\Users\\SP\\Documents\\IEMS_Backups", "Default backup directory path", false, null, "C:\\Users\\SP\\Documents\\IEMS_Backups" },
                    { "Backup.RetentionDays", "Backup", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Integer", "30", "Days to retain backup files", false, null, "30" },
                    { "School.AccreditationNumber", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "115381/2016", "School accreditation number", false, null, "115381/2016" },
                    { "School.AddressLine1", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "Tah. Maregaon, Dist. Yavatmal (Maharashtra)", "School address line 1", false, null, "Tah. Maregaon, Dist. Yavatmal (Maharashtra)" },
                    { "School.AlternatePhone", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "", "School alternate phone number", false, null, "" },
                    { "School.Board", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "State", "Educational board affiliation", false, null, "State" },
                    { "School.Email", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "inspire.mardi@gmail.com", "School official email address", false, null, "inspire.mardi@gmail.com" },
                    { "School.ManagementName", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "Mahalakmi Bahuddeshiy Sanstha, Chikhalgaon", "Name of school management organization", false, null, "Mahalakmi Bahuddeshiy Sanstha, Chikhalgaon" },
                    { "School.Name", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "Inspire English Medium School, Mardi", "Official school name", false, null, "Inspire English Medium School, Mardi" },
                    { "School.Phone", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "8483949981", "School primary phone number", false, null, "8483949981" },
                    { "School.PinCode", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "445303", "School pin code", false, null, "445303" },
                    { "School.UDISECode", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "27140806704", "U-DISE code for school", false, null, "27140806704" },
                    { "School.Website", "School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "String", "", "School website URL", false, null, "" }
                });

            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "Id", "AadharNumber", "Address", "BankAccountNumber", "CreatedAt", "Email", "EmployeeId", "FirstName", "JoiningDate", "LastName", "MonthlySalary", "PANNumber", "PhoneNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "1234-5678-9012", "123 Teachers Colony, Mumbai", "SBI1234567890", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "priya.sharma@iemsschool.edu.in", "T001", "Priya", new DateTime(2020, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma", 55000m, "ABCDE1234F", "9876543201", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "2345-6789-0123", "456 Gandhi Nagar, Pune", "HDFC9876543210", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rajesh.patel@iemsschool.edu.in", "T002", "Rajesh", new DateTime(2019, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel", 62000m, "FGHIJ5678K", "9876543202", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "3456-7890-1234", "789 Shivaji Park, Nashik", "ICICI5432109876", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "anita.kulkarni@iemsschool.edu.in", "T003", "Anita", new DateTime(2021, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kulkarni", 48000m, "LMNOP9012Q", "9876543203", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "4567-8901-2345", "101 Nehru Colony, Nagpur", "AXIS6789012345", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "suresh.gupta@iemsschool.edu.in", "T004", "Suresh", new DateTime(2018, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta", 68000m, "RSTUV3456W", "9876543204", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "5678-9012-3456", "202 Laxmi Nagar, Aurangabad", "PNB3456789012", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "kavita.singh@iemsschool.edu.in", "T005", "Kavita", new DateTime(2022, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh", 45000m, "WXYZE7890A", "9876543205", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "6789-0123-4567", "303 Saraswati Vihar, Delhi", "BOI4567890123", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "amit.verma@iemsschool.edu.in", "T006", "Amit", new DateTime(2020, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma", 52000m, "BCDEF8901B", "9876543206", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "7890-1234-5678", "404 Indira Nagar, Jaipur", "UCO5678901234", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sunita.joshi@iemsschool.edu.in", "T007", "Sunita", new DateTime(2019, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi", 58000m, "CDEFG9012C", "9876543207", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "8901-2345-6789", "505 Vasant Kunj, Lucknow", "CBI6789012345", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "vikram.yadav@iemsschool.edu.in", "T008", "Vikram", new DateTime(2021, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav", 46000m, "DEFGH0123D", "9876543208", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "9012-3456-7890", "606 MG Road, Bangalore", "IOB7890123456", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "meera.agarwal@iemsschool.edu.in", "T009", "Meera", new DateTime(2020, 7, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal", 54000m, "EFGHI1234E", "9876543209", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "0123-4567-8901", "707 Park Street, Kolkata", "UNION8901234567", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "rohit.mishra@iemsschool.edu.in", "T010", "Rohit", new DateTime(2018, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra", 65000m, "FGHIJ2345F", "9876543210", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "FullName", "IsActive", "LastLogin", "ModifiedBy", "ModifiedDate", "MustChangePassword", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, "System", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@iems.school", "System Administrator", true, null, null, null, true, "AQIDBAUGBwgJCgsMDQ4PEEid+CGqDjXQiN9J1YT+4eNXW2t9UKfghgWIcum2O9k6", "Admin", "admin" });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "CreatedAt", "DriverName", "DriverPhone", "Route", "UpdatedAt", "VehicleNumber", "VehicleType" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rajesh Kumar", "9876543213", "Main Street - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MH12AB1234", 1 },
                    { 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Suresh Patil", "9876543214", "Park Road - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MH12CD5678", 2 },
                    { 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mohan Singh", "9876543215", "Highway - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MH12EF9012", 3 },
                    { 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ashok Yadav", "9876543216", "South Delhi - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DL8CAB9876", 1 },
                    { 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ramesh Gupta", "9876543217", "Lucknow City - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UP16GH3456", 2 },
                    { 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Prakash Nair", "9876543218", "Bangalore North - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "KA05JK7890", 3 },
                    { 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Murugan Pillai", "9876543219", "Chennai Central - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "TN07LM2345", 1 },
                    { 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Biswajit Das", "9876543220", "Kolkata East - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "WB10NO6789", 2 },
                    { 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dinesh Sharma", "9876543221", "Jaipur Pink City - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RJ14PQ0123", 3 },
                    { 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kiran Patel", "9876543222", "Ahmedabad West - School", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "GJ01RS4567", 1 }
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "CreatedAt", "Name", "Section", "TeacherId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nursery", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "KG1", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "KG2", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 1", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 2", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 3", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 4", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 5", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 6", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 7", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 8", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 9", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Class 10", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "TransportExpenses",
                columns: new[] { "Id", "Amount", "Category", "CreatedAt", "Description", "DriverName", "ExpenseDate", "FuelType", "InvoiceNumber", "Quantity", "UpdatedAt", "VehicleId" },
                values: new object[,]
                {
                    { 1, 5000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fuel refill for bus", "Rajesh Kumar", new DateTime(2024, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), 1, "F001", 50m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, 1200m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CNG refill for auto", "Suresh Patil", new DateTime(2024, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 3, "F002", 20m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 3, 3500m, 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Brake pad replacement", "Mohan Singh", new DateTime(2024, 5, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "M001", 1m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 4, 6200m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Weekly diesel refill", "Ashok Yadav", new DateTime(2024, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), 1, "F003", 65m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4 },
                    { 5, 980m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CNG refill", "Ramesh Gupta", new DateTime(2024, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), 3, "F004", 15m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5 },
                    { 6, 2800m, 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tire replacement", "Prakash Nair", new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "M002", 1m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6 },
                    { 7, 4800m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Daily diesel supply", "Murugan Pillai", new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Utc), 1, "F005", 45m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7 },
                    { 8, 1400m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auto CNG refill", "Biswajit Das", new DateTime(2024, 5, 26, 0, 0, 0, 0, DateTimeKind.Utc), 3, "F006", 22m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8 },
                    { 9, 4200m, 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Engine service and oil change", "Dinesh Sharma", new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Utc), null, "M003", 1m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9 },
                    { 10, 5500m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bus fuel refill", "Kiran Patel", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Utc), 1, "F007", 55m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10 }
                });

            migrationBuilder.InsertData(
                table: "FeeStructures",
                columns: new[] { "Id", "AcademicYearId", "AcademicYearString", "Amount", "ClassId", "CreatedAt", "Description", "FeeType", "IsActive", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 3, "2024-25", 60000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 10-A", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 3, "2024-25", 60000m, 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 10-B", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 3, "2024-25", 55000m, 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 9-A", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 3, "2024-25", 55000m, 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 9-B", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 3, "2024-25", 50000m, 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 8-A", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 3, "2024-25", 50000m, 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 8-B", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 3, "2024-25", 45000m, 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 7-A", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 3, "2024-25", 40000m, 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 6-A", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 3, "2024-25", 38000m, 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 5-A", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 3, "2024-25", 35000m, 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Tuition Fees for Class 1-A", 0, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 3, "2024-25", 5000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admission Fee for Class 10-A", 1, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 3, "2024-25", 3000m, 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admission Fee for Class 1-A", 1, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, 3, "2024-25", 2000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Examination Fee for Class 10-A", 2, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, 3, "2024-25", 2000m, 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Examination Fee for Class 10-B", 2, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, 3, "2024-25", 1800m, 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Examination Fee for Class 9-A", 2, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 3, "2024-25", 12000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Transport Fee for Class 10-A", 3, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 3, "2024-25", 12000m, 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Transport Fee for Class 10-B", 3, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 3, "2024-25", 10000m, 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Transport Fee for Class 8-A", 3, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, 3, "2024-25", 3000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Sports Fee for Class 10-A", 4, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, 3, "2024-25", 2500m, 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Sports Fee for Class 9-A", 4, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, 3, "2024-25", 1500m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Library Fee for Class 10-A", 5, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, 3, "2024-25", 1500m, 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Library Fee for Class 10-B", 5, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, 3, "2024-25", 800m, 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Annual Library Fee for Class 1-A", 5, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 3, "2024-25", 4000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Uniform Fee for Class 10-A", 6, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, 3, "2024-25", 3000m, 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Uniform Fee for Class 1-A", 6, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, 3, "2024-25", 2000m, 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Miscellaneous Charges for Class 10-A", 7, true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "AadhaarNumber", "Address", "AdmissionAcademicYearId", "AdmissionAcademicYearString", "AdmissionDate", "CasteCategory", "CityVillage", "ClassDivision", "ClassId", "CreatedAt", "DateOfBirth", "FatherName", "FirstName", "Gender", "IsBPL", "IsSemiEnglish", "MotherName", "ParentMobileNumber", "Religion", "SerialNo", "Standard", "StudentNumber", "Surname", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "100150019001", "11 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Mumbai", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Aarav", "Male", false, true, "Pihu Sharma", "9876543211", "Hindu", 1, "Nursery", "S001", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "100250029002", "21 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Delhi", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ananya", "Female", false, true, "Krishna Patel", "9876543212", "Muslim", 2, "Nursery", "S002", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "100350039003", "31 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Pune", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Arjun", "Male", false, false, "Advik Singh", "9876543213", "Christian", 3, "Nursery", "S003", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "100450049004", "41 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Bangalore", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Diya", "Female", false, true, "Samaira Kumar", "9876543214", "Sikh", 4, "Nursery", "S004", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "100550059005", "51 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Chennai", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Ishaan", "Male", true, true, "Kiaan Gupta", "9876543215", "Jain", 5, "Nursery", "S005", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "100650069006", "61 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Kolkata", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Kavya", "Female", false, false, "Avni Agarwal", "9876543216", "Buddhist", 6, "Nursery", "S006", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "100750079007", "71 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Hyderabad", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Karan", "Male", false, true, "Aryan Verma", "9876543217", "Hindu", 7, "Nursery", "S007", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "100850089008", "81 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Ahmedabad", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Myra", "Female", false, true, "Pari Jain", "9876543218", "Muslim", 8, "Nursery", "S008", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "100950099009", "91 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Jaipur", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Riya", "Male", false, false, "Sai Shah", "9876543219", "Christian", 9, "Nursery", "S009", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "101050109010", "101 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Lucknow", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Vihaan", "Female", true, true, "Atharv Yadav", "9876543220", "Sikh", 10, "Nursery", "S010", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, "101150119011", "111 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kanpur", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Saanvi", "Male", false, true, "Ira Reddy", "9876543221", "Jain", 11, "Nursery", "S011", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, "101250129012", "121 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Nagpur", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Reyansh", "Female", false, false, "Ayaan Khan", "9876543222", "Buddhist", 12, "Nursery", "S012", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, "101350139013", "131 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Indore", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Anvi", "Male", false, true, "Navya Mishra", "9876543223", "Hindu", 13, "Nursery", "S013", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, "101450149014", "141 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Bhopal", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Vivaan", "Female", false, true, "Rudra Chopra", "9876543224", "Muslim", 14, "Nursery", "S014", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, "101550159015", "151 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Patna", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Aadya", "Male", true, false, "Anaya Bansal", "9876543225", "Christian", 15, "Nursery", "S015", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, "101650169016", "161 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Vadodara", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Aayan", "Female", false, true, "Om Agrawal", "9876543226", "Sikh", 16, "Nursery", "S016", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, "101750179017", "171 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Ludhiana", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Pihu", "Male", false, true, "Tara Malhotra", "9876543227", "Jain", 17, "Nursery", "S017", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, "101850189018", "181 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Agra", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Krishna", "Female", false, false, "Shaurya Kapoor", "9876543228", "Buddhist", 18, "Nursery", "S018", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, "101950199019", "191 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Nashik", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Advik", "Male", false, true, "Kashvi Mittal", "9876543229", "Hindu", 19, "Nursery", "S019", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, "102050209020", "201 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Faridabad", "A", 1, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2021, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Samaira", "Female", true, true, "Mihir Joshi", "9876543230", "Muslim", 20, "Nursery", "S020", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, "102150219021", "211 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Meerut", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Kiaan", "Male", false, false, "Prisha Saxena", "9876543231", "Christian", 21, "KG1", "S021", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, "102250229022", "221 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Rajkot", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Avni", "Female", false, true, "Aadhya Srivastava", "9876543232", "Sikh", 22, "KG1", "S022", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, "102350239023", "231 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kalyan", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Aryan", "Male", false, true, "Dev Pandey", "9876543233", "Jain", 23, "KG1", "S023", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, "102450249024", "241 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Vasai", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Pari", "Female", false, false, "Krisha Tiwari", "9876543234", "Buddhist", 24, "KG1", "S024", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, "102550259025", "251 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Varanasi", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Sai", "Male", true, true, "Arnav Dubey", "9876543235", "Hindu", 25, "KG1", "S025", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, "102650269026", "261 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Mumbai", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Atharv", "Female", false, true, "Janvi Sharma", "9876543236", "Muslim", 26, "KG1", "S026", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, "102750279027", "271 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Delhi", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ira", "Male", false, false, "Yug Patel", "9876543237", "Christian", 27, "KG1", "S027", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 28, "102850289028", "281 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Pune", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Ayaan", "Female", false, true, "Shanaya Singh", "9876543238", "Sikh", 28, "KG1", "S028", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, "102950299029", "291 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Bangalore", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Navya", "Male", false, true, "Arjun Kumar", "9876543239", "Jain", 29, "KG1", "S029", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, "103050309030", "301 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Chennai", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 7, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Rudra", "Female", true, false, "Anika Gupta", "9876543240", "Buddhist", 30, "KG1", "S030", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, "103150319031", "311 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kolkata", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Anaya", "Male", false, true, "Vedant Agarwal", "9876543241", "Hindu", 31, "KG1", "S031", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, "103250329032", "321 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Hyderabad", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Om", "Female", false, true, "Reet Verma", "9876543242", "Muslim", 32, "KG1", "S032", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 33, "103350339033", "331 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Ahmedabad", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Tara", "Male", false, false, "Aarush Jain", "9876543243", "Christian", 33, "KG1", "S033", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, "103450349034", "341 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Jaipur", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Shaurya", "Female", false, true, "Khushi Shah", "9876543244", "Sikh", 34, "KG1", "S034", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, "103550359035", "351 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Lucknow", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Kashvi", "Male", true, true, "Aarav Yadav", "9876543245", "Jain", 35, "KG1", "S035", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 36, "103650369036", "361 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kanpur", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Mihir", "Female", false, false, "Ananya Reddy", "9876543246", "Buddhist", 36, "KG1", "S036", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 37, "103750379037", "371 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Nagpur", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Prisha", "Male", false, true, "Arjun Khan", "9876543247", "Hindu", 37, "KG1", "S037", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 38, "103850389038", "381 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Indore", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 3, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Aadhya", "Female", false, true, "Diya Mishra", "9876543248", "Muslim", 38, "KG1", "S038", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 39, "103950399039", "391 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Bhopal", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Dev", "Male", false, false, "Ishaan Chopra", "9876543249", "Christian", 39, "KG1", "S039", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 40, "104050409040", "401 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Patna", "A", 2, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Krisha", "Female", true, true, "Kavya Bansal", "9876543250", "Sikh", 40, "KG1", "S040", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, "104150419041", "411 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Vadodara", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Arnav", "Male", false, true, "Karan Agrawal", "9876543251", "Jain", 41, "KG2", "S041", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 42, "104250429042", "421 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Ludhiana", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Janvi", "Female", false, false, "Myra Malhotra", "9876543252", "Buddhist", 42, "KG2", "S042", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 43, "104350439043", "431 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Agra", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Yug", "Male", false, true, "Riya Kapoor", "9876543253", "Hindu", 43, "KG2", "S043", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 44, "104450449044", "441 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Nashik", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Shanaya", "Female", false, true, "Vihaan Mittal", "9876543254", "Muslim", 44, "KG2", "S044", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 45, "104550459045", "451 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Faridabad", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Arjun", "Male", true, false, "Saanvi Joshi", "9876543255", "Christian", 45, "KG2", "S045", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 46, "104650469046", "461 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Meerut", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Anika", "Female", false, true, "Reyansh Saxena", "9876543256", "Sikh", 46, "KG2", "S046", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 47, "104750479047", "471 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Rajkot", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Vedant", "Male", false, true, "Anvi Srivastava", "9876543257", "Jain", 47, "KG2", "S047", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 48, "104850489048", "481 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kalyan", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Reet", "Female", false, false, "Vivaan Pandey", "9876543258", "Buddhist", 48, "KG2", "S048", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 49, "104950499049", "491 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Vasai", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Aarush", "Male", false, true, "Aadya Tiwari", "9876543259", "Hindu", 49, "KG2", "S049", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 50, "105050509050", "501 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Varanasi", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Khushi", "Female", true, true, "Aayan Dubey", "9876543260", "Muslim", 50, "KG2", "S050", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 51, "105150519051", "511 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Mumbai", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 4, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Aarav", "Male", false, false, "Pihu Sharma", "9876543261", "Christian", 51, "KG2", "S051", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 52, "105250529052", "521 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Delhi", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ananya", "Female", false, true, "Krishna Patel", "9876543262", "Sikh", 52, "KG2", "S052", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 53, "105350539053", "531 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Pune", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 6, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Arjun", "Male", false, true, "Advik Singh", "9876543263", "Jain", 53, "KG2", "S053", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 54, "105450549054", "541 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Bangalore", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Diya", "Female", false, false, "Samaira Kumar", "9876543264", "Buddhist", 54, "KG2", "S054", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 55, "105550559055", "551 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Chennai", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Ishaan", "Male", true, true, "Kiaan Gupta", "9876543265", "Hindu", 55, "KG2", "S055", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 56, "105650569056", "561 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kolkata", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Kavya", "Female", false, true, "Avni Agarwal", "9876543266", "Muslim", 56, "KG2", "S056", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 57, "105750579057", "571 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Hyderabad", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Karan", "Male", false, false, "Aryan Verma", "9876543267", "Christian", 57, "KG2", "S057", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 58, "105850589058", "581 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Ahmedabad", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Myra", "Female", false, true, "Pari Jain", "9876543268", "Sikh", 58, "KG2", "S058", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 59, "105950599059", "591 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Jaipur", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 12, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Riya", "Male", false, true, "Sai Shah", "9876543269", "Jain", 59, "KG2", "S059", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 60, "106050609060", "601 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Lucknow", "A", 3, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Vihaan", "Female", true, false, "Atharv Yadav", "9876543270", "Buddhist", 60, "KG2", "S060", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 61, "106150619061", "611 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Kanpur", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Saanvi", "Male", false, true, "Ira Reddy", "9876543271", "Hindu", 61, "1st", "S061", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 62, "106250629062", "621 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Nagpur", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 3, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Reyansh", "Female", false, true, "Ayaan Khan", "9876543272", "Muslim", 62, "1st", "S062", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 63, "106350639063", "631 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Indore", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Anvi", "Male", false, false, "Navya Mishra", "9876543273", "Christian", 63, "1st", "S063", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 64, "106450649064", "641 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Bhopal", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Vivaan", "Female", false, true, "Rudra Chopra", "9876543274", "Sikh", 64, "1st", "S064", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 65, "106550659065", "651 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Patna", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Aadya", "Male", true, true, "Anaya Bansal", "9876543275", "Jain", 65, "1st", "S065", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 66, "106650669066", "661 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Vadodara", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 7, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Aayan", "Female", false, false, "Om Agrawal", "9876543276", "Buddhist", 66, "1st", "S066", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 67, "106750679067", "671 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Ludhiana", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Pihu", "Male", false, true, "Tara Malhotra", "9876543277", "Hindu", 67, "1st", "S067", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 68, "106850689068", "681 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Agra", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Krishna", "Female", false, true, "Shaurya Kapoor", "9876543278", "Muslim", 68, "1st", "S068", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 69, "106950699069", "691 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Nashik", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Advik", "Male", false, false, "Kashvi Mittal", "9876543279", "Christian", 69, "1st", "S069", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 70, "107050709070", "701 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Faridabad", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Samaira", "Female", true, true, "Mihir Joshi", "9876543280", "Sikh", 70, "1st", "S070", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 71, "107150719071", "711 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Meerut", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Kiaan", "Male", false, true, "Prisha Saxena", "9876543281", "Jain", 71, "1st", "S071", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 72, "107250729072", "721 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Rajkot", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Avni", "Female", false, false, "Aadhya Srivastava", "9876543282", "Buddhist", 72, "1st", "S072", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 73, "107350739073", "731 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Kalyan", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Aryan", "Male", false, true, "Dev Pandey", "9876543283", "Hindu", 73, "1st", "S073", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 74, "107450749074", "741 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Vasai", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Pari", "Female", false, true, "Krisha Tiwari", "9876543284", "Muslim", 74, "1st", "S074", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 75, "107550759075", "751 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Varanasi", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Sai", "Male", true, false, "Arnav Dubey", "9876543285", "Christian", 75, "1st", "S075", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 76, "107650769076", "761 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Mumbai", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Atharv", "Female", false, true, "Janvi Sharma", "9876543286", "Sikh", 76, "1st", "S076", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 77, "107750779077", "771 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Delhi", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ira", "Male", false, true, "Yug Patel", "9876543287", "Jain", 77, "1st", "S077", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 78, "107850789078", "781 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Pune", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Ayaan", "Female", false, false, "Shanaya Singh", "9876543288", "Buddhist", 78, "1st", "S078", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 79, "107950799079", "791 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Bangalore", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Navya", "Male", false, true, "Arjun Kumar", "9876543289", "Hindu", 79, "1st", "S079", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 80, "108050809080", "801 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Chennai", "A", 4, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Rudra", "Female", true, true, "Anika Gupta", "9876543290", "Muslim", 80, "1st", "S080", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 81, "108150819081", "811 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Kolkata", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Anaya", "Male", false, false, "Vedant Agarwal", "9876543291", "Christian", 81, "2nd", "S081", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 82, "108250829082", "821 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Hyderabad", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 11, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Om", "Female", false, true, "Reet Verma", "9876543292", "Sikh", 82, "2nd", "S082", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 83, "108350839083", "831 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Ahmedabad", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Tara", "Male", false, true, "Aarush Jain", "9876543293", "Jain", 83, "2nd", "S083", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 84, "108450849084", "841 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Jaipur", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Shaurya", "Female", false, false, "Khushi Shah", "9876543294", "Buddhist", 84, "2nd", "S084", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 85, "108550859085", "851 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Lucknow", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Kashvi", "Male", true, true, "Aarav Yadav", "9876543295", "Hindu", 85, "2nd", "S085", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 86, "108650869086", "861 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Kanpur", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Mihir", "Female", false, true, "Ananya Reddy", "9876543296", "Muslim", 86, "2nd", "S086", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 87, "108750879087", "871 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Nagpur", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Prisha", "Male", false, false, "Arjun Khan", "9876543297", "Christian", 87, "2nd", "S087", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 88, "108850889088", "881 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Indore", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Aadhya", "Female", false, true, "Diya Mishra", "9876543298", "Sikh", 88, "2nd", "S088", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 89, "108950899089", "891 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Bhopal", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Dev", "Male", false, true, "Ishaan Chopra", "9876543299", "Jain", 89, "2nd", "S089", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 90, "109050909090", "901 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Patna", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Krisha", "Female", true, false, "Kavya Bansal", "9876543300", "Buddhist", 90, "2nd", "S090", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 91, "109150919091", "911 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Vadodara", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Arnav", "Male", false, true, "Karan Agrawal", "9876543301", "Hindu", 91, "2nd", "S091", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 92, "109250929092", "921 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Ludhiana", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Janvi", "Female", false, true, "Myra Malhotra", "9876543302", "Muslim", 92, "2nd", "S092", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 93, "109350939093", "931 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Agra", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Yug", "Male", false, false, "Riya Kapoor", "9876543303", "Christian", 93, "2nd", "S093", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 94, "109450949094", "941 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Nashik", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Shanaya", "Female", false, true, "Vihaan Mittal", "9876543304", "Sikh", 94, "2nd", "S094", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 95, "109550959095", "951 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Faridabad", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Arjun", "Male", true, true, "Saanvi Joshi", "9876543305", "Jain", 95, "2nd", "S095", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 96, "109650969096", "961 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Meerut", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Anika", "Female", false, false, "Reyansh Saxena", "9876543306", "Buddhist", 96, "2nd", "S096", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 97, "109750979097", "971 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Rajkot", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Vedant", "Male", false, true, "Anvi Srivastava", "9876543307", "Hindu", 97, "2nd", "S097", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 98, "109850989098", "981 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Kalyan", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Reet", "Female", false, true, "Vivaan Pandey", "9876543308", "Muslim", 98, "2nd", "S098", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 99, "109950999099", "991 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Vasai", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Aarush", "Male", false, false, "Aadya Tiwari", "9876543309", "Christian", 99, "2nd", "S099", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 100, "110051009100", "1001 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Varanasi", "A", 5, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Khushi", "Female", true, true, "Aayan Dubey", "9876543310", "Sikh", 100, "2nd", "S100", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 101, "110151019101", "1011 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Mumbai", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Aarav", "Male", false, true, "Pihu Sharma", "9876543311", "Jain", 101, "3rd", "S101", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 102, "110251029102", "1021 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Delhi", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ananya", "Female", false, false, "Krishna Patel", "9876543312", "Buddhist", 102, "3rd", "S102", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 103, "110351039103", "1031 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Pune", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Arjun", "Male", false, true, "Advik Singh", "9876543313", "Hindu", 103, "3rd", "S103", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 104, "110451049104", "1041 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Bangalore", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Diya", "Female", false, true, "Samaira Kumar", "9876543314", "Muslim", 104, "3rd", "S104", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 105, "110551059105", "1051 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Chennai", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Ishaan", "Male", true, false, "Kiaan Gupta", "9876543315", "Christian", 105, "3rd", "S105", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 106, "110651069106", "1061 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Kolkata", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Kavya", "Female", false, true, "Avni Agarwal", "9876543316", "Sikh", 106, "3rd", "S106", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 107, "110751079107", "1071 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Hyderabad", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Karan", "Male", false, true, "Aryan Verma", "9876543317", "Jain", 107, "3rd", "S107", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 108, "110851089108", "1081 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Ahmedabad", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Myra", "Female", false, false, "Pari Jain", "9876543318", "Buddhist", 108, "3rd", "S108", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 109, "110951099109", "1091 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Jaipur", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Riya", "Male", false, true, "Sai Shah", "9876543319", "Hindu", 109, "3rd", "S109", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 110, "111051109110", "1101 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Lucknow", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Vihaan", "Female", true, true, "Atharv Yadav", "9876543320", "Muslim", 110, "3rd", "S110", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 111, "111151119111", "1111 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kanpur", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Saanvi", "Male", false, false, "Ira Reddy", "9876543321", "Christian", 111, "3rd", "S111", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 112, "111251129112", "1121 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Nagpur", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Reyansh", "Female", false, true, "Ayaan Khan", "9876543322", "Sikh", 112, "3rd", "S112", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 113, "111351139113", "1131 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Indore", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Anvi", "Male", false, true, "Navya Mishra", "9876543323", "Jain", 113, "3rd", "S113", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 114, "111451149114", "1141 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Bhopal", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 7, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Vivaan", "Female", false, false, "Rudra Chopra", "9876543324", "Buddhist", 114, "3rd", "S114", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 115, "111551159115", "1151 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Patna", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Aadya", "Male", true, true, "Anaya Bansal", "9876543325", "Hindu", 115, "3rd", "S115", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 116, "111651169116", "1161 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Vadodara", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Aayan", "Female", false, true, "Om Agrawal", "9876543326", "Muslim", 116, "3rd", "S116", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 117, "111751179117", "1171 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Ludhiana", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Pihu", "Male", false, false, "Tara Malhotra", "9876543327", "Christian", 117, "3rd", "S117", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 118, "111851189118", "1181 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Agra", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Krishna", "Female", false, true, "Shaurya Kapoor", "9876543328", "Sikh", 118, "3rd", "S118", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 119, "111951199119", "1191 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Nashik", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Advik", "Male", false, true, "Kashvi Mittal", "9876543329", "Jain", 119, "3rd", "S119", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 120, "112051209120", "1201 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Faridabad", "A", 6, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Samaira", "Female", true, false, "Mihir Joshi", "9876543330", "Buddhist", 120, "3rd", "S120", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 121, "112151219121", "1211 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Meerut", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Kiaan", "Male", false, true, "Prisha Saxena", "9876543331", "Hindu", 121, "4th", "S121", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 122, "112251229122", "1221 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Rajkot", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 3, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Avni", "Female", false, true, "Aadhya Srivastava", "9876543332", "Muslim", 122, "4th", "S122", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 123, "112351239123", "1231 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kalyan", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Aryan", "Male", false, false, "Dev Pandey", "9876543333", "Christian", 123, "4th", "S123", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 124, "112451249124", "1241 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Vasai", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Pari", "Female", false, true, "Krisha Tiwari", "9876543334", "Sikh", 124, "4th", "S124", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 125, "112551259125", "1251 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Varanasi", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Sai", "Male", true, true, "Arnav Dubey", "9876543335", "Jain", 125, "4th", "S125", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 126, "112651269126", "1261 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Mumbai", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Atharv", "Female", false, false, "Janvi Sharma", "9876543336", "Buddhist", 126, "4th", "S126", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 127, "112751279127", "1271 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Delhi", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ira", "Male", false, true, "Yug Patel", "9876543337", "Hindu", 127, "4th", "S127", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 128, "112851289128", "1281 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Pune", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Ayaan", "Female", false, true, "Shanaya Singh", "9876543338", "Muslim", 128, "4th", "S128", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 129, "112951299129", "1291 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Bangalore", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Navya", "Male", false, false, "Arjun Kumar", "9876543339", "Christian", 129, "4th", "S129", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 130, "113051309130", "1301 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Chennai", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Rudra", "Female", true, true, "Anika Gupta", "9876543340", "Sikh", 130, "4th", "S130", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 131, "113151319131", "1311 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kolkata", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Anaya", "Male", false, true, "Vedant Agarwal", "9876543341", "Jain", 131, "4th", "S131", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 132, "113251329132", "1321 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Hyderabad", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Om", "Female", false, false, "Reet Verma", "9876543342", "Buddhist", 132, "4th", "S132", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 133, "113351339133", "1331 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Ahmedabad", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Tara", "Male", false, true, "Aarush Jain", "9876543343", "Hindu", 133, "4th", "S133", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 134, "113451349134", "1341 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Jaipur", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Shaurya", "Female", false, true, "Khushi Shah", "9876543344", "Muslim", 134, "4th", "S134", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 135, "113551359135", "1351 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Lucknow", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 4, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Kashvi", "Male", true, false, "Aarav Yadav", "9876543345", "Christian", 135, "4th", "S135", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 136, "113651369136", "1361 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kanpur", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Mihir", "Female", false, true, "Ananya Reddy", "9876543346", "Sikh", 136, "4th", "S136", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 137, "113751379137", "1371 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Nagpur", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 6, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Prisha", "Male", false, true, "Arjun Khan", "9876543347", "Jain", 137, "4th", "S137", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 138, "113851389138", "1381 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Indore", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Aadhya", "Female", false, false, "Diya Mishra", "9876543348", "Buddhist", 138, "4th", "S138", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 139, "113951399139", "1391 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Bhopal", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Dev", "Male", false, true, "Ishaan Chopra", "9876543349", "Hindu", 139, "4th", "S139", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 140, "114051409140", "1401 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Patna", "A", 7, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Krisha", "Female", true, true, "Kavya Bansal", "9876543350", "Muslim", 140, "4th", "S140", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 141, "114151419141", "1411 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Vadodara", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Arnav", "Male", false, false, "Karan Agrawal", "9876543351", "Christian", 141, "5th", "S141", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 142, "114251429142", "1421 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Ludhiana", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Janvi", "Female", false, true, "Myra Malhotra", "9876543352", "Sikh", 142, "5th", "S142", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 143, "114351439143", "1431 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Agra", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 12, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Yug", "Male", false, true, "Riya Kapoor", "9876543353", "Jain", 143, "5th", "S143", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 144, "114451449144", "1441 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Nashik", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Shanaya", "Female", false, false, "Vihaan Mittal", "9876543354", "Buddhist", 144, "5th", "S144", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 145, "114551459145", "1451 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Faridabad", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Arjun", "Male", true, true, "Saanvi Joshi", "9876543355", "Hindu", 145, "5th", "S145", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 146, "114651469146", "1461 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Meerut", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 3, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Anika", "Female", false, true, "Reyansh Saxena", "9876543356", "Muslim", 146, "5th", "S146", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 147, "114751479147", "1471 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Rajkot", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Vedant", "Male", false, false, "Anvi Srivastava", "9876543357", "Christian", 147, "5th", "S147", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 148, "114851489148", "1481 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kalyan", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Reet", "Female", false, true, "Vivaan Pandey", "9876543358", "Sikh", 148, "5th", "S148", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 149, "114951499149", "1491 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Vasai", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Aarush", "Male", false, true, "Aadya Tiwari", "9876543359", "Jain", 149, "5th", "S149", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 150, "115051509150", "1501 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Varanasi", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 7, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Khushi", "Female", true, false, "Aayan Dubey", "9876543360", "Buddhist", 150, "5th", "S150", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 151, "115151519151", "1511 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Mumbai", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Aarav", "Male", false, true, "Pihu Sharma", "9876543361", "Hindu", 151, "5th", "S151", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 152, "115251529152", "1521 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Delhi", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ananya", "Female", false, true, "Krishna Patel", "9876543362", "Muslim", 152, "5th", "S152", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 153, "115351539153", "1531 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Pune", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Arjun", "Male", false, false, "Advik Singh", "9876543363", "Christian", 153, "5th", "S153", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 154, "115451549154", "1541 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Bangalore", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Diya", "Female", false, true, "Samaira Kumar", "9876543364", "Sikh", 154, "5th", "S154", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 155, "115551559155", "1551 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Chennai", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Ishaan", "Male", true, true, "Kiaan Gupta", "9876543365", "Jain", 155, "5th", "S155", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 156, "115651569156", "1561 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kolkata", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Kavya", "Female", false, false, "Avni Agarwal", "9876543366", "Buddhist", 156, "5th", "S156", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 157, "115751579157", "1571 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Hyderabad", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Karan", "Male", false, true, "Aryan Verma", "9876543367", "Hindu", 157, "5th", "S157", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 158, "115851589158", "1581 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Ahmedabad", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Myra", "Female", false, true, "Pari Jain", "9876543368", "Muslim", 158, "5th", "S158", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 159, "115951599159", "1591 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Jaipur", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Riya", "Male", false, false, "Sai Shah", "9876543369", "Christian", 159, "5th", "S159", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 160, "116051609160", "1601 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Lucknow", "A", 8, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2014, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Vihaan", "Female", true, true, "Atharv Yadav", "9876543370", "Sikh", 160, "5th", "S160", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 161, "116151619161", "1611 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Kanpur", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Saanvi", "Male", false, true, "Ira Reddy", "9876543371", "Jain", 161, "6th", "S161", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 162, "116251629162", "1621 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Nagpur", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Reyansh", "Female", false, false, "Ayaan Khan", "9876543372", "Buddhist", 162, "6th", "S162", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 163, "116351639163", "1631 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Indore", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Anvi", "Male", false, true, "Navya Mishra", "9876543373", "Hindu", 163, "6th", "S163", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 164, "116451649164", "1641 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Bhopal", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Vivaan", "Female", false, true, "Rudra Chopra", "9876543374", "Muslim", 164, "6th", "S164", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 165, "116551659165", "1651 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Patna", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Aadya", "Male", true, false, "Anaya Bansal", "9876543375", "Christian", 165, "6th", "S165", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 166, "116651669166", "1661 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Vadodara", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 11, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Aayan", "Female", false, true, "Om Agrawal", "9876543376", "Sikh", 166, "6th", "S166", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 167, "116751679167", "1671 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Ludhiana", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Pihu", "Male", false, true, "Tara Malhotra", "9876543377", "Jain", 167, "6th", "S167", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 168, "116851689168", "1681 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Agra", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Krishna", "Female", false, false, "Shaurya Kapoor", "9876543378", "Buddhist", 168, "6th", "S168", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 169, "116951699169", "1691 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Nashik", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Advik", "Male", false, true, "Kashvi Mittal", "9876543379", "Hindu", 169, "6th", "S169", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 170, "117051709170", "1701 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Faridabad", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Samaira", "Female", true, true, "Mihir Joshi", "9876543380", "Muslim", 170, "6th", "S170", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 171, "117151719171", "1711 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Meerut", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Kiaan", "Male", false, false, "Prisha Saxena", "9876543381", "Christian", 171, "6th", "S171", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 172, "117251729172", "1721 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Rajkot", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Avni", "Female", false, true, "Aadhya Srivastava", "9876543382", "Sikh", 172, "6th", "S172", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 173, "117351739173", "1731 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Kalyan", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Aryan", "Male", false, true, "Dev Pandey", "9876543383", "Jain", 173, "6th", "S173", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 174, "117451749174", "1741 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Vasai", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Pari", "Female", false, false, "Krisha Tiwari", "9876543384", "Buddhist", 174, "6th", "S174", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 175, "117551759175", "1751 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Varanasi", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Sai", "Male", true, true, "Arnav Dubey", "9876543385", "Hindu", 175, "6th", "S175", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 176, "117651769176", "1761 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Mumbai", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Atharv", "Female", false, true, "Janvi Sharma", "9876543386", "Muslim", 176, "6th", "S176", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 177, "117751779177", "1771 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Delhi", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ira", "Male", false, false, "Yug Patel", "9876543387", "Christian", 177, "6th", "S177", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 178, "117851789178", "1781 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Pune", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Ayaan", "Female", false, true, "Shanaya Singh", "9876543388", "Sikh", 178, "6th", "S178", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 179, "117951799179", "1791 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Bangalore", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Navya", "Male", false, true, "Arjun Kumar", "9876543389", "Jain", 179, "6th", "S179", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 180, "118051809180", "1801 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Chennai", "A", 9, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2013, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Rudra", "Female", true, false, "Anika Gupta", "9876543390", "Buddhist", 180, "6th", "S180", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 181, "118151819181", "1811 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Kolkata", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Anaya", "Male", false, true, "Vedant Agarwal", "9876543391", "Hindu", 181, "7th", "S181", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 182, "118251829182", "1821 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Hyderabad", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Om", "Female", false, true, "Reet Verma", "9876543392", "Muslim", 182, "7th", "S182", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 183, "118351839183", "1831 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Ahmedabad", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Tara", "Male", false, false, "Aarush Jain", "9876543393", "Christian", 183, "7th", "S183", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 184, "118451849184", "1841 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Jaipur", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Shaurya", "Female", false, true, "Khushi Shah", "9876543394", "Sikh", 184, "7th", "S184", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 185, "118551859185", "1851 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Lucknow", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Kashvi", "Male", true, true, "Aarav Yadav", "9876543395", "Jain", 185, "7th", "S185", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 186, "118651869186", "1861 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Kanpur", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Mihir", "Female", false, false, "Ananya Reddy", "9876543396", "Buddhist", 186, "7th", "S186", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 187, "118751879187", "1871 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Nagpur", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Prisha", "Male", false, true, "Arjun Khan", "9876543397", "Hindu", 187, "7th", "S187", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 188, "118851889188", "1881 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Indore", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Aadhya", "Female", false, true, "Diya Mishra", "9876543398", "Muslim", 188, "7th", "S188", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 189, "118951899189", "1891 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Bhopal", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Dev", "Male", false, false, "Ishaan Chopra", "9876543399", "Christian", 189, "7th", "S189", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 190, "119051909190", "1901 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Patna", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Krisha", "Female", true, true, "Kavya Bansal", "9876543400", "Sikh", 190, "7th", "S190", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 191, "119151919191", "1911 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Vadodara", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Arnav", "Male", false, true, "Karan Agrawal", "9876543401", "Jain", 191, "7th", "S191", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 192, "119251929192", "1921 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Ludhiana", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Janvi", "Female", false, false, "Myra Malhotra", "9876543402", "Buddhist", 192, "7th", "S192", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 193, "119351939193", "1931 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Agra", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Yug", "Male", false, true, "Riya Kapoor", "9876543403", "Hindu", 193, "7th", "S193", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 194, "119451949194", "1941 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Nashik", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Shanaya", "Female", false, true, "Vihaan Mittal", "9876543404", "Muslim", 194, "7th", "S194", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 195, "119551959195", "1951 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Faridabad", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Arjun", "Male", true, false, "Saanvi Joshi", "9876543405", "Christian", 195, "7th", "S195", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 196, "119651969196", "1961 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Meerut", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Anika", "Female", false, true, "Reyansh Saxena", "9876543406", "Sikh", 196, "7th", "S196", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 197, "119751979197", "1971 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Rajkot", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 6, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Vedant", "Male", false, true, "Anvi Srivastava", "9876543407", "Jain", 197, "7th", "S197", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 198, "119851989198", "1981 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Kalyan", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 7, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Reet", "Female", false, false, "Vivaan Pandey", "9876543408", "Buddhist", 198, "7th", "S198", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 199, "119951999199", "1991 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Vasai", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Aarush", "Male", false, true, "Aadya Tiwari", "9876543409", "Hindu", 199, "7th", "S199", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 200, "120052009200", "2001 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Varanasi", "A", 10, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Khushi", "Female", true, true, "Aayan Dubey", "9876543410", "Muslim", 200, "7th", "S200", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 201, "120152019201", "2011 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Mumbai", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Aarav", "Male", false, false, "Pihu Sharma", "9876543411", "Christian", 201, "8th", "S201", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 202, "120252029202", "2021 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Delhi", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ananya", "Female", false, true, "Krishna Patel", "9876543412", "Sikh", 202, "8th", "S202", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 203, "120352039203", "2031 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Pune", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Arjun", "Male", false, true, "Advik Singh", "9876543413", "Jain", 203, "8th", "S203", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 204, "120452049204", "2041 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Bangalore", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Diya", "Female", false, false, "Samaira Kumar", "9876543414", "Buddhist", 204, "8th", "S204", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 205, "120552059205", "2051 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Chennai", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Ishaan", "Male", true, true, "Kiaan Gupta", "9876543415", "Hindu", 205, "8th", "S205", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 206, "120652069206", "2061 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Kolkata", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 3, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Kavya", "Female", false, true, "Avni Agarwal", "9876543416", "Muslim", 206, "8th", "S206", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 207, "120752079207", "2071 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Hyderabad", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Karan", "Male", false, false, "Aryan Verma", "9876543417", "Christian", 207, "8th", "S207", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 208, "120852089208", "2081 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Ahmedabad", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Myra", "Female", false, true, "Pari Jain", "9876543418", "Sikh", 208, "8th", "S208", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 209, "120952099209", "2091 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Jaipur", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Riya", "Male", false, true, "Sai Shah", "9876543419", "Jain", 209, "8th", "S209", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 210, "121052109210", "2101 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Lucknow", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Vihaan", "Female", true, false, "Atharv Yadav", "9876543420", "Buddhist", 210, "8th", "S210", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 211, "121152119211", "2111 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kanpur", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Saanvi", "Male", false, true, "Ira Reddy", "9876543421", "Hindu", 211, "8th", "S211", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 212, "121252129212", "2121 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Nagpur", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Reyansh", "Female", false, true, "Ayaan Khan", "9876543422", "Muslim", 212, "8th", "S212", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 213, "121352139213", "2131 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Indore", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Anvi", "Male", false, false, "Navya Mishra", "9876543423", "Christian", 213, "8th", "S213", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 214, "121452149214", "2141 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Bhopal", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Vivaan", "Female", false, true, "Rudra Chopra", "9876543424", "Sikh", 214, "8th", "S214", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 215, "121552159215", "2151 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Patna", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Aadya", "Male", true, true, "Anaya Bansal", "9876543425", "Jain", 215, "8th", "S215", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 216, "121652169216", "2161 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Vadodara", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Aayan", "Female", false, false, "Om Agrawal", "9876543426", "Buddhist", 216, "8th", "S216", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 217, "121752179217", "2171 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Ludhiana", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Pihu", "Male", false, true, "Tara Malhotra", "9876543427", "Hindu", 217, "8th", "S217", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 218, "121852189218", "2181 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Agra", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Krishna", "Female", false, true, "Shaurya Kapoor", "9876543428", "Muslim", 218, "8th", "S218", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 219, "121952199219", "2191 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Nashik", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 4, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Advik", "Male", false, false, "Kashvi Mittal", "9876543429", "Christian", 219, "8th", "S219", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 220, "122052209220", "2201 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Faridabad", "A", 11, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Samaira", "Female", true, true, "Mihir Joshi", "9876543430", "Sikh", 220, "8th", "S220", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 221, "122152219221", "2211 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Meerut", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 6, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Kiaan", "Male", false, true, "Prisha Saxena", "9876543431", "Jain", 221, "9th", "S221", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 222, "122252229222", "2221 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Rajkot", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Avni", "Female", false, false, "Aadhya Srivastava", "9876543432", "Buddhist", 222, "9th", "S222", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 223, "122352239223", "2231 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kalyan", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Aryan", "Male", false, true, "Dev Pandey", "9876543433", "Hindu", 223, "9th", "S223", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 224, "122452249224", "2241 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Vasai", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Pari", "Female", false, true, "Krisha Tiwari", "9876543434", "Muslim", 224, "9th", "S224", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 225, "122552259225", "2251 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Varanasi", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Sai", "Male", true, false, "Arnav Dubey", "9876543435", "Christian", 225, "9th", "S225", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 226, "122652269226", "2261 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Mumbai", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Atharv", "Female", false, true, "Janvi Sharma", "9876543436", "Sikh", 226, "9th", "S226", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 227, "122752279227", "2271 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Delhi", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 12, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ira", "Male", false, true, "Yug Patel", "9876543437", "Jain", 227, "9th", "S227", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 228, "122852289228", "2281 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Pune", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Ayaan", "Female", false, false, "Shanaya Singh", "9876543438", "Buddhist", 228, "9th", "S228", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 229, "122952299229", "2291 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Bangalore", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Navya", "Male", false, true, "Arjun Kumar", "9876543439", "Hindu", 229, "9th", "S229", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 230, "123052309230", "2301 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Chennai", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 3, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Rudra", "Female", true, true, "Anika Gupta", "9876543440", "Muslim", 230, "9th", "S230", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 231, "123152319231", "2311 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Kolkata", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Anaya", "Male", false, false, "Vedant Agarwal", "9876543441", "Christian", 231, "9th", "S231", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 232, "123252329232", "2321 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Hyderabad", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Om", "Female", false, true, "Reet Verma", "9876543442", "Sikh", 232, "9th", "S232", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 233, "123352339233", "2331 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Ahmedabad", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Tara", "Male", false, true, "Aarush Jain", "9876543443", "Jain", 233, "9th", "S233", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 234, "123452349234", "2341 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Jaipur", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 7, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Shaurya", "Female", false, false, "Khushi Shah", "9876543444", "Buddhist", 234, "9th", "S234", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 235, "123552359235", "2351 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Lucknow", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Kashvi", "Male", true, true, "Aarav Yadav", "9876543445", "Hindu", 235, "9th", "S235", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 236, "123652369236", "2361 Test Street, Kanpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kanpur", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Srivastava Malhotra", "Mihir", "Female", false, true, "Ananya Reddy", "9876543446", "Muslim", 236, "9th", "S236", "Reddy", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 237, "123752379237", "2371 Test Street, Nagpur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Nagpur", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pandey Kapoor", "Prisha", "Male", false, false, "Arjun Khan", "9876543447", "Christian", 237, "9th", "S237", "Khan", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 238, "123852389238", "2381 Test Street, Indore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Indore", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tiwari Mittal", "Aadhya", "Female", false, true, "Diya Mishra", "9876543448", "Sikh", 238, "9th", "S238", "Mishra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 239, "123952399239", "2391 Test Street, Bhopal", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Bhopal", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dubey Joshi", "Dev", "Male", false, true, "Ishaan Chopra", "9876543449", "Jain", 239, "9th", "S239", "Chopra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 240, "124052409240", "2401 Test Street, Patna", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Patna", "A", 12, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sharma Saxena", "Krisha", "Female", true, false, "Kavya Bansal", "9876543450", "Buddhist", 240, "9th", "S240", "Bansal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 241, "124152419241", "2411 Test Street, Vadodara", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Vadodara", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Patel Srivastava", "Arnav", "Male", false, true, "Karan Agrawal", "9876543451", "Hindu", 241, "10th", "S241", "Agrawal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 242, "124252429242", "2421 Test Street, Ludhiana", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Ludhiana", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Singh Pandey", "Janvi", "Female", false, true, "Myra Malhotra", "9876543452", "Muslim", 242, "10th", "S242", "Malhotra", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 243, "124352439243", "2431 Test Street, Agra", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Agra", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kumar Tiwari", "Yug", "Male", false, false, "Riya Kapoor", "9876543453", "Christian", 243, "10th", "S243", "Kapoor", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 244, "124452449244", "2441 Test Street, Nashik", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Nashik", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gupta Dubey", "Shanaya", "Female", false, true, "Vihaan Mittal", "9876543454", "Sikh", 244, "10th", "S244", "Mittal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 245, "124552459245", "2451 Test Street, Faridabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Faridabad", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agarwal Sharma", "Arjun", "Male", true, true, "Saanvi Joshi", "9876543455", "Jain", 245, "10th", "S245", "Joshi", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 246, "124652469246", "2461 Test Street, Meerut", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Meerut", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verma Patel", "Anika", "Female", false, false, "Reyansh Saxena", "9876543456", "Buddhist", 246, "10th", "S246", "Saxena", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 247, "124752479247", "2471 Test Street, Rajkot", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Rajkot", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jain Singh", "Vedant", "Male", false, true, "Anvi Srivastava", "9876543457", "Hindu", 247, "10th", "S247", "Srivastava", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 248, "124852489248", "2481 Test Street, Kalyan", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kalyan", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shah Kumar", "Reet", "Female", false, true, "Vivaan Pandey", "9876543458", "Muslim", 248, "10th", "S248", "Pandey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 249, "124952499249", "2491 Test Street, Vasai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Vasai", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yadav Gupta", "Aarush", "Male", false, false, "Aadya Tiwari", "9876543459", "Christian", 249, "10th", "S249", "Tiwari", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 250, "125052509250", "2501 Test Street, Varanasi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Varanasi", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 11, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Reddy Agarwal", "Khushi", "Female", true, true, "Aayan Dubey", "9876543460", "Sikh", 250, "10th", "S250", "Dubey", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 251, "125152519251", "2511 Test Street, Mumbai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Mumbai", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khan Verma", "Aarav", "Male", false, true, "Pihu Sharma", "9876543461", "Jain", 251, "10th", "S251", "Sharma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 252, "125252529252", "2521 Test Street, Delhi", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Delhi", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mishra Jain", "Ananya", "Female", false, false, "Krishna Patel", "9876543462", "Buddhist", 252, "10th", "S252", "Patel", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 253, "125352539253", "2531 Test Street, Pune", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Pune", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chopra Shah", "Arjun", "Male", false, true, "Advik Singh", "9876543463", "Hindu", 253, "10th", "S253", "Singh", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 254, "125452549254", "2541 Test Street, Bangalore", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Bangalore", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bansal Yadav", "Diya", "Female", false, true, "Samaira Kumar", "9876543464", "Muslim", 254, "10th", "S254", "Kumar", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 255, "125552559255", "2551 Test Street, Chennai", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Chennai", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agrawal Reddy", "Ishaan", "Male", true, false, "Kiaan Gupta", "9876543465", "Christian", 255, "10th", "S255", "Gupta", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 256, "125652569256", "2561 Test Street, Kolkata", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Kolkata", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malhotra Khan", "Kavya", "Female", false, true, "Avni Agarwal", "9876543466", "Sikh", 256, "10th", "S256", "Agarwal", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 257, "125752579257", "2571 Test Street, Hyderabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "General", "Hyderabad", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kapoor Mishra", "Karan", "Male", false, true, "Aryan Verma", "9876543467", "Jain", 257, "10th", "S257", "Verma", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 258, "125852589258", "2581 Test Street, Ahmedabad", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OBC", "Ahmedabad", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mittal Chopra", "Myra", "Female", false, false, "Pari Jain", "9876543468", "Buddhist", 258, "10th", "S258", "Jain", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 259, "125952599259", "2591 Test Street, Jaipur", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SC", "Jaipur", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joshi Bansal", "Riya", "Male", false, true, "Sai Shah", "9876543469", "Hindu", 259, "10th", "S259", "Shah", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 260, "126052609260", "2601 Test Street, Lucknow", null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ST", "Lucknow", "A", 13, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Saxena Agrawal", "Vihaan", "Female", true, true, "Atharv Yadav", "9876543470", "Muslim", 260, "10th", "S260", "Yadav", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "FeePayments",
                columns: new[] { "Id", "AcademicYearId", "AcademicYearString", "AmountPaid", "BankName", "ChequeNumber", "CreatedAt", "Discount", "FeeType", "GeneratedBy", "InstallmentNumber", "LateFee", "NextDueDate", "PaymentDate", "PaymentMethod", "PaymentNotes", "PreviousBalance", "ReceiptNumber", "RemainingBalance", "StudentId", "TransactionId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 3, "2024-25", 15000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0, "Admin", null, 0m, null, new DateTime(2024, 5, 12, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, 0m, "REC001", 45000m, 1, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 3, "2024-25", 20000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0, "Admin", null, 0m, null, new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 0m, "REC002", 40000m, 2, "TXN112233", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 3, "2024-25", 18000m, "HDFC Bank", "CH445566", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0, "Admin", null, 500m, null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 0m, "REC003", 42500m, 3, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 3, "2024-25", 10000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1000m, 0, "Admin", null, 0m, null, new DateTime(2024, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, 0m, "REC004", 49000m, 10, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 3, "2024-25", 12000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0, "Admin", null, 0m, null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 0m, "REC005", 48000m, 15, "TXN223344", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 3, "2024-25", 5000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 1, "Admin", null, 0m, null, new DateTime(2024, 5, 2, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, 0m, "REC006", 0m, 20, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 3, "2024-25", 4000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 3, "Admin", null, 0m, null, new DateTime(2024, 5, 10, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 0m, "REC007", 8000m, 25, "TXN334455", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 3, "2024-25", 2000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 2, "Admin", null, 0m, null, new DateTime(2024, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, 0m, "REC008", 0m, 30, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 3, "2024-25", 8000m, "SBI Bank", "CH556677", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 500m, 0, "Admin", null, 200m, null, new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Utc), 2, null, 0m, "REC009", 46700m, 35, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 3, "2024-25", 16000m, null, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0, "Admin", null, 0m, null, new DateTime(2024, 5, 29, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 0m, "REC010", 39000m, 40, "TXN445566", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_IsCurrent",
                table: "AcademicYears",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_Year",
                table: "AcademicYears",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectricityBills_BillMonth_BillYear",
                table: "ElectricityBills",
                columns: new[] { "BillMonth", "BillYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectricityBills_BillNumber",
                table: "ElectricityBills",
                column: "BillNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_AcademicYearId",
                table: "FeePayments",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_ReceiptNumber",
                table: "FeePayments",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeePayments_StudentId",
                table: "FeePayments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_AcademicYearId",
                table: "FeeStructures",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_ClassId_FeeType_AcademicYearId",
                table: "FeeStructures",
                columns: new[] { "ClassId", "FeeType", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_EmployeeId",
                table: "Staff",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentPromotionHistory_AcademicYearId",
                table: "StudentPromotionHistory",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPromotionHistory_FromClassId",
                table: "StudentPromotionHistory",
                column: "FromClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPromotionHistory_PromotionDate",
                table: "StudentPromotionHistory",
                column: "PromotionDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPromotionHistory_StudentId",
                table: "StudentPromotionHistory",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPromotionHistory_ToClassId",
                table: "StudentPromotionHistory",
                column: "ToClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_AdmissionAcademicYearId",
                table: "Students",
                column: "AdmissionAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SerialNo",
                table: "Students",
                column: "SerialNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_EmployeeId",
                table: "Teachers",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportExpenses_VehicleId",
                table: "TransportExpenses",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VehicleNumber",
                table: "Vehicles",
                column: "VehicleNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectricityBills");

            migrationBuilder.DropTable(
                name: "FeePayments");

            migrationBuilder.DropTable(
                name: "FeeStructures");

            migrationBuilder.DropTable(
                name: "OtherExpenses");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "StudentPromotionHistory");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TransportExpenses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
