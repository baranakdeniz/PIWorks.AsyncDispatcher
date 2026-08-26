# Async Dispatcher Core Architecture

This repository contains the core architectural skeleton of an asynchronous command dispatching and background worker system designed for backend processing.

## 🏗️ Architecture Components

* **CommandEnvelope (Wrapper):** Encapsulates the actual command along with its unique tracking ID (`TKey`) and metadata (`CommandType`) to prevent code duplication in workers.
* **IAsyncCommand & IAsyncCommandHandler:** Generic contracts defining how commands and their respective handlers should be structured.
* **AsyncCommandWorker:** A background worker service designed to dequeue envelopes, resolve handlers dynamically using Reflection, and process commands asynchronously.
* **Exception & Cancellation Handling:** Specialized handlers (`IFailureHandler`, `ICancellationHandler`) to manage operational failures and cancellations safely.

##  Current Status & Roadmap
- [x] Core architecture setup (Envelope, Worker, Handler contracts).
- [ ] InMemory / Channel based CommandBus implementation.
- [ ] Unit and Integration tests.

## 📂 Project Structure
- `Core.Abstractions`: Contains generic interfaces and envelope definitions.
- `Core.Workers`: Background worker implementation for background processing.