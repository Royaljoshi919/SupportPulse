# 🚀 SupportPulse Backend Architecture & Implementation Guide

SupportPulse is a secure, enterprise-grade backend ticketing system designed with a strict **Zero Trust** and **Defense-in-Depth** architecture using C# and .NET 9.

---

## 📅 Day 1 & 2: Foundation, Database & Login (The Foundation)

### **What was done:**
* **Base Setup:** Initialized the project using .NET 9 and Entity Framework Core (EF Core).
* **Database Connection:** Integrated a MySQL database to securely persist user accounts and core entities.
* **Authentication System:** Implemented secure User Registration and Login endpoints powered by **JWT (JSON Web Tokens)** and BCrypt hashing. Upon successful authentication, the server issues a secure token to verify the user's identity.

> **💡 Simple Terms:** Think of this as the foundation and the main security gate of a house—only valid users with proper credentials and verified tokens are allowed entry.

---

## 📅 Day 3: Multimedia Uploads (Audio & Images)

### **What was done:**
* **Multi-modal Ticket Ingestion:** Upgraded the ticket creation workflow. Customers are no longer limited to text—they can now describe issues via recorded audio voice notes or upload error screenshots/images.
* **File Storage Mechanism:** Implemented secure file handling on the backend server to safely store and manage incoming media assets linked to tickets.

> **💡 Simple Terms:** This provides convenience where customers don't always have to type long descriptions; they can simply snap a photo or send a quick voice note.

---

## 📅 Day 4: Super Security & Status Rules (BOLA & State Machine)

### **What was done:**
* **BOLA / IDOR Protection:** Enforced strict server-side ownership verification on ticket retrieval (`GET /api/v1/tickets/{id}`). If a customer attempts to access another user's ticket via URL manipulation, the server immediately returns a `403 Forbidden` response.
* **Status State Machine:** Implemented rigid lifecycle rules for ticket progression (`OPEN` ➔ `IN_PROGRESS` ➔ `RESOLVED` ➔ `CLOSED`), rejecting any invalid or skipped state transitions.
* **Role-Based Access Control (RBAC):** Restricted status modification endpoints to authorized personnel using `[Authorize(Roles = "AGENT,ADMIN")]`.

> **💡 Simple Terms:** This acts as the internal security guard of the project, ensuring no unauthorized user can view someone else's data or illegally modify ticket statuses out of order.

---
📅 Day 5: Agent Search, Dynamic Filtering & Server-Side Pagination
What was done:
Agent Dashboard Endpoint: Built the GET /api/v1/tickets/agent/tickets endpoint allowing Support Agents and Admins to search, filter, and manage all system tickets.

Dynamic Multi-Criteria Filtering: Implemented LINQ-based dynamic filtering (AsQueryable) supporting Status, Priority, Category, and Sentiment.

Keyword Text Search: Integrated case-insensitive full-text searching across ticket Subject and Description fields.

High-Performance Offset Pagination: Optimized query performance for 10,000+ records using .Skip() and .Take(), returning frontend-friendly metadata (totalRecords, totalPages, pageNumber, pageSize).

DB Schema Sync & Security: Enforced strict RBAC ([Authorize(Roles = "AGENT,ADMIN")]) and synced MySQL schema by adding nullable Category and Sentiment columns.

💡 Simple Terms: Think of this as giving support agents a high-speed search engine and custom control panel—allowing them to find specific tickets instantly out of thousands without slowing down the server.

## 🛠️ Tech Stack Overview
Framework: ASP.NET Core Web API (.NET 9)

Database & ORM: MySQL & Entity Framework Core (EF Core)

Security: JWT Authentication, BCrypt Hashing, Server-Side RBAC & BOLA Guards

Query & Performance: Dynamic LINQ Querying, Deferred Execution (AsQueryable), Server-Side Offset Pagination


## 🚀 Recent Updates: Async Queue & Background Processing (Day 8 & 9)[cite: 1]

### 1. Optimized Knowledge Base Lookup
* **Endpoint:** `GET /api/v1/knowledge-base`
* **Optimization:** Refactored query execution using EF Core `.AnyAsync()` to perform lightweight boolean checks (`ELIGIBLE` / `NOT ELIGIBLE`), avoiding heavy data transfers.

### 2. Asynchronous AI Job Queue Architecture (Day 8)[cite: 1]
* **Decoupled Architecture:** Separated ticket creation from long-running AI processing pipelines to guarantee sub-200ms API response times[cite: 1].
* **Database & Domain Models:** 
  * Updated `ai_jobs` table schema to track `updated_at` timestamps[cite: 1].
  * Mapped `AiJob.cs` entity within `ApplicationDbContext`[cite: 1].
* **Controller Logic:** Updated `TicketController.CreateTicket` to automatically enqueue a `PENDING` AI job upon ticket persistence[cite: 1].
* **Response DTO:** Extended `TicketResponseDto` to expose `AiStatus` (`PENDING`) directly to clients[cite: 1].

### 3. Background Worker Engine (Day 9)[cite: 1]
* **Worker Service:** Implemented `AIEnrichmentWorker.cs` extending `.NET BackgroundService` for continuous 24/7 background task polling[cite: 1].
* **Scoped Dependency Resolution:** Integrated `IServiceScopeFactory` to safely manage scoped `ApplicationDbContext` instances inside a singleton background worker.
* **Job State Machine:** Configured state transitions for fault tolerance:
  `PENDING` ➔ `PROCESSING` (Job Lock) ➔ `COMPLETED` / `FAILED`[cite: 1].


  🚀 Day 9 & 10: Background Worker & AI Transcription Pipeline

1. Background Worker Implementation (Day 9)

Background Service: Implemented AIEnrichmentWorker.cs using .NET BackgroundService to run tasks asynchronously in the background without blocking API responses.

Service Scopes & DI: Configured IServiceScopeFactory to manage Entity Framework Core's scoped ApplicationDbContext inside the singleton worker.

Job State Machine: Successfully implemented continuous polling from the ai_jobs table, managing the lifecycle states: PENDING ➔ PROCESSING (Job Lock) ➔ COMPLETED.

2. Audio-to-Text Pipeline Initial Setup (Day 10)

Database & Domain Modeling: Created and verified the ticket_ai_results table in MySQL to store AI outputs. Mapped the TicketAiResult.cs model in Entity Framework Core.

Mock AI Processing: Implemented a simulated asynchronous wait and a "Mock Transcription" generator in the worker to test end-to-end database writes before integrating the actual OpenAI Whisper API.

Configuration Ready: Prepared appsettings.json for external API keys and registered HttpClient in Program.cs to allow outbound calls to AI services.

## 🎙️ Day 10 — Audio → Text (Whisper API Integration)

### 📌 Objective
Customer support tickets mein attached audio messages (.mp3, .wav) ko automatically text mein transcribe karna background worker service ke zariye.

### ⚙️ Implementation Details
- **Background Worker:** Updated `AIEnrichmentWorker` service to handle background queue execution asynchronously.
- **AI Model Integration:** Integrated Groq API (`whisper-large-v3`) for audio transcription using `IHttpClientFactory`.
- **Payload & Data Flow:** Handled `MultipartFormDataContent` to stream audio files securely to the Groq API endpoint.
- **Database Tracking:**
  - **`ai_jobs`**: Managed status state machine (`PENDING` ➔ `PROCESSING` ➔ `COMPLETED` / `FAILED`).
  - **`ticket_ai_results`**: Stored extracted transcription text into the `transcription` column for the target ticket.

### 🔄 Execution Flow
1. Worker identifies and picks a `PENDING` job from the `ai_jobs` table.
2. Fetches associated audio attachment path from `ticket_files`.
3. Sends audio stream payload to Groq Whisper API endpoint (`/v1/audio/transcriptions`).
4. Parses JSON response text and inserts record into `ticket_ai_results`.
5. Marks `ai_jobs` status as `COMPLETED`.

### ✅ Result
Audio-to-text pipeline successfully built, tested, and verified with MySQL database persistence.