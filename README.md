# Sentinel.AISafety
Sentinel is a high-performance .NET 9 Middleware gateway designed to provide a secure boundary between users and Large Language Models (LLMs). It specializes in intent validation, prompt injection prevention, and PII redaction for local LLM deployments (Ollama).

Key Features :

Request Interception: Validates user prompts against a dynamic security policy before they reach the LLM.

Response Filtering: Scans AI-generated content to prevent "Instruction Leaks" or the generation of prohibited content.

Dynamic Policy Engine: JSON-based rules allow for "Block," "Redact," and "Massage" actions without restarting the service.

PII Masking: Automatically detects and redacts emails and sensitive credentials from both inputs and outputs.

Audit Logging: Integrated with Azure Table Storage to log security violations and safety events for future analysis.

Architecture

The system operates as a specialised layer in the ASP.NET Core pipeline:

Extract: Reads the JSON payload from the HTTP request.

Evaluate (Input): Passes the prompt through the SafetyEngine using fuzzy regex matching.

Process/Short-Circuit: If a "Block" rule is triggered, the middleware returns a 403 Forbidden immediately.

LLM Communication: Forwards safe/transformed prompts to the Ollama API.

Evaluate (Output): Scans the LLM response to ensure it didn't generate restricted information.

Deliver: Returns the sanitized response to the client.

Prerequisites

.NET 9 SDK
Ollama (running locally)
Azure Storage Emulator or Account (for logging)

<img width="703" height="595" alt="Screenshot 2026-03-17 at 8 02 07 AM" src="https://github.com/user-attachments/assets/592e3e40-8de9-433d-a0ff-91c37067f7f2" />
