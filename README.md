# Niuro Loans

A small loan application. A Next.js UI talks to a .NET 10 API that approves or denies applications and saves them in SQL Server. 
The .NET API sends each approved customer to a Node/Express events service, which writes one JSON file per SSN.

| Service | Tech | URL |
| --- | --- | --- |
| Web | Next.js 16 | http://localhost:3000 |
| API 1 (loans) | .NET 10, EF Core | http://localhost:5000 |
| API 2 (events) | Node 22, Express 5 | http://localhost:4000 |
| Database | SQL Server 2022 | localhost,1433 (`sa` / `Loans_Dev_Pass123!`) |

## Loom Video walkthrough: 
https://www.loom.com/share/0e1f4e9d6b5c4a2b8b8f4f3c5e2d1a2b

## Run everything (Docker Desktop)

```powershell
docker compose up --build
```

Open http://localhost:3000. On first start, API 1 creates the database and seeds the states, 
an admin user, two blacklisted SSNs and one existing customer. 
Files written by API 2 appear in `events-api\data\`.

To reset the database: `docker compose down -v`.

## Run each part locally (PowerShell)

Prerequisites: .NET 10 SDK, Node 22+, Docker Desktop (for SQL Server only).

```powershell
# 1. Database
docker compose up -d sqlserver

# 2. Events API (API 2) -> http://localhost:4000
cd events-api; npm install; npm start

# 3. Loans API (API 1) -> http://localhost:5000   (new terminal)
cd api; dotnet run --project src/Loans.Api

# 4. Web -> http://localhost:3000                 (new terminal)
cd web; npm install; npm run dev
```

Settings live in `api/src/Loans.Api/appsettings.json` (connection string, events URL, JWT key, CORS origin). 
The web app reads `NEXT_PUBLIC_API_URL` (default `http://localhost:5000`).

## Run the tests

```powershell
cd api; dotnet test          # rule engine, returning customer, retries/rollback, endpoint
cd events-api; npm test      # file creation and replacement
```

The .NET tests need no database or Docker: repositories and the event publisher are replaced with in-memory fakes. 
They cover the domain rules, the rule engine, the approval and returning-customer paths with retries and rollback, 
and every endpoint (applications, login, blacklist, states).

### End-to-end tests (Playwright)

Run these against the running stack (`docker compose up --build` first):

```powershell
cd e2e
npm install
npx playwright install chromium
npm test                     # or: npm run test:headed
npm run report               # opens the HTML report
```

They cover the redirect, SSN mask, validation, approval (NewCustomer file), the returning customer (file replaced), 
State and SSN denials, login failure and success, logout, the config guard, toggling a state, and the blacklist Add/Remove flow. 
File checks read `events-api\data`; set `EVENTS_DATA_DIR` if it lives elsewhere. Tests create their own random SSNs and undo their configuration changes.

## Test data

| Goal | What to enter |
| --- | --- |
| Log in | `admin@niuro.test` / `Admin123!` |
| Approved, new customer | Any state except New York, any SSN not listed below, e.g. `123-45-6789` |
| Returning customer | SSN `333-33-3333` (seeded), or submit the same new SSN twice |
| Denied: State | State **New York** (or any state you tick in Configuration → State) |
| Denied: SSN | SSN `111-11-1111` or `222-22-2222` (or any SSN you add in Configuration → SSN BlackList) |
| Denied: could not complete | Stop API 2 (`docker compose stop events-api`), then submit an approvable application |

After an approval, check `events-api\data\<ssn>.json`: `"type"` is `NewCustomer` or `ReturningCustomer`.

---

# ARCHITECTURE

## Project structure

```
api/                          .NET 10 solution (Loans.slnx)
  src/Loans.Domain            Entities (Customer, LoanApplication, State, BlacklistedSsn, User) and the value objects
                              Ssn and ApplicantDetails, which enforce their own invariants. No dependencies.
  src/Loans.Application       Use cases (LoanApplicationService, StateService, BlacklistService, AuthService),
                              the loan rules, and ports (repositories, IUnitOfWork, ICustomerEventPublisher, security).
  src/Loans.Infrastructure    EF Core DbContext + repositories, unit of work, HTTP event publisher,
                              password hashing and JWT issuing.
  src/Loans.Api               Thin controllers, request contracts, JWT validation, composition root (Program.cs).
  tests/Loans.Tests           xUnit tests with in-memory fakes and WebApplicationFactory.
events-api/                   Express app: POST /events -> writes data/<ssn>.json.
web/                          Next.js App Router UI: left navbar, application form, login, configuration pages.
e2e/                          Playwright end-to-end tests against the running stack.
docker-compose.yml            SQL Server, API 1, API 2, web.
```

Dependencies point inward: Api → Application → Domain, and Infrastructure → Application → Domain. Api, as the composition root, 
also references Infrastructure, only to register it (`AddInfrastructure`) and to read `JwtOptions` for token validation;
controllers never use Infrastructure types. The Application layer only knows interfaces. Swapping SQL Server, the HTTP publisher 
or the password hasher means changing Infrastructure and one DI line.

The database uses the table and column names from the spec (including `requested_ammount`). The C# model uses clean names, 
and `LoansDbContext` maps between them. The applicant's personal data is one value object, `ApplicantDetails`,
stored in the Customer row with EF complex-type mapping.

Validation has one source of truth, the domain: `ApplicantDetails.Validate`, `LoanApplication.ValidateRequestedAmount` 
and `Ssn.TryParse` hold every rule and message, including limits that match the database columns (text lengths, 
and an amount that fits `decimal(18,2)` with at most 2 decimals). The constructors use them to reject invalid data, 
and `LoanApplicationRequest` calls the same methods to return every field error in one 400 response.

One file per type, named after the type. Names follow the domain: a `LoanApplication` is submitted with 
the `SubmitLoanApplication` command and gets a `LoanDecision`.

## Rule engine

`ILoanRule.EvaluateAsync(SubmitLoanApplication)` returns a `DenialReason`, or `null` when the rule passes. `LoanRuleEngine` runs 
the registered rules in order and stops at the first denial. So an application that fails both rules reports `State`, the first one.

Current rules: `StateAllowedRule` (the state exists and `isNotAllowed = 0`) and `SsnNotBlacklistedRule`.

**To add a rule:**

1. Create a class in `Loans.Application/LoanApplications/Rules` that implements `ILoanRule`. If it needs new data, add a port.
2. If it has a new reason, add a value to `DenialReason` and its wording in `web/app/denied/page.tsx` (the API returns only the reason; the UI owns the text).
3. Register it in `Loans.Application/DependencyInjection.cs`. Registration order is evaluation order.

## Background event and the external service

After the customer and application rows are saved (but not committed), `LoanApplicationService` publishes a `CustomerEvent` 
through `ICustomerEventPublisher`. The event is a flat record of plain fields (type, ssn, the applicant's fields, 
requestedAmount, submittedAtUtc), so it is a stable contract that doesn't change when domain types are refactored. 
The type is `NewCustomer` if the SSN was not in the Customer table, otherwise `ReturningCustomer`. 
`HttpCustomerEventPublisher` POSTs it as JSON to `{EventsApi:BaseUrl}/events` with a 5-second timeout and throws on any non-2xx response.

API 2 validates the type and the SSN (9 digits, which also blocks path tricks). For `NewCustomer` it writes `<ssn>.json`. 
For `ReturningCustomer` it deletes the file if it exists and writes it again. Any error returns 500, which API 1 treats as a failure.

## Transaction handling

Each attempt evaluates the rules and then runs one unit of work through `IUnitOfWork.ExecuteInTransactionAsync`. 
The rules are inside the retry because they read the database too:

1. Begin a SQL transaction.
2. Upsert Customer by SSN, upsert Application by customerId, and call `SaveChanges`.
3. Publish the event.
4. Commit.

If step 2 or 3 throws (including an HTTP timeout), or the commit fails, the transaction is disposed without committing 
(SQL Server rolls it back). The EF change tracker is also cleared, so no half-saved customer, orphan application or 
event remains. The service retries the whole unit up to **3 times**. After the third failure it denies the application 
with *"Could not be completed at this moment. try in a few minutes."*

EF's built-in `EnableRetryOnFailure` is intentionally off: it cannot wrap user-started transactions, and the explicit 
loop is simpler to reason about.

**Remaining gap:** if API 2 writes the file and then the commit itself fails, the file exists without its rows. That 
file is overwritten on the next successful submit for the same SSN. Closing the gap fully needs an outbox table plus a 
background dispatcher, or a distributed transaction. That is a lot of machinery for a file drop, so it is left out.

## Security notes

- Passwords are stored as salted PBKDF2 hashes (ASP.NET `PasswordHasher`), never in plain text.
- JWTs are issued by `JwtAccessTokenIssuer` in Infrastructure (behind `IAccessTokenIssuer`); the Api only validates them.
- Configuration endpoints require a JWT (60 minutes). The UI keeps it in `sessionStorage` and logs out on 401.
- SSNs are stored as 9 digits (`char(9)`, unique) and shown as `XXX-XX-XXXX`.

## Trade-offs (what was left out and why)

- **`EnsureCreated` instead of EF migrations.** The schema is created and seeded at startup, so there is no migration history. 
  With one schema version this is enough; add migrations when the schema starts to change.
- **Synchronous publish inside the transaction instead of an outbox.** It is the smallest design that meets "roll back if 
  publishing fails". The gap is described above.
- **No SSN encryption at rest.** The spec needs SSN as a unique key and a file name. Production would encrypt or tokenize it.
- **Fakes instead of a real database in tests.** Tests run anywhere, in seconds. Real SQL transaction behavior is covered by 
  the design, not by an integration test against SQL Server.
- **No MediatR, AutoMapper, generic repository or UI component library.** Nothing here needs them.
- **No foreign key from `Customer.stateId` to `StateStatus`.** `stateId` is part of the `ApplicantDetails` value object, 
  and EF cannot put a foreign key on a complex-type property. The state rule already rejects unknown states before anything is saved.
- **`IsNotAllowed` keeps the spec's name.** It is a double negative, but matching the spec's `isNotAllowed` column keeps the UI, API 
  and database speaking the same language.
- **Blacklist add race.** Two admins adding the same SSN at the same moment: the unique index keeps the data correct, but the second 
  request gets a 500 instead of a 409. Handling it would mean translating an EF exception in the Application layer or adding an 
  abstraction for one admin-only edge case.
- **One application per customer.** The spec asks to update the existing Application row, so history is not kept.
- **SSN input.** The spec mentions "10 digits". An SSN has 9 digits (11 characters with dashes), so inputs accept `XXX-XX-XXXX`.
- **SSN BlackList button.** It shows **Remove** while the typed text matches any listed SSN, as the spec describes. It only acts once 
  all 9 digits are entered.
