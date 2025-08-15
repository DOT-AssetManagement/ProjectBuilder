# ProjectBuilder 4.0 — README

### Table of Contents

- [Overview](#overview)
- [Core Features](#core-features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Contributing](#contributing)
- [License](#license)
- [Contacts](#contacts)

### Overview

**ProjectBuilder** is a C#/.NET, SQL Server-backed web application designed to help PennDOT optimize capital projects. It accomplishes this by:

* **Bundling** asset-level treatments (pavement, bridges, etc.) into candidate projects based on configurable business rules.
* **Optimizing** the selection and timing of these projects across annual Pavement, Bridge, and Open budgets using a mixed-integer linear program (LP) with a refined cutting-plane method.

The core platform relies on **Windows Server**, **SQL Server**, the open-source **SYMPHONY** solver, and an **ESRI SDK** map interface integrated via a data gateway.

---

### Core Features

* Treatment bundling with geographic, temporal, and compatibility rules.
* Multi-year and multi-asset project handling, including modeling indirect costs (Design/ROW/Utilities).
* Scenario management with features for copying, running, and adjusting budget parameters.
* Import and export functionality to and from BAMS/PAMS via Excel.
* A map interface for visualizing and editing projects and work items.
* Administrative interfaces for managing system data, users, and roles.

---

### Architecture

* **Backend:** ASP.NET (C#) with SQL Server.
* **Optimization:** An LP formulation with a relaxed integer pass and cuts for fractional solutions.
* **Mapping:** An ESRI SDK Mapping application embedded into the .NET repo.
* **Deployment OS:** Windows Server.

---

### Prerequisites

To run this application, you'll need the following:

* Windows Server with IIS.
* .NET 8+ SDK.
* SQL Server 2019+ (engine + tools).
* Access to the SYMPHONY solver binaries.

---

## Contributing
We welcome issues and pull requests! To contribute, follow these steps:

- Fork the repository.
- Create a feature branch for your changes.
- Commit your changes with clear, descriptive messages.
- Open a pull request and describe the changes you've made and the reasoning behind them.

---

## License
This repository includes a LICENSE file. Please refer to this file for the specific terms and conditions.

---

## Contacts
For program information and collaboration inquiries, you can reach out to:

PennDOT Asset Management
Justin Bruner, P.E. — jbruner@pa.gov

