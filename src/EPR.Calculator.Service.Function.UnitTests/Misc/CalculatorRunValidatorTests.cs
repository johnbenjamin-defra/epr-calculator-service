namespace EPR.Calculator.Service.Function.UnitTests
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Misc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunValidatorTests
    {
        private CalculatorRunValidator TestClass { get; init; }

        private IFixture Fixture { get; init; }

        public CalculatorRunValidatorTests
        {
            this.Fixture = new Fixture();
            this.TestClass = new CalculatorRunValidator();
        }

        [TestMethod]
        public void CheckForNullIds_ShouldReturnErrorMessages_WhenIdsAreNull()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                CalculatorRunOrganisationDataMasterId = null,
                DefaultParameterSettingMasterId = null,
                CalculatorRunPomDataMasterId = 1,
                LapcapDataMasterId = null,
                Name = "soe",
                Financial_Year = "2024-25",
            };

            var validator = new CalculatorRunValidator();

            // Act
            ValidationResult result = validator.ValidateCalculatorRunIds(calculatorRun);

            // Assert
            var expectedErrors = new List<string>
            {
                "CalculatorRunOrganisationDataMasterId is null",
                "DefaultParameterSettingMasterId is null",
                "LapcapDataMasterId is null"
            };
            CollectionAssert.AreEqual(expectedErrors, result.ErrorMessages.ToList());
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void CheckForNullIds_ShouldReturnEmptyList_WhenNoIdsAreNull()
        {
            // Arrange
            var calculatorRun = new CalculatorRun
            {
                CalculatorRunOrganisationDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                CalculatorRunPomDataMasterId = 1,
                LapcapDataMasterId = 1,
                Name = "soe",
                Financial_Year = "2024-25",
            };

            var validator = new CalculatorRunValidator();

            // Act
            ValidationResult result = validator.ValidateCalculatorRunIds(calculatorRun);

            // Assert
            Assert.AreEqual(0, result.ErrorMessages.Count());
            Assert.IsTrue(result.IsValid);
        }
    }
}