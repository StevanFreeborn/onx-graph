# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="1.3.0"></a>
## [1.3.0](https://www.github.com/StevanFreeborn/onx-graph/releases/tag/v1.3.0) (2024-05-05)

### Features

* add add graphs button ([d7623c9](https://www.github.com/StevanFreeborn/onx-graph/commit/d7623c95c010ec0177dde0d6d300d07475c82567))
* add graph queue and queue processing with initial stub issuing updates back to client ([cd3730d](https://www.github.com/StevanFreeborn/onx-graph/commit/cd3730d049183b4ce204428f5eafc057c9399a07))
* add graph status const ([911e2da](https://www.github.com/StevanFreeborn/onx-graph/commit/911e2dac959d489657a14fc318e45a20b45aac22))
* add graphs hub using signalR to server ([34d8c35](https://www.github.com/StevanFreeborn/onx-graph/commit/34d8c35f7a24414fd8eb68acbc2d0b2cbf4cb85e))
* add index on userId field ([1a18d38](https://www.github.com/StevanFreeborn/onx-graph/commit/1a18d3831dd013209d1ca3c5fbc98ba770c1e0d1))
* add spinning loader component ([2139dee](https://www.github.com/StevanFreeborn/onx-graph/commit/2139dee548f3f411318cda42234cfc05a90c2aec))
* allow spinning loader to display custom messages ([6d56930](https://www.github.com/StevanFreeborn/onx-graph/commit/6d5693070ae00d8ba3de5b422dfca518c8792512))
* begin working on individual graphs page ([1ce2707](https://www.github.com/StevanFreeborn/onx-graph/commit/1ce2707e240795af2bb130a15a845e0f54f982d7))
* build data pager component ([9d9c105](https://www.github.com/StevanFreeborn/onx-graph/commit/9d9c105a9fe76b069711b4a41d36726cee1b2c5e))
* build graph card component ([2e6b536](https://www.github.com/StevanFreeborn/onx-graph/commit/2e6b5368d9ebe4f65d36ea9aed70f3464c699c2d))
* build special graph monitor loader with animation states for success and failure ([49cc80e](https://www.github.com/StevanFreeborn/onx-graph/commit/49cc80e2777a1bbb630edbabe64a9c9f424239d0))
* connect to server via client with signalR ([9ac2b8f](https://www.github.com/StevanFreeborn/onx-graph/commit/9ac2b8f60063b290c55a97b141842bf52ce4668a))
* display graph ([a5d4e95](https://www.github.com/StevanFreeborn/onx-graph/commit/a5d4e956b7a47c9a7cec358b921a3bea4aad0e05))
* display graph status on graph ([0cfdfc3](https://www.github.com/StevanFreeborn/onx-graph/commit/0cfdfc3fe8be9cfd1cda75b47cc4cd5016dbc039))
* display graph status on graph ([31ae30f](https://www.github.com/StevanFreeborn/onx-graph/commit/31ae30f955e2e004f576f76fb0baf6e912748b24))
* handle error state ([967fcc3](https://www.github.com/StevanFreeborn/onx-graph/commit/967fcc37c732a7f2c5fdeca914fe79341c5bde27))
* implement get graphs action ([491dcac](https://www.github.com/StevanFreeborn/onx-graph/commit/491dcacce5f3cfb2edc06f976f30e16dab2f9591))
* implement getGraphs on client graph service ([6f32801](https://www.github.com/StevanFreeborn/onx-graph/commit/6f32801b357ce2a9d27c31d549491777453fa39c))
* initial implementation of retreiving graph nodes and edges ([6c4e2ba](https://www.github.com/StevanFreeborn/onx-graph/commit/6c4e2ba9f796e9afddd4b22198fde2799857afad))
* register mongo index service ([c4152f8](https://www.github.com/StevanFreeborn/onx-graph/commit/c4152f8c60d7808950a7891d908297ba60b5fd92))
* retrieve single graph for user ([e0b8944](https://www.github.com/StevanFreeborn/onx-graph/commit/e0b8944d65cdc2305675e95c0e5405c9992a7a13))
* retrieve users graph when going to graphs view ([8fff23f](https://www.github.com/StevanFreeborn/onx-graph/commit/8fff23ff3b16950abfa32a555f80a7ea56649811))
* scroll main content when grows beyond container ([e29d4ad](https://www.github.com/StevanFreeborn/onx-graph/commit/e29d4ad13d2956af89f1330590952bb8d0fb3fca))
* work on displaying one graph ([a2fc726](https://www.github.com/StevanFreeborn/onx-graph/commit/a2fc7261447e7eec7c69992f8545cec6da86af61))

### Bug Fixes

* add back testid ([ca6aecc](https://www.github.com/StevanFreeborn/onx-graph/commit/ca6aeccd6750cce988a1c05d45ecde7a2510ca17))
* add parameter less constructor ([c1170da](https://www.github.com/StevanFreeborn/onx-graph/commit/c1170da42b939bb4e8259367e1c1269f828dfc81))
* adjust box shadow for light mode ([714429a](https://www.github.com/StevanFreeborn/onx-graph/commit/714429a1a4a6ed12fc77e563d37cc23f6cc720b7))
* artificially delay queue processing to allow for client to connect for updates. ([1509641](https://www.github.com/StevanFreeborn/onx-graph/commit/1509641d0e7e63df5b73f543e2e0e2fb28fb23bd))
* background color on home view ([37bb927](https://www.github.com/StevanFreeborn/onx-graph/commit/37bb92700c534a60933300ba357c87d781140317))
* change default page size to 10 ([a39d3df](https://www.github.com/StevanFreeborn/onx-graph/commit/a39d3df4b7f310031247397559ce212456387e48))
* change page size ([adc4a89](https://www.github.com/StevanFreeborn/onx-graph/commit/adc4a8917d87a1d415626be2a7be026f46280d68))
* create separate request for retry ([66c27fb](https://www.github.com/StevanFreeborn/onx-graph/commit/66c27fb700a7a499eea64fd36592176f45cd8ec8))
* disable buttons until mounted ([31a24b2](https://www.github.com/StevanFreeborn/onx-graph/commit/31a24b2f5e254d72707155a4c699044e61dc884a))
* handle long graph names ([7de1817](https://www.github.com/StevanFreeborn/onx-graph/commit/7de1817047730edfd54a15a55ab6f714c909973a))
* make sure sidebar expands over top of all content ([a10c6fa](https://www.github.com/StevanFreeborn/onx-graph/commit/a10c6fa771b0c707fafa14a9106e02414a16c1e1))
* map status property ([e16e27e](https://www.github.com/StevanFreeborn/onx-graph/commit/e16e27e367ac4cac543f96f0c16f3947cd418e6c))
* map userId property ([f005d3e](https://www.github.com/StevanFreeborn/onx-graph/commit/f005d3e4c0dad5ea42b588d68ffd52bd3890ff7c))
* move adding mongo indexes to separate hosted service ([f6eabb5](https://www.github.com/StevanFreeborn/onx-graph/commit/f6eabb5907d207deef92b0581e82f917be1c5d8c))
* remove unnecessary constructor ([33d53d6](https://www.github.com/StevanFreeborn/onx-graph/commit/33d53d66f0d11e29b24be3ca4461a2ad0c15d681))
* use constants for hub events ([1cf5efc](https://www.github.com/StevanFreeborn/onx-graph/commit/1cf5efcd97e4793de40da92ae42ee22190f2006c))

