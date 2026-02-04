# FolderGuard

FolderGuard is a file protection system primarily designed for server deployment.<br>
Built on **.NET 9** following a decoupled CLI-service philosophy to enable standalone functionality for individual components while maintaining an automated workflow.

## Features
* **Encryption:** AES-256
* **HMAC Integrity:** For tamper detection
* **Event-Driven Monitoring:** Uses `FileSystemWatcher` for real-time file detection.
* **Parallel Processing:** Processes multiple files simultaneously.
* **Auto-Cleanup:** Automatically removes unencrypted source files for security.

---

## Architecture

### 1. FolderGuard.Engine
A CLI tool used for encryption tasks.
* **Input:** Accepts JSON via `stdin` containing file details and customizable PBKDF2 iterations.
* **Security:** Using `stdin` for data transfer is more secure than command-line arguments, which can be visible in process logs.
* **HMAC:** Generates a hash that acts as a seal, if the file is modified, the hash won't match and decryption will fail.



### 2. FolderGuard.Service
A background service that automates the encryption workflow.
* **Queuing:** Uses `System.Threading.Channels` to queue file paths for encryption across multiple threads.
* **Reliability:** Verifies a file is not currently being written to by another process before attempting encryption.

---

## Data Structures
* **`EncryptRequest`**: The DTO containing the action, password bytes, and file paths.
* **`EncryptionOptions`**: Holds configuration like `PathToWatch`, `OutputDir`, and `MaxParallelism`.

---

## Setup & Usage

### Requirements
* **.NET 9 SDK**

### Build
Compile both projects from the root directory:
```bash
dotnet build FolderGuard.Engine
dotnet build FolderGuard.Service
```

---

## TODO
* Implement unencrypted obscured log
* Implement encrypted verbose log
* Implement Kyber key exchange with AES-256-GCM for quantum resistance
