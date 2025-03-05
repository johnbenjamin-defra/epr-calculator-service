namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static EPR.Calculator.Service.Function.Services.TransposePomAndOrgDataService;

    [TestClass]
    public class TransposePomAndOrgDataServiceTests
    {
        private readonly ApplicationDBContext _context;

        private Mock<IDbContextFactory<ApplicationDBContext>> ContextFactory { get; init; }

        private readonly DbContextOptions<ApplicationDBContext> _dbContextOptions;

        public TransposePomAndOrgDataServiceTests()
        {
            this.CommandTimeoutService = new Mock<ICommandTimeoutService>().Object;
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            this._context = new ApplicationDBContext(_dbContextOptions);
            this.ContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            this.ContextFactory.Setup(f => f.CreateDbContext()).Returns(this._context);

            this.SeedDatabase();

            this.TestClass = new TransposePomAndOrgDataService(
                this._context,
                this.CommandTimeoutService,
                new Mock<IDbLoadingChunkerService<ProducerDetail>>().Object,
                new Mock<IDbLoadingChunkerService<ProducerReportedMaterial>>().Object);
        }

        private ICommandTimeoutService CommandTimeoutService { get; init; }

        public Fixture Fixture { get; init; } = new Fixture();

        public TransposePomAndOrgDataService TestClass { get; set; }

        [TestCleanup]
        public void TearDown()
        {
            this._context.Database.EnsureDeleted();
            this._context.Dispose();
        }

        private void SeedDatabase()
        {

            this._context.CalculatorRunOrganisationDataMaster.AddRange(GetCalculatorRunOrganisationDataMaster());
            this._context.CalculatorRunOrganisationDataDetails.AddRange(GetCalculatorRunOrganisationDataDetails());

            this._context.CalculatorRunPomDataMaster.AddRange(GetCalculatorRunPomDataMaster());
            this._context.CalculatorRunPomDataDetails.AddRange(GetCalculatorRunPomDataDetails());

            this._context.CalculatorRuns.AddRange(GetCalculatorRuns());
            this._context.Material.AddRange(GetMaterials());

            this._context.SaveChanges();
        }

        [TestMethod]
        public void Transpose_Should_Return_Latest_Organisation_Name()
        {
            var mockContext = new Mock<ApplicationDBContext>();

            var organisationDetails = new List<CalculatorRunOrganisationDataDetail>
            {
                new ()
                {
                    OrganisationId = 1,
                    OrganisationName = "Test1",
                    SubsidaryId = "sub1",
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new ()
                {
                    OrganisationId = 2,
                    OrganisationName = "Test2",
                    SubsidaryId = "sub2",
                    SubmissionPeriodDesc = "January to June 2023",
                },
            };

            var orgDetails = this.TestClass.GetAllOrganisationsBasedonRunId(organisationDetails);

            var orgSubDetails = new List<OrganisationDetails>()
            {
                new ()
                {
                     OrganisationId = 1,
                     OrganisationName = "Test1",
                     SubsidaryId = "sub1",
                     SubmissionPeriodDescription = "January to June 2023",
                },
                new ()
                {
                     OrganisationId = 2,
                     OrganisationName = "Test2",
                     SubsidaryId = "sub2",
                     SubmissionPeriodDescription = "January to June 2024",
                },
            };

            var output = this.TestClass.GetLatestOrganisationName(1, orgSubDetails, orgDetails);
            Assert.IsNotNull(output);
            Assert.AreEqual("Test1", output);
        }


        protected static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>
            {
                new ()
                {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
                new ()
                {
                    Id = 2,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.AddRange(new List<CalculatorRunOrganisationDataDetail>()
            {
                new ()
                {
                    Id = 1,
                    OrganisationId = 1,
                    OrganisationName = "UPU LIMITED",
                    SubsidaryId = "1",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new ()
                {
                    Id = 2,
                    OrganisationId = 1,
                    OrganisationName = "Test LIMITED",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new ()
                {
                    Id = 3,
                    OrganisationId = 2,
                    SubsidaryId = "1",
                    OrganisationName = "Subsid2",
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunOrganisationDataMasterId = 2,
                    SubmissionPeriodDesc = "July to December 2023",
                },
            });
            return list;
        }

        protected static IEnumerable<Material> GetMaterials()
        {
            var list = new List<Material>
            {
                new ()
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new ()
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new ()
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new ()
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new ()
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new ()
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new ()
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new ()
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
        {
            var list = new List<CalculatorRunPomDataMaster>
            {
                new ()
                {
                    Id = 1,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
                new ()
                {
                    Id = 2,
                    CalendarYear = "2024-25",
                    EffectiveFrom = DateTime.Now,
                    CreatedBy = "Test user",
                    CreatedAt = DateTime.Now,
                },
            };
            return list;
        }

        protected static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
        {
            var list = new List<CalculatorRunPomDataDetail>
            {
                new ()
                {
                    Id = 1,
                    OrganisationId = 1,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2023-P2",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new ()
                {
                    Id = 2,
                    OrganisationId = 1,
                    SubmissionPeriod = "2023-P2",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "July to December 2023",
                },
                new ()
                {
                    Id = 3,
                    OrganisationId = 1,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2023-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 1,
                    SubmissionPeriodDesc = "January to June 2023",
                },
                new ()
                {
                    Id = 4,
                    OrganisationId = 2,
                    SubsidaryId = "1",
                    SubmissionPeriod = "2024-P1",
                    PackagingActivity = null,
                    PackagingType = "CW",
                    PackagingClass = "O1",
                    PackagingMaterial = "PC",
                    PackagingMaterialWeight = 1000,
                    LoadTimeStamp = DateTime.Now,
                    CalculatorRunPomDataMasterId = 2,
                    SubmissionPeriodDesc = "January to June 2024",
                },
            };
            return list;
        }



        protected static IEnumerable<CalculatorRun> GetCalculatorRuns()
        {
            var list = new List<CalculatorRun>
            {
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                },
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                },
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Run",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 1,
                    CalculatorRunPomDataMasterId = 1,
                },
                new ()
                {
                    CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                    Name = "Test Calculated Result",
                    Financial_Year = "2024-25",
                    CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                    CreatedBy = "Test User",
                    CalculatorRunOrganisationDataMasterId = 2,
                    CalculatorRunPomDataMasterId = 2,
                },
            };
            return list;
        }
    }
}
