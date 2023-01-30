# HeliumApi.SDK - _Unofficial_ Helium Api SDK for .NET
[![NuGet](https://img.shields.io/nuget/v/HeliumApi.SDK.svg)](https://www.nuget.org/packages/HeliumApi.SDK)

An unofficial SDK I created to work with Helium Api endpoints in .NET applications (Console, Web or Mobile). 
This SDK follow service models and is divided into different service classes.

## Required
- [dotnet 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or [dotnet 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## How to use
Available service classes are:
- **HotspotService:** methods to retrieve and work with hotspots, beacon and witness transactions information
  - GetHotspot
  - GetHotspotByName
  - GetWitnessed
  - GetRoles
  - GetTransaction
  - GetChallenges
  - GetNetworkChallenges
  - RetrieveNetworkChallenges
  - GetBeaconTransactions
  - GetBlockTransactions
  - GetWitnessedTransactions
  - GetHotspotsByRadius
  - GetHotspotsByBox
- **AccountService:** method to work with user accounts information
  - GetAccount
  - GetRewards
  - GetDailyRewards
- **OraclePriceService:** methods to work with Oracle price information
  - GetOraclePrice

## Caching
The SDK by default has response caching enabled for most objects. However you can disable it by using
```
   CacheOptions.DisableCacheFor<object type>();
```
The cache validity is by default set to 1 day (24 hours). You can change it first thing in your program via _CacheOptions_.