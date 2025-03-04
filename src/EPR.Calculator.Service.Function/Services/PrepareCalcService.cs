﻿namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Exporter;
    using EPR.Calculator.API.Validators;
    using EPR.Calculator.Service.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder;
    using EPR.Calculator.Service.Function.Data;
    using EPR.Calculator.Service.Function.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using EPR.Calculator.Service.Function.Misc;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Abstractions;
    using Microsoft.ApplicationInsights;

    public class PrepareCalcService : IPrepareCalcService
    {
        public PrepareCalcService(
            IDbContextFactory<ApplicationDBContext> context,
            IRpdStatusDataValidator rpdStatusDataValidator,
            IOrgAndPomWrapper wrapper,
            ICalcResultBuilder builder,
            ICalcResultsExporter<CalcResult> exporter,
            ITransposePomAndOrgDataService transposePomAndOrgDataService,
            IStorageService storageService,
            CalculatorRunValidator validationRules,
            ICommandTimeoutService commandTimeoutService,
            TelemetryClient telemetryClient)
        {
            this.Context = context.CreateDbContext();
            this.rpdStatusDataValidator = rpdStatusDataValidator;
            this.Wrapper = wrapper;
            this.Builder = builder;
            this.Exporter = exporter;
            this.transposePomAndOrgDataService = transposePomAndOrgDataService;
            this.storageService = storageService;
            this.validatior = validationRules;
            this.commandTimeoutService = commandTimeoutService;
            this._telemetryClient = telemetryClient;
        }

        private readonly TelemetryClient _telemetryClient;

        private ApplicationDBContext Context { get; init; }

        private IRpdStatusDataValidator rpdStatusDataValidator { get; init; }

        private IOrgAndPomWrapper Wrapper { get; init; }

        private ICalcResultBuilder Builder { get; init; }

        private ICalcResultsExporter<CalcResult> Exporter { get; init; }

        private ITransposePomAndOrgDataService transposePomAndOrgDataService { get; init; }

        private IStorageService storageService { get; init; }

        private CalculatorRunValidator validatior { get; init; }

        private ICommandTimeoutService commandTimeoutService { get; init; }

        public async Task<bool> PrepareCalcResults(
            [FromBody] CalcResultsRequestDto resultsRequestDto,
            CancellationToken cancellationToken)
        {
            this.commandTimeoutService.SetCommandTimeout(this.Context.Database);

            CalculatorRun? calculatorRun = null;
            try
            {
                calculatorRun = await this.Context.CalculatorRuns.SingleOrDefaultAsync(
                    run => run.Id == resultsRequestDto.RunId,
                    cancellationToken);
                if (calculatorRun == null)
                {
                    return false;
                }

                // Validate the result for all the required IDs
                var validationResult = validatior.ValidateCalculatorRunIds(calculatorRun);
                if (!validationResult.IsValid)
                {
                    return false;
                }

                this._telemetryClient.TrackTrace("Builder started...");
                var results = await this.Builder.Build(resultsRequestDto);
                this._telemetryClient.TrackTrace("Builder end...");

                this._telemetryClient.TrackTrace("Exporter started...");
                var exportedResults = this.Exporter.Export(results);
                this._telemetryClient.TrackTrace("Exporter end...");

                this._telemetryClient.TrackTrace("Exporter started...");
                var fileName = new CalcResultsFileName(
                    results.CalcResultDetail.RunId,
                    results.CalcResultDetail.RunName,
                    results.CalcResultDetail.RunDate);
                var blobUri = await this.storageService.UploadResultFileContentAsync(fileName, exportedResults);
                this._telemetryClient.TrackTrace("Exporter end...");

                var startTime = DateTime.Now;
                if (!string.IsNullOrEmpty(blobUri))
                {
                    await SaveCsvFileMetadataAsync(results.CalcResultDetail.RunId, fileName.ToString(), blobUri);
                    calculatorRun.CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED;
                    this.Context.CalculatorRuns.Update(calculatorRun);
                    await this.Context.SaveChangesAsync(cancellationToken);
                    var timeDiff = startTime - DateTime.Now;
                    return true;
                }
            }
            catch (OperationCanceledException exception)
            {
                await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                return false;
            }
            catch (Exception exception)
            {
                await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
                return false;
            }

            await this.HandleErrorAsync(calculatorRun, RunClassification.ERROR);
            return false;
        }

        private async Task HandleErrorAsync(CalculatorRun? calculatorRun, RunClassification classification)
        {
            if (calculatorRun != null)
            {
                calculatorRun.CalculatorRunClassificationId = (int)classification;
                this.Context.CalculatorRuns.Update(calculatorRun);
                await this.Context.SaveChangesAsync();
            }
        }

        private async Task SaveCsvFileMetadataAsync(int runId, string fileName, string blobUri)
        {
            var csvFileMetadata = new CalculatorRunCsvFileMetadata
            {
                FileName = fileName,
                BlobUri = blobUri,
                CalculatorRunId = runId
            };
            await this.Context.CalculatorRunCsvFileMetadata.AddAsync(csvFileMetadata);
        }
    }
}
