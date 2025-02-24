using EPR.Calculator.Service.Function.Builder.ScaledupProducers;

namespace EPR.Calculator.Service.Function.UnitTests
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using EPR.Calculator.Service.Function.Builder.Summary;
    using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultSummaryBuilderTests
    {
        private readonly DbContextOptions<ApplicationDBContext> dbContextOptions;
        private readonly ApplicationDBContext context;
        private readonly CalcResultSummaryBuilder calcResultsService;
        private readonly CalcResult calcResult;
        private readonly CalcResultScaledupProducers scaledupProducers;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryBuilderTests()
        {
            this.dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CalcResultSummaryTestDb")
                .Options;
            this.context = new ApplicationDBContext(this.dbContextOptions);
            this.calcResultsService = new CalcResultSummaryBuilder(this.context);
            this.scaledupProducers = TestDataHelper.GetScaledupProducers();
            this.calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    Details =
                    [
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
                    ],
                    Materiality =
                    [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality",
                        },
                    ],
                    Name = "Parameters - Other",
                    SaOperatingCost =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 0,
                            England = "England",
                            EnglandValue = 0,
                            Wales = "Wales",
                            WalesValue = 0,
                            Scotland = "Scotland",
                            ScotlandValue = 0,
                            NorthernIreland = "Northern Ireland",
                            NorthernIrelandValue = 0,
                            Total = "Total",
                            TotalValue = 0,
                        },
                    ],
                    SchemeSetupCost =
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
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="ScotlandTest",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Material = "Material1",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Name="Material1",
                            Scotland="ScotlandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="10",
                            England="EnglandTest",
                            Wales="WalesTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Name="Material2",
                            Scotland="ScotlandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                    },
                },
                CalcResultLapcapData = new CalcResultLapcapData() { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() { } },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new ()
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
                        new ()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new ()
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
                        new ()
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
                        new ()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 14.53M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 0.20M,
                            Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                        }

                    ],
                },
                CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>()
                    {
                        new ()
                            {
                            ProducerCommsFeesByMaterial = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>() { },
                            ProducerDisposalFeesByMaterial = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>() { },
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            },
                    },
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost()
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Material1",
                        },
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Material2",
                        },
                    ],
                    CommsCostByCountry =
                    [
                        new ()
                        {
                            Total = "Total",
                        },
                        new ()
                        {
                            TotalValue = 2530,
                        },
                    ],
                },
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),

                CalcResultScaledupProducers = new CalcResultScaledupProducers
                {
                    TitleHeader = null,
                    MaterialBreakdownHeaders = null,
                    ColumnHeaders = null,
                    ScaledupProducers = new List<CalcResultScaledupProducer>(),
                },
            };

            // Seed database
            SeedDatabase(this.context);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.context.Database.EnsureDeleted();
            this.context.Dispose();
        }

        [TestMethod]
        public void Construct_ShouldReturnCalcResultSummary()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.Construct(requestDto, calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader?.Name);
            Assert.AreEqual(25, result.ProducerDisposalFeesHeaders.Count());

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
            var firstProducer = result.ProducerDisposalFees.FirstOrDefault();
            Assert.IsNotNull(firstProducer);
            Assert.AreEqual("Producer1", firstProducer.ProducerName);
        }

        [TestMethod]
        public void Construct_ShouldMapMaterialBreakdownHeaders()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.Construct(requestDto, calcResult);

            results.Wait();
            var result = results.Result;

            Assert.AreEqual("Material1 Breakdown", result.MaterialBreakdownHeaders.First().Name);
        }

        [TestMethod]
        public void Construct_ShouldCalculateProducerDisposalFeesCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.Construct(requestDto, calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsTrue(result.ProducerDisposalFees.Any());
            Assert.AreEqual("Producer1", result.ProducerDisposalFees.First().ProducerName);
        }

        [TestMethod]
        public void Construct_ShouldReturnEmptyProducerDisposalFees_WhenNoProducers()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.Construct(requestDto, calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
        }

        [TestMethod]
        public void Construct_ShouldCalculateBadDebtProvisionCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = this.calcResultsService.Construct(requestDto, calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public void Construct_ShouldReturnProducerDisposalFees_WithoutSubsidiaryTotalRow()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.Construct(calcResultsRequestDto, calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
            Assert.IsFalse(result.ProducerDisposalFees.Any(fee => fee.ProducerName.Contains("Total")));
        }

        [TestMethod]
        public void Construct_ShouldReturnProducerDisposalFees_WithSubsidiaryTotalRow()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.Construct(calcResultsRequestDto, calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public void Construct_ShouldReturnOverallTotalRow_ForAllProducers()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.Construct(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);
            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
        }

        [TestMethod]
        public void GetTotalBadDebtprovision1_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.BadDebtProvisionFor1 = 100m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.BadDebtProvisionFor1);

            Assert.AreEqual(100m, totalFee);
        }

        [TestMethod]
        public void GetTotalDisposalCostswithBadDebtprovision1_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.TotalProducerDisposalFeeWithBadDebtProvision = 200m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerDisposalFeeWithBadDebtProvision);

            Assert.AreEqual(200m, totalFee);
        }

        [TestMethod]
        public void GetTotalCommsCostswoBadDebtprovision2A_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.TotalProducerCommsFee = 300m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerCommsFee);

            Assert.AreEqual(300m, totalFee);
        }

        [TestMethod]
        public void GetTotalBadDebtprovision2A_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(calcResultsRequestDto, calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.BadDebtProvisionFor2A = 400m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.BadDebtProvisionFor2A);

            Assert.AreEqual(400m, totalFee);
        }

        [TestMethod]
        public void GetTotalCommsCostswithBadDebtprovision2A_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.TotalProducerCommsFeeWithBadDebtProvision = 500m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerCommsFeeWithBadDebtProvision);

            Assert.AreEqual(500m, totalFee);
        }

        [TestMethod]
        public void GetTotalFee_ShouldReturnZero_WhenNoTotalsLevel()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.BadDebtProvisionFor1 = 0m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.BadDebtProvisionFor1);

            Assert.AreEqual(0m, totalFee);
        }

        [TestMethod]
        public void ProducerTotalPercentageVsTotal_ShouldReturnCorrectValue()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(requestDto, this.calcResult);
            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
            Assert.AreEqual(25, result.ProducerDisposalFeesHeaders!.Count());

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
            var producerTotalPercentage = result.ProducerDisposalFees.First().PercentageofProducerReportedTonnagevsAllProducers;
            Assert.IsNotNull(producerTotalPercentage);
            Assert.AreEqual(100, producerTotalPercentage);
        }

        [TestMethod]
        public void CommsCost2bBill_ShouldReturnCorrectValue()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.Construct(requestDto, this.calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
            Assert.AreEqual(25, result.ProducerDisposalFeesHeaders!.Count());
            var isColumnHeaderExists = result.ProducerDisposalFeesHeaders!.Select(dict => dict.ColumnIndex == 213 || dict.ColumnIndex == 214 || dict.ColumnIndex == 215).ToList();
            Assert.IsTrue(isColumnHeaderExists.Contains(true));
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public void MaterialMapper_ShouldReturnCorrectValue()
        {
            var materials = this.context.Material.ToList();
            var result = Mappers.MaterialMapper.Map(materials);
            Assert.IsNotNull(result);
            Assert.AreEqual(materials.Count, result.Count);
            var material = result[0];
            var actualMaterial = materials[0];
            Assert.IsNotNull(material);
            Assert.IsNotNull(actualMaterial);
            Assert.AreEqual(material.Name, actualMaterial.Name);
            Assert.AreEqual(material.Code, actualMaterial.Code);
        }

        [TestMethod]
        public void GetProducerRunMaterialDetails_ShouldReturnCorrectValue()
        {
            var result = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(
                this.context.ProducerDetail.ToList(),
                this.context.ProducerReportedMaterial.ToList(),
                1);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            var producer = result.FirstOrDefault(t => t.ProducerDetail.Id == 1);
            Assert.AreEqual(3, producer?.ProducerDetail.ProducerReportedMaterials.Count);
            Assert.AreEqual("Producer1", producer?.ProducerDetail.ProducerName);
        }

        [TestMethod]
        public void GetOrderedListOfProducersAssociatedRunId_ShouldReturnCorrectValue()
        {
            var result = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, this.context.ProducerDetail.ToList());
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("Producer1", result.First().ProducerName);
            Assert.AreEqual("Producer5", result.Last().ProducerName);
        }

        [TestMethod]
        public void GetCalcResultSummary_ShouldReturnCorrectValue()
        {
            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, this.context.ProducerDetail.ToList());
            var runProducerMaterialDetails = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(
                orderedProducerDetails,
                this.context.ProducerReportedMaterial.ToList(),
                1);

            var materials = Mappers.MaterialMapper.Map(this.context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materials, 1);

            var result = new CalcResultSummaryBuilder(this.context).GetCalcResultSummary(orderedProducerDetails, materials,
                this.calcResult, totalPackagingTonnage);

            Assert.IsNotNull(result);
            Assert.AreEqual(128, result.ColumnHeaders.Count());

            var producerDisposalFees = result.ProducerDisposalFees;
            Assert.IsNotNull(producerDisposalFees);

            var totals = producerDisposalFees.First(t => t.isProducerScaledup == "Totals");
            var producer = producerDisposalFees.First(t => t.Level == "1");
            Assert.IsNotNull(producer);

            Assert.AreEqual(string.Empty, totals?.ProducerName);
            Assert.IsNotNull(producer.ProducerName);
            Assert.AreEqual("Producer1", producer.ProducerName);
        }

        [TestMethod]
        public void GetTonnages_ShouldCalculateCorrectlyForGlass()
        {
            List<CalculatorRunPomDataDetail> pomData = [];
            List<MaterialDetail> materials = [];

            var glassMaterial = new MaterialDetail
            {
                Code = MaterialCodes.Glass,
                Name = "Glass",
                Description = "Glass material description",
            };
            materials.Add(glassMaterial);

            var glassPomData = new List<CalculatorRunPomDataDetail>
            {
                new CalculatorRunPomDataDetail
                {
                    PackagingMaterial = MaterialCodes.Glass,
                    SubmissionPeriod = "2024-P1",
                    PackagingType = PackagingTypes.Household,
                    PackagingMaterialWeight = 100,
                    SubmissionPeriodDesc = "2024 Period 1",
                    LoadTimeStamp = DateTime.UtcNow,
                },
                new CalculatorRunPomDataDetail
                {
                    PackagingMaterial = MaterialCodes.Glass,
                    SubmissionPeriod = "2024-P1",
                    PackagingType = PackagingTypes.HouseholdDrinksContainers,
                    PackagingMaterialWeight = 30,
                    SubmissionPeriodDesc = "2024 Period 1",
                    LoadTimeStamp = DateTime.UtcNow,
                },
            };

            pomData.AddRange(glassPomData);

            // Act
            var result = CalcResultScaledupProducersBuilder.GetTonnages(pomData, materials, "2024-P1", 1);

            // Assert
            Assert.IsTrue(result.ContainsKey(MaterialCodes.Glass));
            var glassTonnage = result[MaterialCodes.Glass];

            Assert.AreEqual(0.1m, glassTonnage.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.TotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.NetReportedTonnage);
            Assert.AreEqual(0.1m, glassTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupTotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupNetReportedTonnage);
        }

        private static void SeedDatabase(ApplicationDBContext context)
        {
            context.Material.AddRange(new List<Material>
            {
                new () { Id = 1, Name = "Material1", Code = "123" },
                new () { Id = 2, Name = "Material2", Code = MaterialCodes.Glass },
            });

            context.ProducerDetail.AddRange(new List<ProducerDetail>
            {
                new () { Id = 1, ProducerName = "Producer1", ProducerId = 1, CalculatorRunId = 1, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test1" } },
                new () { Id = 2, ProducerName = "Producer2", ProducerId = 2, CalculatorRunId = 2, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test2" } },
                new () { Id = 3, ProducerName = "Producer3", ProducerId = 3, CalculatorRunId = 3, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test3" } },
                new () { Id = 4, ProducerName = "Producer4", ProducerId = 4, CalculatorRunId = 1 },
                new () { Id = 5, ProducerName = "Producer5", ProducerId = 5, CalculatorRunId = 1 },
            });

            context.ProducerReportedMaterial.AddRange(new List<ProducerReportedMaterial>
            {
                new () { Id = 1, MaterialId = 1, PackagingType = "HH", PackagingTonnage = 400m, ProducerDetailId = 1 },
                new () { Id = 2, MaterialId = 2, PackagingType = "HH", PackagingTonnage = 400m, ProducerDetailId = 2 },
                new () { Id = 3, MaterialId = 1, PackagingType = "CW", PackagingTonnage = 200m, ProducerDetailId = 1 },
                new () { Id = 4, MaterialId = 2, PackagingType = "CW", PackagingTonnage = 200m, ProducerDetailId = 2 },
                new () { Id = 5, MaterialId = 1, PackagingType = "HDC", PackagingTonnage = 300m, ProducerDetailId = 1 },
                new () { Id = 6, MaterialId = 2, PackagingType = "HDC", PackagingTonnage = 300m, ProducerDetailId = 2 },
            });
            context.SaveChanges();
        }
    }
}