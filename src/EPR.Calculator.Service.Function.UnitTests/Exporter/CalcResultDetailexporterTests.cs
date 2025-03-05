namespace EPR.Calculator.Service.Function.UnitTests.Exporter
{
    using System;
    using System.Text;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.Detail;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultDetailexporterTests
    {
        private CalcResultDetailexporter _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new CalcResultDetailexporter();
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultDetail = fixture.Create<CalcResultDetail>();
            calcResultDetail.RunName = "SomeRunName";
            calcResultDetail.RunId = 999;
            calcResultDetail.RunDate = new DateTime(2024, 12, 1);
            calcResultDetail.FinancialYear = "2024-25";
            calcResultDetail.RpdFileORG = "RpdFileOrg";
            calcResultDetail.RpdFilePOM = "RpdFilePom";
            calcResultDetail.LapcapFile = "LapcapFile";
            calcResultDetail.ParametersFile = "ParamsFile";

            var csvContent = new StringBuilder();

            // Act
            _testClass.Export(calcResultDetail, csvContent);

            var result = csvContent.ToString();
            var lines = result.Split(Environment.NewLine);
            Assert.AreEqual(7, lines.Count());

            Assert.IsTrue(lines.First().Contains("Run Name"));
            Assert.IsTrue(lines.First().Contains("SomeRunName"));
            Assert.IsTrue(lines.Last().Contains(string.Empty));
        }
    }
}