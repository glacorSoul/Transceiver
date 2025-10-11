# Telemetry

Transceiver implements telemetry by default, allowing per request metrics and it is compatible with `OpenTelemetryAPI`!
The following metrics are gathered:

| Metric                                     | Description                                                                                      |
|--------------------------------------------|--------------------------------------------------------------------------------------------------|
| requests_error_total_\<service name>       | Total number of errors for this service.                                                         |
| requests_total_\<service name>             | Total number of requests.                                                                        |
| requests_slow_total_\<service name>        | Number of requests that exceed 500ms SLA.                                                        |
| request_execution_time_ms_\<service name>  | Execution time of a request im milliseconds.                                                     |
| requests_per_second_\<service name>        | Number of requests per sencond.                                                                  |
