# 🧠 Wellness Check-in

A small HR wellness app: employees log how they're doing each day (a mood from **1-5** plus an optional note), and managers get a dashboard to spot trends across the team.

I built this as a take-home to show how I structure a feature end to end: a .NET API with a clean, testable architecture, and a small but polished Next.js frontend on top of it.

> **A note on "auth":** identity here is intentionally a demo. The frontend just sends a header saying who you are. It's simple on purpose and clearly flagged (see [Roles & "auth"](#roles--auth)). Everything else is built the way I'd build it for real.

---

## Tech stack

| | |
| --- | --- |
| **Backend** | ASP.NET Core Web API (.NET 9) · EF Core · PostgreSQL · FluentValidation |
| **Frontend** | Next.js 15 (App Router) · TypeScript · Tailwind CSS · SWR |
| **Tests** | xUnit · Moq · FluentAssertions (backend) · Jest · React Testing Library (frontend) |

Under the hood it's a lightweight take on **Clean Architecture**, with **CQRS** splitting the application's use cases into reads and writes ([here's why](#why-cqrs)).

---

## Quick start

You'll need the **.NET 9 SDK**, **Node 18+**, and **PostgreSQL** (Docker is the easiest route).

```bash
# 1. Database (Docker spins up Postgres with the default credentials).
#    Already have Postgres locally? See "Database" below.
docker compose up -d

# 2. API  ->  http://localhost:5080   (Swagger UI at /swagger)
cd backend
dotnet run --project src/MentalHealth.Api

# 3. Frontend  ->  http://localhost:3000
cd frontend
npm install
npm run dev
```

Then open **http://localhost:3000** and use the **user switcher in the top-right** to act as an employee or a manager. On first run the API creates the schema and seeds a handful of users plus a couple of weeks of check-ins, so nothing looks empty.

---

## What you can do

- **As an employee:** submit a daily check-in, then browse and edit your own history.
- **As a manager:** review the whole team's check-ins and watch a dashboard of average mood over time.

Managers are review-only by design: they don't get a "new check-in" form or edit buttons, and the API enforces that too, not just the UI.

---

## How it's put together

Dependencies point inward: `Api -> Application -> Domain`, and `Infrastructure -> Application`. The domain doesn't know EF Core exists; the application layer defines interfaces that infrastructure implements.

```
backend/
├── MentalHealth.slnx
└── src/
    ├── MentalHealth.Domain/          # Entities, enums, business rules (pure C#, no dependencies)
    ├── MentalHealth.Application/      # Use cases (CQRS), DTOs, validation, interfaces, Result pattern
    ├── MentalHealth.Infrastructure/   # EF Core, PostgreSQL, migrations, seed data
    └── MentalHealth.Api/              # Controllers, header auth, DI wiring, Result -> HTTP mapping
frontend/
└── src/
    ├── app/          # routes: /, /checkins, /checkins/[id], /dashboard
    ├── components/   # CheckInForm, CheckInList, FilterBar, MoodTrendChart, …
    └── lib/          # API client, SWR hooks, current-user context, types
```

**The short version of each layer:**

- **Domain** is just C#. A `CheckIn` is created through a `CheckIn.Create` factory that enforces its own rules (mood 1-5, notes ≤ 1000 chars), so the invariants hold no matter who calls it.
- **Application** is one class per use case, a *handler*. Handlers talk to EF through an `IApplicationDbContext` seam (not the concrete `DbContext`), validate with FluentValidation, and return a `Result`/`Result<T>` instead of throwing for expected outcomes.
- **Infrastructure** is the EF Core `DbContext`, entity configs, migrations, and the seeder.
- **Api** is thin: build a command/query, hand it to a handler, map the `Result` to an HTTP status. No business logic lives in the controllers.

---

## Why CQRS

"CQRS" here just means **reads and writes are separate use cases**: `CreateCheckInCommand` / `UpdateCheckInCommand` for writes, `GetCheckInsQuery` / `GetCheckInByIdQuery` / `GetDashboardStatsQuery` for reads, each with its own handler.

Every handler implements a tiny interface:

```csharp
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}
```

Controllers depend on `ICommandHandler<…>` / `IQueryHandler<…>` and DI injects the concrete handler.

Why I like it for a project like this:

- Each use case is one small, self-contained class with a single `Handle` method, so it's easy to find, read, and reason about.
- **F12 in the controller jumps straight to the handler** that does the work, with no indirection to step through.
- Handlers are trivial to unit-test: you `new` one up with a fake `IApplicationDbContext` and call `Handle`.
- All the wiring lives in one readable file, [`Application/DependencyInjection.cs`](backend/src/MentalHealth.Application/DependencyInjection.cs).

I also deliberately **didn't** add a generic repository or a pipeline-behavior framework. Handlers use `IApplicationDbContext` directly, which is plenty at this scope.

---

## Database

The default connection string lives in [`appsettings.json`](backend/src/MentalHealth.Api/appsettings.json):

```
Host=localhost;Port=5432;Database=mentalhealth;Username=postgres;Password=postgres
```

That `postgres`/`postgres` is the throwaway pair the bundled `docker-compose.yml` provisions, so **no real secret is committed**. With Docker you don't change anything; the API runs its migrations and seeds the database on startup.

**Using your own local Postgres with a different password?** Override it *without editing the committed file*. Config precedence means either of these wins:

```bash
# .NET user-secrets (recommended; stored in your user profile, never committed)
cd backend/src/MentalHealth.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=mentalhealth;Username=postgres;Password=YOUR_PASSWORD"
```

```bash
# …or an environment variable (note the double underscore)
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=mentalhealth;Username=postgres;Password=YOUR_PASSWORD"
```

> If Postgres isn't reachable, the API still starts (so Swagger loads) and logs a warning, so start the database and restart.

---

## API

Base URL `http://localhost:5080`. There's a ready-to-run set of requests in [`MentalHealth.Api.http`](backend/src/MentalHealth.Api/MentalHealth.Api.http).

| Method | Route | What it does |
| --- | --- | --- |
| `POST` | `/checkins` | Submit a check-in (author comes from the auth header, not the body) |
| `GET` | `/checkins` | List check-ins; filter by `userId`, `from`, `to` and paginate with `page`, `pageSize` |
| `GET` | `/checkins/{id}` | Get one check-in |
| `PUT` | `/checkins/{id}` | Update a check-in |
| `GET` | `/dashboard/stats` | Average mood over time + totals (managers only) |
| `GET` | `/users` | Seeded users (for the filter and role switcher) |

**Create a check-in:**

```bash
curl -X POST http://localhost:5080/checkins \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 22222222-2222-2222-2222-222222222222" \
  -H "X-User-Role: Employee" \
  -d '{ "mood": 4, "notes": "Good day" }'
```

**Validation errors** come back as RFC-7807 `ValidationProblemDetails`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Mood": ["Mood must be between 1 and 5."] }
}
```

---

## Roles & "auth"

There's no real authentication, by design. The frontend tells the API who the "logged-in" user is via two headers, which [`CurrentUser`](backend/src/MentalHealth.Api/Auth/CurrentUser.cs) reads:

```
X-User-Id:   <user guid>
X-User-Role: Employee | Manager
```

What each role can do (the rules are enforced in the API, not just the UI):

| Action | Employee | Manager | No identity |
| --- | --- | --- | --- |
| Submit a check-in (`POST /checkins`) | ✅ for themselves | ❌ `403` | ❌ `401` |
| Edit a check-in (`PUT /checkins/{id}`) | ✅ own only | ❌ `403` | ❌ `401` |
| Get a check-in (`GET /checkins/{id}`) | ✅ own only (else `404`) | ✅ anyone's | open |
| List check-ins (`GET /checkins`) | ✅ own only (auto-scoped) | ✅ everyone's | open |
| Dashboard (`GET /dashboard/stats`) | ❌ `403` | ✅ | ❌ `401` |

A few details I was deliberate about:

- The author of a check-in is **always** taken from the header, never the request body, so you can't submit on someone else's behalf.
- Employees can only read or edit their **own** check-ins. Asking for someone else's id returns `404`, not `403`, so we don't even leak that it exists.

Seeded users: **Alice Manager** (Manager) and **Bob / Carol / Dan** (Employees).

> ⚠️ Header-based identity is trivially spoofable and is only acceptable for a demo. The real version would use cookies/JWT with ASP.NET Core authentication and authorization policies.

---

## Tests

```bash
# Backend: 32 tests, EF Core in-memory provider, no database needed
cd backend
dotnet test

# Frontend: component behaviour
cd frontend
npm test
```

The backend tests cover handler validation and business logic, query filtering/pagination/date-range, dashboard aggregation, and the controllers under Moq, including role-based scoping, the dashboard's `401`/`403` access control, and `Result -> 404` mapping. The frontend tests cover the check-in form (mood selection, validation, edit pre-fill) and the list rendering.

---

## A few decisions worth calling out

- **`Result` over exceptions for control flow.** Handlers return `Result<T>` with an `ErrorType`; a small extension maps that to the right status code. Controllers stay branch-free and exceptions are reserved for the genuinely exceptional.
- **`IApplicationDbContext` instead of a repository.** A repository would mostly re-wrap what `DbContext` already gives me. The trade-off (handlers know they're talking to EF) is fine, and they stay unit-testable via the in-memory provider.
- **DTOs everywhere; entities never leave the application layer.** Reads use an EF-translatable projection so only the needed columns come back.
- **Validation in two places, on purpose.** FluentValidation gives friendly field-level API errors; the domain factory is the last line of defence so the entity can't exist in an invalid state.
- **Runtime seeding instead of EF `HasData`,** because check-ins are created through the factory with dates relative to "today", which keeps the dashboard chart populated without re-baking seed data into migrations.
- **The dashboard buckets by the caller's local day.** Timestamps are stored UTC, but the browser passes its timezone offset so the chart's dates line up with the times shown in the lists. The aggregation is done in memory because the dataset is small and it keeps things provider-agnostic.
