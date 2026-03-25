# Module 3: Carbon Footprint Calculator — Changelog

**Source Spec**: `t3-1.md`
**Date**: 2026-03-19
**Total**: 64 files changed, ~2200 lines added

---

## Architecture Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Data access | Raw Npgsql SQL | Matches existing ProductGateway/CategoryGateway pattern |
| UnitOfWork | NpgsqlTransaction wrapper | Lightweight, no ORM overhead |
| RecordSet | DataTable wrapper | Matches existing CategoryGateway DataAdapter pattern |
| UI layer | API controllers only | No Razor views; frontend consumes REST APIs |
| Cross-team interfaces | Stub implementations | [P1-5] and [P1-3/P1-4] interfaces return mock data |
| Naming | EmployeeCarbonManager/BuildingCarbonManager | Avoids collision with ASP.NET `Controller` convention |
| RouteData conflict | Type alias (`EntityRouteData`) | Disambiguates from `Microsoft.AspNetCore.Routing.RouteData` |
| Multi-interface DI | Factory delegate forwarding | `sp.GetRequiredService<Concrete>()` for shared instances |

---

## Files Created

### Domain/Entity/ — Entity Classes (9 files)

| File | Spec Class | Role |
|---|---|---|
| `TransportMode.cs` | `<<Enumeration>> TransportMode` | CAR, BUS, TRAIN |
| `WorkMode.cs` | `<<Enumeration>> WorkMode` | REMOTE, ONSITE |
| `CarbonSource.cs` | `<<Abstract Entity>> CarbonSource` | Base for Employee/Building |
| `Employee.cs` | `<<Entity>> Employee` | Employee with commute data |
| `Building.cs` | `<<Entity>> Building` | Building with energy data |
| `TransportHub.cs` | `<<Abstract Entity>> TransportHub` + Airport, TrainStation, ShippingPorts | Transport hub hierarchy |
| `RouteData.cs` | `<<Abstract Entity>> RouteData` + CustomerRouteData, SupplierRouteData | Route information hierarchy |
| `DistanceCF.cs` | `<<Abstract Entity>> DistanceCF` + OrderDistanceCF, SupplierDistanceCF | Distance carbon hierarchy |
| `TotalCF.cs` | `<<Abstract Entity>> TotalCF` + TotalCustomerCF, TotalRestockCF | Total carbon hierarchy |

### Domain/DomainInterface/ — Service Interfaces (14 files)

| File | Spec Interface | Cross-Team? |
|---|---|---|
| `IOSRMService.cs` | `IOSRMService` | No |
| `IPostalService.cs` | `IPostalService` | No |
| `ITotalCarbonService.cs` | `ItotalCarbonService` | Yes [P1-3/P1-4] |
| `ICarbonAnalysisService.cs` | `ICarbonAnalysisService` | No |
| `IEstimatedTimeService.cs` | `IEstimatedTimeService` | No |
| `ICustomerDistanceService.cs` | `ICustomerDistanceService` | No |
| `IRestockDistanceService.cs` | `IRestockDistanceService` | No |
| `IDistanceCarbonService.cs` | `IDistanceCarbonService` | No |
| `IPreShipmentCarbonReader.cs` | `IPreShipmentCarbonReader` | Yes [P1-5] |
| `IGetCarbonData.cs` | `IgetCarbonData` | No |
| `ICoefficientManager.cs` | `ICoefficientManager` | Yes [P1-5] |
| `ISaveShippingMethod.cs` | `IsaveShippingMethod` | No |
| `ICorpCarbonService.cs` | `ICorpCarbonService` | Yes [P1-5] |
| `ICarbonEntityFactory.cs` | `ICarbonEntityFactory` | No |

### Domain/BoundaryInterface/ — Gateway Interfaces (9 files)

| File | Spec Interface |
|---|---|
| `IOutboundDistributionGateway.cs` | `IOutboundDistribution` |
| `IInboundLogisticsGateway.cs` | `IInboundLogisticsGateway` |
| `ISupplierTransportCarbonGateway.cs` | `SupplierTransportCarbon` |
| `ICustomerTransportCarbonGateway.cs` | `CustomerTransportCarbon` |
| `ISupplierCarbonDataGateway.cs` | `SupplierCarbonData` |
| `ICustomerCarbonDataGateway.cs` | `CustomerCarbonData` |
| `IEmployeeRepository.cs` | `IEmployeeRepository` (Fix #1) |
| `IBuildingRepository.cs` | `IBuildingRepository` (Fix #1) |
| `IShippingMethodGateway.cs` | `ShippingMethod` |

### Domain/Control/ — Business Logic (11 files)

| File | Spec Class | Pattern |
|---|---|---|
| `RouteCalculationHandler.cs` | `RouteCalculationHandler` | Template Method (abstract) |
| `OutboundDistribution.cs` | `OutboundDistribution` | Template Method (concrete) |
| `InboundLogistics.cs` | `InboundLogistics` | Template Method (concrete) |
| `TransportCarbonManager.cs` | `TransportCarbonManager` | Multi-interface implementation |
| `CarbonDataAggregator.cs` | `CarbonDataAggregator` | Aggregation |
| `CarbonAnalysis.cs` | `CarbonAnalysis` | Strategy/Analysis |
| `TransportHubManager.cs` | `TransportHubManager` | Aggregation |
| `EmployeeCarbonManager.cs` | `EmployeeController` | Renamed to avoid ASP.NET conflict |
| `BuildingCarbonManager.cs` | `BuildingController` | Renamed to avoid ASP.NET conflict |
| `CustomerEntityFactory.cs` | `CustomerEntityFactory` | Abstract Factory |
| `SupplierEntityFactory.cs` | `SupplierEntityFactory` | Abstract Factory |

### DataSource/ — Data Access (11 files)

| File | Spec Class |
|---|---|
| `UnitOfWork.cs` | `UnitOfWork` |
| `RecordSet.cs` | `RecordSet` |
| `Gateways/OutboundDistributionGateway.cs` | `OutboundDistributionGateway` |
| `Gateways/InboundLogisticsGateway.cs` | `InboundLogisticsGateway` |
| `Gateways/SupplierTransportCarbonGateway.cs` | `SupplierTransportCarbonGateway` |
| `Gateways/CustomerTransportCarbonGateway.cs` | `CustomerTransportCarbonGateway` |
| `Gateways/SupplierCarbonDataGateway.cs` | `SupplierCarbonDataGateway` |
| `Gateways/CustomerCarbonDataGateway.cs` | `CustomerCarbonDataGateway` |
| `Gateways/EmployeeRepositoryGateway.cs` | `EmployeeRepositoryGateway` (Fix #1) |
| `Gateways/BuildingRepositoryGateway.cs` | `BuildingRepositoryGateway` (Fix #1) |
| `Gateways/ShippingMethodGateway.cs` | `ShippingMethodGateway` |

### Boundary/ — External Adapters (2 files)

| File | Spec Class |
|---|---|
| `Boundary/OSRMApiAdapter.cs` | `OSRM API` |
| `Boundary/PostalCodeDatabaseAdapter.cs` | `PostalCodeDatabase` |

### Stubs/ — Cross-Team Integration Mocks (2 files)

| File | Mocks Interface |
|---|---|
| `Stubs/PreShipmentCarbonReaderStub.cs` | `IPreShipmentCarbonReader [P1-5]` |
| `Stubs/CoefficientManagerStub.cs` | `ICoefficientManager [P1-5]` |

### DTO/ — Data Transfer Objects (2 files)

| File | Purpose |
|---|---|
| `DTO/PreShipmentCarbonDataDTO.cs` | Pre-shipment carbon data |
| `DTO/CoefficientDTO.cs` | Emission coefficient data |

### Controllers/ — API Endpoints (3 files)

| File | Route | Endpoints |
|---|---|---|
| `CorporateFootprintController.cs` | `api/corporate-footprint` | Employee/Building CRUD + footprint calc |
| `CarbonCalculationController.cs` | `api/carbon` | Shipping/total carbon queries |
| `CarbonAnalysisController.cs` | `api/carbon-analysis` | Analysis, recommendations, estimates |

### Modified Files

| File | Change |
|---|---|
| `Program.cs` | Added DI registrations for all Module 3 services |

---

## API Endpoints Summary

### Corporate Footprint (`/api/corporate-footprint`)
- `GET /employees` — List all employees
- `POST /employees` — Add employee
- `DELETE /employees/{id}` — Remove employee
- `GET /employees/{id}/footprint` — Calculate employee carbon footprint
- `GET /buildings` — List all buildings
- `POST /buildings` — Add building
- `DELETE /buildings/{id}` — Remove building
- `GET /buildings/{id}/footprint` — Calculate building carbon footprint

### Carbon Calculation (`/api/carbon`)
- `GET /order/{orderID}/shipping` — Get order shipping carbon
- `GET /restock/{restockID}/shipping` — Get restock shipping carbon
- `GET /order/{orderID}/total` — Get order total carbon
- `GET /restock/{restockID}/total` — Get restock total carbon
- `GET /logs` — Get carbon logs

### Carbon Analysis (`/api/carbon-analysis`)
- `GET /recommend` — Get shipping method recommendation
- `GET /analyze` — Analyze carbon level (LOW/MEDIUM/HIGH)
- `GET /estimate` — Estimate shipping carbon for a method + distance

---

## Spec Fixes Applied

All 8 fixes from the spec changelog were incorporated:
1. Placeholder classes renamed to IEmployeeRepository, IBuildingRepository etc.
2. BuildingController constructor uses IBuildingRepository (not IEmployeeRepository)
3. InboundLogistics uses setRestockID (not setOrderID)
4. RouteData uses "rail" (not "plane") to match TrainStation
5. totalCustomerCF cleaned of stray "TotalRestockCF" text
6. SupplierEntityFactory implements (not extends) ICarbonEntityFactory
7. CarbonAnalysis → IsaveShippingMethod relationship removed
8. ICustomerDistanceService (raw km) vs IDistanceCarbonService (CO2) clarified
