// <copyright file="OtlpMetricExporterExtensions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using OpenTelemetry.Exporter;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Metrics
{
    /// <summary>
    /// Extension methods to simplify registering of the OpenTelemetry Protocol (OTLP) exporter.
    /// </summary>
    public static class OtlpMetricExporterExtensions
    {
        /// <summary>
        /// Adds <see cref="OtlpMetricExporter"/> to the <see cref="MeterProviderBuilder"/> using default options.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddOtlpExporter(this MeterProviderBuilder builder)
        {
            return AddOtlpExporter(builder, options => { });
        }

        /// <summary>
        /// Adds <see cref="OtlpMetricExporter"/> to the <see cref="MeterProviderBuilder"/>.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <param name="configureExporter">Exporter configuration options.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddOtlpExporter(this MeterProviderBuilder builder, Action<OtlpExporterOptions> configureExporter)
        {
            Guard.ThrowIfNull(builder);

            if (builder is IDeferredMeterProviderBuilder deferredMeterProviderBuilder)
            {
                return deferredMeterProviderBuilder.Configure((sp, builder) =>
                {
                    AddOtlpExporter(builder, sp.GetOptions<OtlpExporterOptions>(), sp.GetOptions<MetricReaderOptions>(), configureExporter, null, sp);
                });
            }

            return AddOtlpExporter(builder, new OtlpExporterOptions(), new MetricReaderOptions(), configureExporter, null, serviceProvider: null);
        }

        /// <summary>
        /// Adds <see cref="OtlpMetricExporter"/> to the <see cref="MeterProviderBuilder"/>.
        /// </summary>
        /// <param name="builder"><see cref="MeterProviderBuilder"/> builder to use.</param>
        /// <param name="configureExporterAndMetricReader">Exporter and <see cref="MetricReader"/> configuration options.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain the calls.</returns>
        public static MeterProviderBuilder AddOtlpExporter(
            this MeterProviderBuilder builder,
            Action<OtlpExporterOptions, MetricReaderOptions> configureExporterAndMetricReader)
        {
            Guard.ThrowIfNull(builder, nameof(builder));

            if (builder is IDeferredMeterProviderBuilder deferredMeterProviderBuilder)
            {
                return deferredMeterProviderBuilder.Configure((sp, builder) =>
                {
                    AddOtlpExporter(builder, sp.GetOptions<OtlpExporterOptions>(), sp.GetOptions<MetricReaderOptions>(), null, configureExporterAndMetricReader, sp);
                });
            }

            return AddOtlpExporter(builder, new OtlpExporterOptions(), new MetricReaderOptions(), null, configureExporterAndMetricReader, serviceProvider: null);
        }

        private static MeterProviderBuilder AddOtlpExporter(
            MeterProviderBuilder builder,
            OtlpExporterOptions exporterOptions,
            MetricReaderOptions metricReaderOptions,
            Action<OtlpExporterOptions> configureExporter,
            Action<OtlpExporterOptions, MetricReaderOptions> configureExporterAndMetricReader,
            IServiceProvider serviceProvider)
        {
            if (configureExporterAndMetricReader != null)
            {
                configureExporterAndMetricReader.Invoke(exporterOptions, metricReaderOptions);
            }
            else
            {
                configureExporter?.Invoke(exporterOptions);
            }

            exporterOptions.TryEnableIHttpClientFactoryIntegration(serviceProvider, "OtlpMetricExporter");

            exporterOptions.AppendExportPath(OtlpExporterOptions.MetricsExportPath);

            var metricExporter = new OtlpMetricExporter(exporterOptions);

            if (metricReaderOptions.MetricReaderType == (MetricReaderType)(-1))
            {
                metricReaderOptions.MetricReaderType = MetricReaderType.Periodic;
            }

            var metricReader = metricReaderOptions.MetricReaderType == MetricReaderType.Manual
                ? new BaseExportingMetricReader(metricExporter)
                : new PeriodicExportingMetricReader(metricExporter, metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds);

            metricReader.Temporality = metricReaderOptions.Temporality;
            return builder.AddReader(metricReader);
        }
    }
}
