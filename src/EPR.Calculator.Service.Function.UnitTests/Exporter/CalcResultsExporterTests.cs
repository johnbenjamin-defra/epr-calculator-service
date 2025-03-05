namespace EPR.Calculator.API.UnitTests.Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AutoFixture;
    using EPR.Calculator.API.Exporter;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Exporter.Detail;
    using EPR.Calculator.Service.Function.Exporter.CommsCost;
    using EPR.Calculator.Service.Function.Exporter.LaDisposalCost;
    using EPR.Calculator.Service.Function.Exporter.OtherCosts;
    using EPR.Calculator.Service.Function.Exporter.ScaledupProducers;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultsExporterTests
    {
        public CalcResultsExporterTests()
        {
            this.Fixture = new Fixture();
            this.MockLateReportingExporter = new();
            this.MockResultDetailexporter = new();
            this.MockOnePlusFourExporter = new();
            this.MockLaDisposalCostDataExporter = new();
            this.MockScaledupProducersExporter = new();
            this.MockLapcaptDetailExporter = new();
            this.MockParameterOtherCostExporter = new();
            this.MockCalcResultSummaryExporter = new();
            this.MockCommsCostExporter = new();
            this.TestClass = new CalcResultsExporter(
                this.MockLateReportingExporter.Object,
                this.MockResultDetailexporter.Object,
                this.MockOnePlusFourExporter.Object,
                this.MockLaDisposalCostDataExporter.Object,
                this.MockScaledupProducersExporter.Object,
                this.MockLapcaptDetailExporter.Object,
                this.MockParameterOtherCostExporter.Object,
                this.MockCommsCostExporter.Object,
                this.MockCalcResultSummaryExporter.Object);
        }

        private Mock<ICalcResultDetailExporter> mockResultDetailexporter = new();
        private Mock<IOnePlusFourApportionmentExporter> mockOnePlusFourExporter = new();
        private Mock<ICalcResultSummaryExporter> mockCalcResultSummaryExporter = new();
        private Fixture Fixture { get; init; }

        private Mock<ILateReportingExporter> MockLateReportingExporter { get; init; }

        private Mock<ICalcResultDetailExporter> MockResultDetailexporter { get; init; }

        private Mock<IOnePlusFourApportionmentExporter> MockOnePlusFourExporter { get; init; }

        private Mock<ICalcResultLaDisposalCostExporter> MockLaDisposalCostDataExporter { get; init; }

        private Mock<ICalcResultScaledupProducersExporter> MockScaledupProducersExporter { get; init; }

        private Mock<ILapcaptDetailExporter> MockLapcaptDetailExporter { get; init; }

        private Mock<ICalcResultParameterOtherCostExporter> MockParameterOtherCostExporter { get; init; }

        private Mock<ICalcResultSummaryExporter> MockCalcResultSummaryExporter { get; init; }

        private Mock<ICommsCostExporter> MockCommsCostExporter { get; init; }

        private CalcResultsExporter TestClass { get; init; }

        [TestMethod]
        public void Export_ShouldReturnCsvContent_WhenAllDataIsPresent()
        {
            // Arrange
            var calcResult = CreateCalcResult();

            // Act
            var result = this.TestClass.Export(calcResult);

            // Assert
            Assert.IsNotNull(result);

            this.MockLateReportingExporter.Verify(x => x.Export(calcResult.CalcResultLateReportingTonnageData));
            this.MockCalcResultSummaryExporter.Verify(x => x.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>()));
            this.MockLapcaptDetailExporter.Verify(x => x.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<StringBuilder>()));
            this.MockResultDetailexporter.Verify(x => x.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
            this.MockLaDisposalCostDataExporter.Verify(x => x.Export(It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>()));
            this.MockCommsCostExporter.Verify(x => x.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<StringBuilder>()));
            this.MockOnePlusFourExporter.Verify(x => x.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
            this.MockScaledupProducersExporter.Verify(x => x.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<StringBuilder>()));
            this.MockParameterOtherCostExporter.Verify(x => x.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
        }

        [TestMethod]
        public void AppendFileInfoTest()
        {
            var csvContent = new StringBuilder();
            CalcResultDetailexporter.AppendFileInfo(csvContent, "Label", "Filename,20/12/2024,User");
            Assert.IsTrue(csvContent.ToString().Contains("Label"));
            Assert.IsTrue(csvContent.ToString().Contains("Filename"));
            Assert.IsTrue(csvContent.ToString().Contains("20/12/2024"));
        }

        private static CalcResult CreateCalcResult()
        {
            return new CalcResult
            {
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = "LAPCAP Data",
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>
                    {
                        new()
                        {
                            Name = "Total",
                            EnglandDisposalCost = "£13,280.45",
                            WalesDisposalCost = "£210.28",
                            ScotlandDisposalCost = "£161.07",
                            NorthernIrelandDisposalCost = "£91.00",
                            TotalDisposalCost = "£13,742.80",
                            EnglandCost = 13280.45m,
                            WalesCost = 210.28m,
                            ScotlandCost = 91.00m,
                            NorthernIrelandCost = 91.00m,
                            TotalCost = 13742.80m,
                        },

                    },
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    Name = "Late Reporting Tonnage",
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>
                    {
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Aluminium",
                            TotalLateReportingTonnage = 8000.00m,
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Plastic",
                            TotalLateReportingTonnage = 2000.00m,
                        },
                        new CalcResultLateReportingTonnageDetail
                        {
                            Name = "Total",
                            TotalLateReportingTonnage = 10000.00m,
                        },
                    },
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    BadDebtValue = 6m,
                    Details = new List<CalcResultParameterOtherCostDetail>
                    {
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            OrderId = 1,
                            England = "£40.00",
                            EnglandValue = 40,
                            Wales = "£30.00",
                            WalesValue = 30,
                            Scotland = "£20.00",
                            ScotlandValue = 20,
                            NorthernIreland = "£10.00",
                            NorthernIrelandValue = 10,
                            Total = "£100.00",
                            TotalValue = 100,
                        },

                    },
                    Materiality = new List<CalcResultMateriality>
                    {
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality",
                        },
                    },
                    Name = "Parameters - Other",
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>
                    {
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 1,
                            England = "£40.00",
                            EnglandValue = 40,
                            Wales = "£30.00",
                            WalesValue = 30,
                            Scotland = "£20.00",
                            ScotlandValue = 20,
                            NorthernIreland = "£10.00",
                            NorthernIrelandValue = 10,
                            Total = "£100.00",
                            TotalValue = 100,
                        },
                    },
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail
                    {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100,
                    },
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
                    {
                        new CalcResultOnePlusFourApportionmentDetail
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 14.53M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 1.15M,
                            WalesTotal = 0.20M,
                            Name = "1 + 4 Apportionment %s",
                        },
                        new CalcResultOnePlusFourApportionmentDetail
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 0.20M,
                            Name = "Test",
                        },
                    },
                    Name = "some test",
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost
                {
                    CalcResultCommsCostCommsCostByMaterial = new List<CalcResultCommsCostCommsCostByMaterial>
                    {
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Aluminium",
                        },
                        new CalcResultCommsCostCommsCostByMaterial
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Glass",
                        },
                    },
                    CalcResultCommsCostOnePlusFourApportionment =
                        new Fixture().CreateMany<CalcResultCommsCostOnePlusFourApportionment>(1),
                    CommsCostByCountry = new Fixture().CreateMany<CalcResultCommsCostOnePlusFourApportionment>(1),
                },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
                    {
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne = "20",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "ScotlandTest",
                            Scotland = "ScotlandTest",
                            Material = "Material1",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "null",
                            ProducerReportedHouseholdPackagingWasteTonnage = "null",
                            ReportedPublicBinTonnage = string.Empty,
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne = "20",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "Material1",
                            Scotland = "ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "null",
                            ProducerReportedHouseholdPackagingWasteTonnage = "null",
                            ReportedPublicBinTonnage =string.Empty,
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne = "10",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "Material2",
                            Scotland = "ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = "100",
                            ProducerReportedHouseholdPackagingWasteTonnage = "null",
                            ReportedPublicBinTonnage = string.Empty,
                        },
                    },
                    Name = "LA Disposal Cost Data",
                },
                CalcResultScaledupProducers = new CalcResultScaledupProducers
                {
                    TitleHeader = new CalcResultScaledupProducerHeader
                    {
                        Name = "Scaled-up Producers",
                    },
                    MaterialBreakdownHeaders = new List<CalcResultScaledupProducerHeader>()
                    {
                        new CalcResultScaledupProducerHeader { Name = "Each submission for the year", ColumnIndex = 1 },
                        new CalcResultScaledupProducerHeader { Name = "Aluminium Breakdown", ColumnIndex = 2 },
                        new CalcResultScaledupProducerHeader { Name = "Glass Breakdown", ColumnIndex = 3 },
                    },
                    ColumnHeaders = new List<CalcResultScaledupProducerHeader>()
                    {
                        new CalcResultScaledupProducerHeader { Name = "Producer ID" },
                        new CalcResultScaledupProducerHeader { Name = "Subsidiary ID" },
                        new CalcResultScaledupProducerHeader { Name = "HouseholdDrinksContainersTonnageGlass" },
                        new CalcResultScaledupProducerHeader { Name = "ScaledupHouseholdDrinksContainersTonnageGlass" },
                    },
                    ScaledupProducers = GetCalcResultScaledupProducerList(),
                },
                CalcResultSummary = new CalcResultSummary
                {
                    ResultSummaryHeader = new CalcResultSummaryHeader
                    {
                        Name = "SummaryData",
                    },
                    ProducerDisposalFeesHeaders = new List<CalcResultSummaryHeader>
                    {
                       new CalcResultSummaryHeader
                       {
                           Name = "Producer disposal fees header",
                           ColumnIndex = 1,
                       },
                    },
                    MaterialBreakdownHeaders = new List<CalcResultSummaryHeader>
                    {
                       new CalcResultSummaryHeader
                       {
                           Name = "Material breakdown header",
                           ColumnIndex = 1,
                       },
                    },
                    ColumnHeaders = new List<CalcResultSummaryHeader>
                    {
                       new CalcResultSummaryHeader
                       {
                           Name = "Column header",
                           ColumnIndex = 1,
                       },
                    },
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                    {
                        new CalcResultSummaryProducerDisposalFees
                        {
                            ProducerCommsFeesByMaterial =
                                new Fixture().Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>(),
                            ProducerDisposalFeesByMaterial =
                                new Fixture().Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(),
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1,
                        },
                    },
                },
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunDate = DateTime.Now,
                    RunName = "CalculatorRunName",
                },
            };
        }

        private static Dictionary<string, CalcResultScaledupProducerTonnage> GetScaledupProducerTonnageByMaterial()
        {
            var tonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            tonnageByMaterial.Add(
                "AL",
                new CalcResultScaledupProducerTonnage()
                {
                    ReportedHouseholdPackagingWasteTonnage = 1000,
                    ReportedPublicBinTonnage = 100,
                    TotalReportedTonnage = 1100,
                    ReportedSelfManagedConsumerWasteTonnage = 500,
                    NetReportedTonnage = 1100,
                    ScaledupReportedHouseholdPackagingWasteTonnage = 2000,
                    ScaledupReportedPublicBinTonnage = 200,
                    ScaledupTotalReportedTonnage = 2200,
                    ScaledupReportedSelfManagedConsumerWasteTonnage = 1000,
                    ScaledupNetReportedTonnage = 2200,
                });

            return tonnageByMaterial;
        }

        private static List<CalcResultScaledupProducer> GetCalcResultScaledupProducerList()
        {
            var scaledupProducerList = new List<CalcResultScaledupProducer>();

            scaledupProducerList.AddRange([
                new CalcResultScaledupProducer()
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnageByMaterial(),
                },
                new CalcResultScaledupProducer()
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnageByMaterial(),
                    IsTotalRow = true,
                },
            ]);

            return scaledupProducerList;
        }
    }
}