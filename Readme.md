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

## 🛠️ Tech Stack Overview
* **Framework:** ASP.NET Core Web API (.NET 9)
* **Database & ORM:** MySQL & Entity Framework Core
* **Security:** JWT Authentication, BCrypt, Server-side RBAC & BOLA Guards