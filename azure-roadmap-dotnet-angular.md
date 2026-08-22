# Azure Learning Pathway for a .NET + Angular Developer
### Basics → Advanced, with hands-on labs and buildable projects

---

## How to use this roadmap

This is built around **one evolving product**, not 40 disconnected tutorials. You'll build a single app — *TaskFlow*, a team task-management SaaS — and each phase upgrades its architecture. By Phase 9 you'll have a production-grade, multi-tenant system that you can put on your resume and talk about in interviews.

Two rules that matter more than the content:

1. **Never watch without building.** Every phase has a "Build" section. If you skip it, you'll recognise Azure services in interviews but won't be able to use them.
2. **Delete your resources every Sunday.** Seriously. Wrap each phase's infra in a resource group and `az group delete` it when the phase ends. This is the single biggest cost saver and it forces you to learn Infrastructure-as-Code early.

**Assumed pace:** ~6–8 hours/week → roughly 7–8 months to complete everything. Phases 0–4 (~3 months) already make you employable as an "Azure .NET developer."

---

## Before you start — one-time setup

| Item | Notes |
|---|---|
| Azure free account | $200 credit for 30 days + a set of services free for 12 months + ~25 services always free. Requires a card for identity verification. |
| Azure CLI | `winget install Microsoft.AzureCLI` — you'll live in this more than the Portal |
| Azure Developer CLI (`azd`) | Scaffolds + deploys full stacks in one command. Underrated. |
| VS Code extensions | Azure Resources, Azure App Service, Azure Functions, Bicep, Azure Static Web Apps |
| Visual Studio 2022+ | Has "Publish to Azure" and Connected Services built in |
| .NET 9 SDK, Node LTS, Angular CLI | Your existing stack |
| Docker Desktop | Needed from Phase 5 onward |
| A GitHub account | CI/CD from Phase 1, non-negotiable |

### Cost guardrails (set these up on day one)

- Create a **Budget** with an alert at ₹500 / $10. Portal → Cost Management → Budgets.
- Prefer **Free (F1)** and **Basic (B1)** tiers while learning.
- The expensive traps: **AKS control plane + node pools**, **Application Gateway/WAF**, **APIM Developer tier**, **provisioned Cosmos DB throughput**, **Azure Firewall**, **VMs left running**. Use these in short bursts, then destroy.
- Use **Cosmos DB serverless** and **Azure SQL serverless with auto-pause**, not provisioned.
- Tag everything: `az group create -n rg-taskflow-p3 -l centralindia --tags phase=3 owner=me`

---

# Phase 0 — Cloud Foundations
**Time: 1–2 weeks · Difficulty: ★☆☆☆☆**

The goal here isn't depth, it's vocabulary. You need to stop being confused by "why is my resource in the wrong subscription."

### Learn
- IaaS vs PaaS vs SaaS — and why, as a .NET dev, **you should live in PaaS** and only touch IaaS when forced
- The resource hierarchy: Management Group → Subscription → **Resource Group** → Resource
- Regions and Availability Zones (use `Central India` / `South India` for latency, `East US` when a service isn't available in India yet — this happens more than you'd expect with new services)
- Azure Resource Manager (ARM) — everything, including the Portal, is just calling the ARM REST API
- Pricing models: pay-as-you-go, reserved instances, spot; the Pricing Calculator and TCO Calculator
- The Azure Portal, Cloud Shell, Azure CLI, and Azure PowerShell — and when each is right

### Hands-on lab
```bash
az login
az account show
az group create -n rg-taskflow-learn -l centralindia
az storage account create -n sttaskflowlearn001 -g rg-taskflow-learn --sku Standard_LRS
az resource list -g rg-taskflow-learn -o table
az group delete -n rg-taskflow-learn --yes --no-wait
```
Do the same five operations through the Portal, then compare. Notice the Portal is slower and less repeatable — that's the lesson.

### 🚀 Project 0 — "Hello Azure"
Deploy your existing Angular app to **Azure Static Web Apps**, connected to a GitHub repo. Every push to `main` auto-deploys via a GitHub Action, and every PR gets its own preview URL.

> ⚠️ **Gotcha:** For Angular 17+, the build output location must end with **`/browser`** (e.g. `dist/taskflow-web/browser`). This trips up nearly everyone on their first deploy.

Add a `staticwebapp.config.json` with a navigation fallback so Angular's client-side routing doesn't 404 on refresh:
```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/assets/*", "*.css", "*.js"]
  }
}
```

### ✅ You're done when
- You can explain a resource group to a colleague in 30 seconds
- Your Angular app is live on a `*.azurestaticapps.net` URL
- A `git push` redeploys it without you touching the Portal
- You have a budget alert configured

**Docs:** [Deploy an Angular app on Static Web Apps](https://learn.microsoft.com/azure/static-web-apps/deploy-angular) · [Static Web Apps overview](https://learn.microsoft.com/azure/static-web-apps/overview)

---

# Phase 1 — Hosting Your .NET Backend
**Time: 2 weeks · Difficulty: ★★☆☆☆**

### Learn
- **App Service** and **App Service Plans** — the plan is the VM you rent; the apps are what run on it. Multiple apps can share one plan. This distinction saves real money.
- Deployment methods: ZIP deploy, Run-From-Package, Git, container, GitHub Actions
- **Deployment slots** — staging slots with swap. This is your zero-downtime deployment story.
- **App settings and connection strings** — these override `appsettings.json` at runtime. Nested config uses `__` (double underscore): `ConnectionStrings__Default`, `Logging__LogLevel__Default`.
- Scaling: **scale up** (bigger plan) vs **scale out** (more instances), autoscale rules
- Kudu / SCM site (`https://<app>.scm.azurewebsites.net`) for log streaming and file browsing
- CORS, custom domains, managed TLS certificates
- Health check endpoints and Always On

### Hands-on lab
Deploy an ASP.NET Core Web API to App Service three different ways: Visual Studio publish, `az webapp up`, and a GitHub Action. Then create a `staging` slot, deploy a change there, verify it, and swap.

### 🚀 Project 1 — TaskFlow v1
Angular SPA on Static Web Apps + ASP.NET Core Web API on App Service. In-memory data for now.

Build these deliberately:
- Configure CORS on the API so the SWA origin is allowed
- Move the API base URL into Angular `environment.prod.ts` — no hardcoded localhost
- Add `/healthz` and wire it to App Service Health Check
- Enable Application Logging + Log Stream, and watch a live request come through

### ✅ You're done when
- Angular calls a real cloud API with no CORS errors
- You can do a slot swap and describe why it's better than redeploying prod
- You can change API behaviour by editing an App Setting, without redeploying

**Docs:** [App Service documentation](https://learn.microsoft.com/azure/app-service/)

---

# Phase 2 — Data & Storage
**Time: 3 weeks · Difficulty: ★★☆☆☆**

This is where the AZ-204 weight starts (<cite index="2-25">Develop for Azure storage is 15–20% of the exam</cite>).

### Learn

**Azure Blob Storage**
- Account types, access tiers (Hot / Cool / Cold / Archive), redundancy (LRS / ZRS / GRS / RA-GRS)
- Containers, blobs, **blob types** (block vs append vs page)
- **SAS tokens** — user delegation SAS vs service SAS vs account SAS. User delegation SAS (backed by Entra ID) is the one you should default to.
- Blob metadata and properties, lifecycle management policies
- The `Azure.Storage.Blobs` SDK — `BlobServiceClient` → `BlobContainerClient` → `BlobClient`

**Azure SQL Database**
- DTU vs vCore purchasing models; **serverless with auto-pause** (your learning tier)
- EF Core migrations against Azure SQL, connection resiliency / `EnableRetryOnFailure()`
- Elastic pools, firewall rules, Entra-based authentication
- Query Performance Insight and automatic tuning

**Azure Cosmos DB**
- NoSQL API, **partition key design** (the single most important decision — get it wrong and you can't fix it without a migration)
- Request Units (RU/s), provisioned vs **serverless** vs autoscale
- The five consistency levels and what each actually costs you
- **Change feed** — this becomes your event source later
- `Microsoft.Azure.Cosmos` SDK

**Also:** Azure Table Storage (cheap key-value), Azure Files, and Azure Cache for Redis basics.

### Hands-on lab
Write one console app that: uploads a file to Blob, generates a 15-minute read-only SAS URL for it, inserts a metadata row into Azure SQL via EF Core, and writes an audit document to Cosmos DB. One app, three data stores — you'll feel the difference in each SDK's shape.

### 🚀 Project 2 — TaskFlow v2: Attachments + Persistence
- Tasks now persist in Azure SQL via EF Core (code-first migrations run on startup or via a deploy step)
- Users attach files to tasks. **Critical design:** the Angular client uploads *directly to Blob Storage* using a short-lived SAS URL issued by the API — files never proxy through your API. This is the pattern real systems use and it's a great interview talking point.
- Activity log (who did what, when) goes into Cosmos DB, partitioned by `tenantId`
- Add a lifecycle policy that moves attachments to Cool tier after 30 days

### ✅ You're done when
- You can explain why partition key choice matters and give a bad example
- You can generate a SAS token in C# and use it from Angular's `HttpClient`
- You can articulate when you'd pick Cosmos over SQL (and honestly answer "usually SQL")

**Docs:** [Blob Storage](https://learn.microsoft.com/azure/storage/blobs/) · [Azure SQL](https://learn.microsoft.com/azure/azure-sql/database/) · [Cosmos DB](https://learn.microsoft.com/azure/cosmos-db/)

---

# Phase 3 — Identity & Security
**Time: 3 weeks · Difficulty: ★★★☆☆**

The phase most developers skip and most interviews probe. Don't skip it.

### Learn

**Microsoft Entra ID** (formerly Azure AD)
- Tenants, app registrations, service principals, enterprise applications
- OAuth 2.0 + OIDC flows — specifically **Authorization Code + PKCE** (what your Angular SPA uses) and **client credentials** (service-to-service)
- Delegated permissions vs application permissions; scopes vs app roles
- Access tokens vs ID tokens vs refresh tokens — know which one your API validates
- **MSAL Angular** (`@azure/msal-angular` + `@azure/msal-browser`) with `MsalInterceptor`
- **Microsoft.Identity.Web** on the API side
- Multi-tenant app registration (you'll need this for TaskFlow SaaS)
- Microsoft Graph basics

**Secrets & Access**
- **Managed Identity** — system-assigned vs user-assigned. The whole point: no secrets in config, ever.
- **Azure Key Vault** — secrets, keys, certificates; RBAC vs access policies; the Key Vault reference syntax in App Service: `@Microsoft.KeyVault(SecretUri=...)`
- **Azure App Configuration** — feature flags and centralised config, with dynamic refresh
- **Azure RBAC** — built-in roles, scope inheritance, custom roles
- `DefaultAzureCredential` from `Azure.Identity` — works locally with your `az login`, works in Azure with Managed Identity. Same code both places.

### Hands-on lab
Take an app with a connection string in `appsettings.json` and remove it entirely. Store it in Key Vault, grant the App Service's system-assigned managed identity the *Key Vault Secrets User* role, and reference it from App Settings. Then go further: drop the connection string secret altogether and use Managed Identity to authenticate directly to Azure SQL.

### 🚀 Project 3 — TaskFlow v3: Real Auth
- Register two apps in Entra ID: the SPA and the API. Expose an API scope (`api://taskflow-api/Tasks.ReadWrite`), grant it to the SPA.
- Angular: MSAL login, silent token acquisition, `MsalInterceptor` attaching bearer tokens, route guards
- .NET API: `[Authorize]`, scope validation with `RequiredScope`, **app roles** for Admin vs Member
- Zero secrets in the repo — Key Vault + Managed Identity for everything
- Add a feature flag via App Configuration and toggle a UI feature without redeploying

### ✅ You're done when
- Your API returns 401 for an unauthenticated call and 403 for an authenticated one lacking the right role
- Your codebase contains zero connection strings or keys
- You can draw the auth code + PKCE flow on a whiteboard

**Docs:** [Microsoft identity platform](https://learn.microsoft.com/entra/identity-platform/) · [Key Vault](https://learn.microsoft.com/azure/key-vault/) · [Managed identities](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/)

---

# Phase 4 — Serverless & Messaging
**Time: 3–4 weeks · Difficulty: ★★★☆☆**

This is where you stop building a website and start building a *system*.

### Learn

**Azure Functions**
- **Isolated worker model** for .NET (in-process is legacy — don't learn it)
- Hosting plans: Consumption, Flex Consumption, Premium, Dedicated. Know cold start trade-offs.
- Triggers: HTTP, Timer (CRON), Blob, Queue, Service Bus, Cosmos DB change feed, Event Grid
- Input and output bindings — how to do a lot with almost no code
- `host.json`, `local.settings.json`, Core Tools for local dev
- **Durable Functions** — orchestrator / activity / entity functions; fan-out-fan-in, function chaining, human interaction, and the **saga pattern** for distributed transactions

**Messaging — pick the right one**

| Service | Use it for |
|---|---|
| **Storage Queues** | Simple, cheap, at-least-once work queues |
| **Service Bus** | Enterprise messaging: topics/subscriptions, sessions (FIFO), dead-letter queues, transactions, duplicate detection, scheduled messages |
| **Event Grid** | Reactive event *notifications* (something happened), push-based, discrete events |
| **Event Hubs** | High-throughput telemetry *streams*, partitioned, replayable |

Being able to explain the difference between Service Bus, Event Grid and Event Hubs is a classic senior-level interview question.

**Also:** Azure SignalR Service — this is your Angular real-time story and it's genuinely delightful.

### Hands-on lab
Build a Durable Function that fans out to process 100 items in parallel, aggregates results, and exposes a status endpoint your Angular app polls for progress.

### 🚀 Project 4 — TaskFlow v4: Event-Driven
- When a task is created, the API publishes a message to a **Service Bus topic**
- Three subscribers, each a Function: send notification email, update a reporting projection, index for search
- **Blob-triggered Function** generates thumbnails for image attachments
- **Timer-triggered Function** runs nightly to send digest emails and archive stale tasks
- **Durable Function** orchestrates task approval: request → wait for approver (with timeout) → escalate or complete
- **SignalR Service** pushes live task updates to the Angular board — no polling, cards move in real time across browsers
- Handle poison messages properly: configure max delivery count and monitor the dead-letter queue

### ✅ You're done when
- Your app still works if a downstream Function is broken (messages queue up, then drain)
- You can explain idempotency and why your message handlers need it
- Two browser windows show the same board updating live

**Docs:** [Azure Functions](https://learn.microsoft.com/azure/azure-functions/) · [Durable Functions](https://learn.microsoft.com/azure/azure-functions/durable/) · [Service Bus](https://learn.microsoft.com/azure/service-bus-messaging/) · [SignalR Service](https://learn.microsoft.com/azure/azure-signalr/)

---

# Phase 5 — Containers
**Time: 3 weeks · Difficulty: ★★★☆☆**

### Learn

**Docker fundamentals for your stack**
- Multi-stage Dockerfile for ASP.NET Core (SDK image builds, runtime image ships — your final image should be ~110MB, not 800MB)
- Multi-stage Dockerfile for Angular (Node builds, **nginx** serves)
- `.dockerignore`, layer caching, why `COPY *.csproj` before `COPY .` matters
- Docker Compose for local multi-service dev

**Azure Container Registry (ACR)**
- Tiers, geo-replication, **ACR Tasks** (build in the cloud), image scanning, admin user vs Managed Identity pull

**Azure Container Apps** — *this should be your default container platform*
- Built on Kubernetes but hides it. Revisions, traffic splitting, ingress.
- **KEDA-based autoscaling**, including **scale-to-zero** and scaling on queue depth
- Dapr integration for service invocation, pub/sub, state
- Secrets and Managed Identity

**Azure Kubernetes Service (AKS)** — learn it, but know when you *don't* need it
- Pods, Deployments, Services, ConfigMaps, Secrets, Ingress
- `kubectl` essentials, node pools, HPA
- Helm charts
- Workload Identity (the modern way to give pods Azure access)

**Also:** Azure Container Instances (ACI) for one-off burst jobs.

### 🚀 Project 5 — TaskFlow v5: Containerised
1. Write production Dockerfiles for both API and Angular. Get the API image under 150MB.
2. Build and push to ACR using ACR Tasks (no local Docker needed).
3. Deploy to **Container Apps**: API scales 0→10 on HTTP load, a background worker scales on Service Bus queue depth.
4. Do a **canary deployment** — 10% traffic to a new revision, verify, then shift to 100%.
5. *Then* deploy the same images to AKS with an ingress controller and a Helm chart — purely so you understand what Container Apps is doing for you.

### ✅ You're done when
- Your worker container scales to zero when the queue is empty (check the bill — it's ₹0)
- You can argue for Container Apps over AKS for a small team, with specifics

**Docs:** [Container Apps](https://learn.microsoft.com/azure/container-apps/) · [ACR](https://learn.microsoft.com/azure/container-registry/) · [AKS](https://learn.microsoft.com/azure/aks/)

---

# Phase 6 — DevOps & Infrastructure as Code
**Time: 3 weeks · Difficulty: ★★★☆☆**

### Learn

**Bicep** (learn this before Terraform — it's Azure-native and far more readable than raw ARM JSON)
- Resources, parameters, variables, outputs, modules
- `existing` references, loops, conditions
- **`what-if`** deployments — dry run before you break prod
- Deployment scopes: resource group, subscription, tenant
- Azure Verified Modules (AVM) — don't write everything from scratch

**CI/CD**
- **GitHub Actions**: workflows, jobs, matrix builds, environments with approval gates, **OIDC federated credentials** (stop using long-lived publish profiles and service principal secrets)
- **Azure DevOps Pipelines**: multi-stage YAML, variable groups linked to Key Vault, service connections, environments — you'll meet this in most Indian enterprise shops
- Build + test + deploy for both .NET and Angular in one pipeline
- Deployment strategies: blue-green via slots, canary via Container Apps revisions, feature-flag-driven release

**Azure Developer CLI (`azd`)** — `azd up` provisions infra and deploys code in one shot. Excellent for spinning up phase sandboxes.

### 🚀 Project 6 — TaskFlow v6: One Command, Full Stack
- Every resource from Phases 1–5 defined in **Bicep modules**: `main.bicep` + `modules/appservice.bicep`, `sql.bicep`, `servicebus.bicep`, `keyvault.bicep`, etc.
- Parameterised per environment: `dev.bicepparam`, `prod.bicepparam`
- A GitHub Actions workflow that: builds .NET + Angular → runs tests → runs `what-if` → deploys infra → deploys apps → runs EF migrations → smoke tests → swaps slots
- Prod stage gated behind a manual approval in a GitHub Environment
- OIDC auth to Azure — zero stored secrets in GitHub

### ✅ You're done when
- You can delete your entire resource group and rebuild everything with one command
- Your pipeline fails safely when tests fail, and never deploys a broken build
- No credentials exist in GitHub secrets except the OIDC config

**Docs:** [Bicep](https://learn.microsoft.com/azure/azure-resource-manager/bicep/) · [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/)

---

# Phase 7 — Observability
**Time: 2 weeks · Difficulty: ★★★☆☆**

Small on the exam (<cite index="1-0">Monitor and troubleshoot is 5–10%</cite>) but enormous in real jobs. This is what separates "deployed it" from "operates it."

### Learn
- **Application Insights**: the .NET SDK, the **Angular/JS SDK** (`@microsoft/applicationinsights-web`), correlation between them
- **Distributed tracing** — `operation_Id` flowing from an Angular click → API → Service Bus → Function → SQL, all in one Transaction view
- **OpenTelemetry** in .NET — the direction Microsoft is heading; `Azure.Monitor.OpenTelemetry.AspNetCore`
- **KQL (Kusto Query Language)** — genuinely worth two focused days:
  ```kusto
  requests
  | where timestamp > ago(24h)
  | summarize p50=percentile(duration,50), p95=percentile(duration,95), count() by name
  | order by p95 desc
  ```
- Live Metrics, Application Map, Failures blade, Performance blade
- Availability tests (standard + custom TrackAvailability)
- Log Analytics workspaces, diagnostic settings, metric alerts, action groups
- Structured logging with Serilog → App Insights sink
- Workbooks and dashboards

### 🚀 Project 7 — TaskFlow v7: Full Observability
- Instrument Angular *and* API *and* Functions into one App Insights resource
- Prove end-to-end correlation: click a button in Angular, then find that exact user action, its API call, the Service Bus message, and the Function execution in a single trace
- Build a KQL workbook: request volume, p95 latency by endpoint, error rate, dependency failures, slowest SQL queries
- Alert rules: p95 > 2s for 5 min, error rate > 5%, dead-letter queue depth > 0
- Add a custom event (`TaskCompleted`) and chart completion rates by tenant

### ✅ You're done when
- Given a "the app is slow" complaint, you can find the actual bottleneck in under 5 minutes
- You get an email before your users notice a problem

**Docs:** [Azure Monitor](https://learn.microsoft.com/azure/azure-monitor/) · [Application Insights](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview/)

---

# Phase 8 — Networking, Gateways & Advanced Architecture
**Time: 4 weeks · Difficulty: ★★★★☆**

### Learn

**API Management (APIM)**
- Products, APIs, operations, subscriptions
- **Policies** — the killer feature: rate limiting, quota, JWT validation, caching, request/response transformation, CORS, mock responses, retry, circuit breaker
- Versioning and revisions
- Self-hosted gateway, Developer Portal
- Tiers — note the Consumption tier for learning; Developer tier is *not* cheap to leave running

**Edge & routing**
- **Azure Front Door** — global L7, CDN, WAF, routing rules, caching
- **Application Gateway** — regional L7 + WAF, path-based routing
- **Traffic Manager** — DNS-level global routing
- Know which to pick and why (Front Door for global multi-region; App Gateway for regional inside a VNet)

**Networking**
- VNets, subnets, NSGs, service endpoints
- **Private Endpoints** vs **Service Endpoints** — and why Private Endpoints are the modern answer
- VNet Integration for App Service / Functions / Container Apps
- Private DNS zones (this is where most people get stuck — the DNS resolution is the hard part, not the endpoint)
- Hub-and-spoke topology, Azure Bastion

**Performance & resilience**
- **Azure Cache for Redis** — cache-aside pattern, output caching, distributed session, `IDistributedCache`
- Resilience patterns with **Polly** / `Microsoft.Extensions.Http.Resilience`: retry with exponential backoff + jitter, circuit breaker, timeout, bulkhead
- Idempotency, outbox pattern, optimistic concurrency
- Multi-region active-passive vs active-active; RTO/RPO; Azure SQL failover groups; Cosmos multi-region writes

**Frameworks to read properly**
- **Azure Well-Architected Framework** — five pillars: Reliability, Security, Cost Optimization, Operational Excellence, Performance Efficiency
- **Azure Architecture Center** reference architectures

### 🚀 Project 8 — TaskFlow v8: Enterprise Grade
- **Front Door** in front of everything: global entry, WAF enabled, static content cached at edge
- **APIM** in front of the API: JWT validation at the gateway, 100 req/min rate limit per subscription, response caching on read endpoints, a mocked v2 endpoint
- **Redis** caching for the task list, with proper invalidation on write
- **Private Endpoints** for SQL, Storage, Key Vault and Service Bus; public network access **disabled**; App Service reaching them via VNet integration
- Polly policies on every outbound HTTP and SQL call
- Write a one-page **WAF self-assessment** of your own architecture, honestly scoring each pillar and listing what you'd fix with a real budget

### ✅ You're done when
- Your SQL server rejects connections from the public internet, and your app still works
- You can draw the full architecture from memory and defend every box in it

**Docs:** [API Management](https://learn.microsoft.com/azure/api-management/) · [Front Door](https://learn.microsoft.com/azure/frontdoor/) · [Well-Architected Framework](https://learn.microsoft.com/azure/well-architected/) · [Architecture Center](https://learn.microsoft.com/azure/architecture/)

---

# Phase 9 — Specialise
**Time: ongoing · Difficulty: ★★★★★**

Pick **one** based on where you want your career to go. Don't do all three.

### Track A — AI Engineering *(highest demand right now)*
- **Azure OpenAI Service** / Azure AI Foundry — deployments, quotas, content filters
- **Semantic Kernel** for .NET — plugins, planners, function calling
- **Azure AI Search** — vector search, hybrid search, semantic ranking
- **RAG architecture** end to end: chunking → embedding → indexing → retrieval → generation → evaluation
- Prompt engineering, token cost management, streaming responses to Angular via SSE
- **Project:** TaskFlow AI — natural language task creation ("remind me to review the PR Friday"), semantic search over tasks and attachments, auto-generated standup summaries. Stream responses into Angular.
- **Cert:** AI-102

### Track B — Solutions Architecture
- Landing zones, Azure Policy, Blueprints, management group hierarchy
- Governance, cost management at scale, chargeback
- Migration strategies (the 6 Rs), Azure Migrate
- Hybrid: Arc, ExpressRoute, VPN Gateway
- **Cert:** AZ-305

### Track C — Platform / DevOps Engineering
- Advanced AKS: service mesh (Istio), GitOps with Flux/ArgoCD, cluster autoscaler, Karpenter
- Terraform on Azure (you'll need it — most enterprises are multi-cloud)
- Advanced pipeline design, artifact management, supply chain security, SBOMs
- Azure Chaos Studio, SRE practices, error budgets
- **Cert:** AZ-400

---

# 🏆 Capstone — TaskFlow SaaS

Build this over 4–6 weeks after Phase 8. It's what you'll actually show people.

**Multi-tenant task management SaaS** with:

- Multi-tenant Entra ID (external orgs sign in with their own Microsoft accounts)
- Tenant isolation at the data layer (row-level in SQL, partition key in Cosmos)
- Subscription tiers with feature flags via App Configuration
- Real-time collaboration via SignalR Service
- AI-assisted task creation and summaries
- Full IaC in Bicep, multi-environment CI/CD
- Front Door + APIM + private networking
- Complete observability with SLO dashboards
- **A written architecture decision record (ADR) for each major choice** — this is what senior interviews actually dig into

Write a README with an architecture diagram and a cost breakdown. Push it to GitHub. Write a blog post about the hardest part. This combination is worth more than any certificate.

---

# Certification Track

Certs are a hiring filter, not an education. Build first, certify second — but they do help with recruiters, especially in the Indian market where they're screened for heavily.

| Order | Cert | When | Notes |
|---|---|---|---|
| 1 | **AZ-900** (Fundamentals) | After Phase 0 | Optional if you're already a working dev. 1 week of prep. |
| 2 | **AZ-204** (Developer Associate) | After Phase 7 | **The one that matters for you.** |
| 3 | **AZ-400** or **AZ-305** | After Phase 9 | DevOps Engineer Expert / Solutions Architect Expert |
| 4 | **AI-102** | Track A only | AI Engineer Associate |

### AZ-204 weighting (as of January 2026)

<cite index="2-14,2-15,2-16,2-17,2-18">Develop Azure compute solutions 25–30%; Develop for Azure storage 15–20%; Implement Azure security 15–20%; Monitor, troubleshoot, and optimize 5–10%; Connect to and consume Azure services and third-party services 20–25%.</cite>

Map that to this roadmap: **Phases 1, 4, 5** cover compute; **Phase 2** covers storage; **Phase 3** covers security; **Phase 7** covers monitoring; **Phases 4 and 8** cover the connect/consume domain. If you finish Phase 8, you're over-prepared.

<cite index="9-3">The certification renews every 12 months</cite> via a free online assessment — set a calendar reminder.

**Prep resources:** [AZ-204 study guide](https://learn.microsoft.com/credentials/certifications/resources/study-guides/az-204) · [Official AZ-204 course](https://learn.microsoft.com/training/courses/az-204t00) · [Certification page](https://learn.microsoft.com/credentials/certifications/azure-developer/)

---

# Suggested Timeline

| Month | Phases | Milestone |
|---|---|---|
| 1 | 0, 1 | Angular + .NET API live in Azure with CI/CD |
| 2 | 2 | Real data, file uploads, three data stores |
| 3 | 3 | Proper enterprise auth, zero secrets |
| 4 | 4 | Event-driven, real-time, serverless |
| 5 | 5, 6 | Containerised, fully reproducible infra |
| 6 | 7 | Observable — and sit **AZ-204** |
| 7 | 8 | Enterprise architecture |
| 8+ | 9 + Capstone | Specialise and ship the portfolio piece |

---

# A few honest notes

**On the Portal.** Use it to explore and to read; use the CLI and Bicep to do. If you find yourself clicking the same sequence twice, script it.

**On depth vs breadth.** Azure has 200+ services. You will never learn them all, and nobody expects you to. The ~25 services in this roadmap cover 90% of what .NET shops actually run. Depth in those beats shallow familiarity with fifty.

**On the "advanced" label.** What makes an engineer senior on Azure isn't knowing more services — it's knowing which service *not* to use. Container Apps over AKS. App Service over VMs. SQL over Cosmos, usually. Queue over a database poll. Practise justifying the boring choice.

**On cost.** Everything here can be done for well under ₹3,000/month total if you delete resource groups between phases and stay on free/basic tiers. The one thing that will genuinely hurt you is leaving an AKS cluster, an Application Gateway, or an APIM Developer instance running for a month. Set that budget alert.

**On getting stuck.** When something doesn't work in Azure, the answer is almost always in one of three places: the Activity Log (what actually happened), the resource's Diagnose and Solve Problems blade (surprisingly good), or a networking/DNS misconfiguration (if it's Phase 8).
