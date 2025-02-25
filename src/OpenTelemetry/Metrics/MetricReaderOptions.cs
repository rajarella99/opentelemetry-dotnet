// <copyright file="MetricReaderOptions.cs" company="OpenTelemetry Authors">
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

namespace OpenTelemetry.Metrics;

/// <summary>
/// Options for configuring either a <see cref="BaseExportingMetricReader"/> or <see cref="PeriodicExportingMetricReader"/> .
/// </summary>
public class MetricReaderOptions
{
    private const MetricReaderType MetricReaderTypeUnspecified = (MetricReaderType)(-1);
    private MetricReaderType metricReaderType = MetricReaderTypeUnspecified;

    /// <summary>
    /// Gets or sets the AggregationTemporality used for Histogram
    /// and Sum metrics.
    /// </summary>
    public AggregationTemporality Temporality { get; set; } = AggregationTemporality.Cumulative;

    /// <summary>
    /// Gets or sets the <see cref="MetricReaderType" /> to use. Defaults to <c>MetricReaderType.Manual</c>.
    /// </summary>
    public MetricReaderType MetricReaderType
    {
        get
        {
            if (this.metricReaderType == MetricReaderTypeUnspecified)
            {
                this.metricReaderType = MetricReaderType.Manual;
            }

            return this.metricReaderType;
        }

        set
        {
            this.metricReaderType = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="PeriodicExportingMetricReaderOptions" /> options. Ignored unless <c>MetricReaderType</c> is <c>Periodic</c>.
    /// </summary>
    public PeriodicExportingMetricReaderOptions PeriodicExportingMetricReaderOptions { get; set; } = new PeriodicExportingMetricReaderOptions();
}
