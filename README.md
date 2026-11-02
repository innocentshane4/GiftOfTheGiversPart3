# This is Gift of the Givers — Part 3 Package

## File inventory

```
GiftOfTheGivers_Part3/
├── README.md                          ← this file
│
├── UnitTests/
│   ├── GiftOfTheGivers.Tests.csproj   ← unit + integration test project
│   └── DonationTests.cs               ← 17 unit tests (3 test classes)
│
├── IntegrationTests/
│   └── DonationFlowIntegrationTests.cs ← 5 integration tests (EF InMemory)
│
├── SeleniumTests/
│   ├── GiftOfTheGivers.UITests.csproj  ← Selenium test project
│   ├── SeleniumTestBase.cs             ← shared driver setup
│   ├── EmployeeLoginTests.cs           ← 3 UI tests — employee login
│   └── AnonymousDonationTests.cs       ← 4 UI tests — anonymous donation
│
├── LoadTest/
│   └── load_test.js                    ← k6 load test script
│
├── Pipeline/
│   └── azure-pipelines.yml             ← Azure DevOps CI/CD pipeline
│
└── UserManual/
    ├── UserManual.html                  ← non-technical staff user manual
    └── TeamContributionReport.html      ← one-page team contribution report
```

---

## A. Set up the unit test project in Visual Studio

### 1. Add the test project to the solution

```
Right-click the solution in Solution Explorer
→ Add → Existing Project
→ browse to UnitTests/GiftOfTheGivers.Tests.csproj
```

Move `IntegrationTests/DonationFlowIntegrationTests.cs` into
`UnitTests/` so both files live in the same project (the `.csproj`
reference covers both). The filter `FullyQualifiedName~Integration`
in the pipeline separates them automatically.

### 2. Add the NuGet packages

The `.csproj` already lists them. Visual Studio will restore them
automatically on build. If not, run:

```
dotnet restore UnitTests/GiftOfTheGivers.Tests.csproj
```

### 3. Run the tests

- Open **Test Explorer** (Test → Test Explorer).
- Click **Run All** (the green play button).
- All 17 unit tests and 5 integration tests should go green.

**Screenshot to submit:** Test Explorer showing all tests passing.

---

## B. Set up the Selenium test project

### 1. Add the UI test project

```
Right-click the solution
→ Add → Existing Project
→ browse to SeleniumTests/GiftOfTheGivers.UITests.csproj
```

### 2. Install Chrome

Selenium uses Chrome in headless mode. You need Chrome installed at
its default system path (it is already there on Windows 10/11).

The `Selenium.WebDriver.ChromeDriver` NuGet package must match your
installed Chrome version. Update the version in `.csproj` if needed:

```
<PackageReference Include="Selenium.WebDriver.ChromeDriver"
                  Version="127.0.6533.7200" />
```

Check your Chrome version: `chrome://version/` in the address bar.

### 3. Start the application first

Selenium tests connect to a running instance of the app.

Either:
- Press **F5** in Visual Studio to start the app locally, or
- Set `BaseUrl` in `SeleniumTestBase.cs` to the Azure URL.

### 4. Run the UI tests

In Test Explorer, filter by project `GiftOfTheGivers.UITests` and click
**Run All**.

**Screenshot to submit:** Test Explorer showing all 7 Selenium tests passing.

---

## C. Run the k6 load test

### Install k6

```
winget install k6
```

Or download from https://k6.io/docs/getting-started/installation/

### Run against local app

```
cd LoadTest
k6 run load_test.js
```

### Run against Azure

```
k6 run --env BASE_URL=https://giftofthegivers.azurewebsites.net load_test.js
```

### What to submit

- Screenshot of the terminal showing the k6 summary output with:
  - `http_req_failed` < 1 %
  - `http_req_duration p(95)` < 2 000 ms
- The generated `load_test_results.json` file.

---

## D. Set up the Azure DevOps pipeline

### 1. Push the pipeline file

Commit `Pipeline/azure-pipelines.yml` to the root of your Azure Repos
repository:

```
git add azure-pipelines.yml
git commit -m "Add CI/CD pipeline"
git push
```

### 2. Create the pipeline in Azure DevOps

```
Pipelines → New Pipeline
→ Azure Repos Git
→ select your repository
→ "Existing Azure Pipelines YAML file"
→ select /azure-pipelines.yml
→ Save and run
```

### 3. Create the variable group

```
Pipelines → Library → + Variable group
Name: GiftOfTheGivers-Prod
Add variables (mark secrets as secret):
  AZURE_WEBAPP_NAME        giftofthegivers
  AZURE_FUNCTIONAPP_NAME   gotg-functions
  AZURE_SQL_SERVER         gotg-server.database.windows.net
  AZURE_SQL_DB             GiftOfTheGiversDB
  AZURE_SQL_USER           (secret)
  AZURE_SQL_PASSWORD       (secret)
  ConnectionStrings__DefaultConnection   (secret)
```

Link the group to the pipeline under **Pipeline → Edit → Variables →
Variable groups**.

### 4. Create the service connection

```
Project Settings → Service connections
→ New service connection → Azure Resource Manager
→ name it: AzureServiceConnection
```

### What to submit

- Screenshot of the pipeline run showing the Build stage green and
  tests passing.
- Screenshot of the pipeline test results tab showing all unit and
  integration tests.
- Screenshot of the Deploy stage showing successful deployment to
  Azure App Service.

---

## E. User manual screenshots

Open `UserManual/UserManual.html` in a browser (double-click to open).

Replace every grey dashed **Screenshot placeholder** box with an actual
screenshot from your running application. The placeholders are labelled
with exactly what to capture.

To convert to PDF: open in Chrome, press Ctrl+P, set "Save as PDF".

---

## F. Team contribution report

Open `UserManual/TeamContributionReport.html` in a browser.

1. Replace `[Your Full Name]` and `[Teammate Full Name]` with actual names.
2. Replace the placeholder commit hashes with real ones from your Git log:
   ```
   git log --oneline --all
   ```
3. Print/save as PDF the same way as the user manual.
