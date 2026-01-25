# Changelog

## [2.0.0](https://github.com/glacorSoul/Transceiver/compare/1.0.4...2.0.0)  (2026-01-25)

### Features

* SSL Now supports different host than localhost [e6c8c76](https://github.com/glacorSoul/Transceiver/commit/e6c8c7646298e0911b7d7036cfa1778356bc8dc8)
* Improved Channels performance and external API [a944c94](https://github.com/glacorSoul/Transceiver/commit/a944c9410ab77c7ee802c198cb59ab6e31f3851f)
* Improved performance of CorrelatedMessageProcessor [a944c94](https://github.com/glacorSoul/Transceiver/commit/a944c9410ab77c7ee802c198cb59ab6e31f3851f)
* DictionaryList now uses .NET built-in concurrent collection types [a944c94](https://github.com/glacorSoul/Transceiver/commit/a944c9410ab77c7ee802c198cb59ab6e31f3851f)

## [1.0.4](https://github.com/glacorSoul/Transceiver/compare/1.0.3...1.0.4)  (2025-10-25)

### Features

* Add resilient protocol that will do retries when services throw exceptions

## [1.0.3](https://github.com/glacorSoul/Transceiver/compare/V1.0.1...1.0.3)  (2025-10-12)

### Features

* Improved telemetry [b17354d](https://github.com/glacorSoul/Transceiver/commit/b17354dace58f094aca6c3b5a85a72b80ceffa5b)
* Improved Transceiver pipelines [b17354d](https://github.com/glacorSoul/Transceiver/commit/b17354dace58f094aca6c3b5a85a72b80ceffa5b)
* Transceiver documentation [e48bdd1](https://github.com/glacorSoul/Transceiver/commit/e48bdd112e0c33542e41d301b4b077e005ab84a3)

### Bug Fixes

* Resolve infinite asyncronous flow that caused memory leaks and high resource usage. [d8b7f1c](https://github.com/glacorSoul/Transceiver/commit/d8b7f1cecabfa9dc755f57af585ccc453c4c8613)

## [1.0.2] (2025-10-12)

* This version was not published correctly to Nuget.

## 1.0.1 (2025-09-08)

### Features

* Initial Version of Transceiver. For a full reference of the features please [read the documentation](https://glacorsoul.github.io/Transceiver/guide/codeGen).
