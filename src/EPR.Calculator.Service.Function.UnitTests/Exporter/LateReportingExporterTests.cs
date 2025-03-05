namespace EPR.Calculator.Service.Function.UnitTests.Exporter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="LateReportingExporter"/> class.
    /// </summary>
    [TestClass]
    public class LateReportingExporterTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LateReportingExporterTests"/> class.
        /// </summary>
        public LateReportingExporterTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = new LateReportingExporter();
        }

        private LateReportingExporter TestClass { get; init; }

        private IFixture Fixture { get; init; }

        /// <summary>
        /// Checks that the output matches the expected format.
        /// </summary>
        [TestMethod]
        public void CanCallPrepareData()
        {
            // Arrange
            var input = this.Fixture.Create<CalcResultLateReportingTonnage>();
            var expectedheader = $"\"{input.Name}\"," + Environment.NewLine +
                $"\"{input.MaterialHeading}\",\"{input.TonnageHeading}\"," + Environment.NewLine;
            var expectedMaterials = input.CalcResultLateReportingTonnageDetails.Select(m
                => $"\"{m.Name}\",\"{m.TotalLateReportingTonnage:0.000}\",");

            var expectedResult = Environment.NewLine
                + Environment.NewLine
                + expectedheader
                + string.Join(Environment.NewLine, expectedMaterials)
                + Environment.NewLine;

            // Act
            var result = this.TestClass.Export(input);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}