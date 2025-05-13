# 🔌 Socket Programming Project

This project demonstrates a simple **Client-Server architecture** using **TCP sockets** in **C#**. The server handles multiple clients **concurrently** with **asynchronous programming** and secure **AES encryption** 🔒.

## 📦 Setup Instructions

1️⃣ **Clone the Repository**:
```bash
git clone <repository-url>
cd SocketProgramming
```

2️⃣ **Install Dependencies**:
Ensure you have .NET SDK installed. You can install it from [Microsoft's official site](https://dotnet.microsoft.com/download).
```bash
dotnet restore
```

3️⃣ **Run the Server**:
```bash
cd SocketProgramApp
dotnet run
```

4️⃣ **Run the Client**:
```bash
cd SocketProgramClient
dotnet run
```

## 🛠️ Detailed Project Setup

### 🌐 Environment Setup

🔹 **Prerequisites**:
- .NET SDK ➡️ [Download](https://dotnet.microsoft.com/download)
- IDE: Visual Studio / Visual Studio Code

🔹 **Environment Variables**:
For encryption configuration:
- `SOCKET_AES_KEY`: AES key (base64, 32 bytes for AES-256)
- `SOCKET_AES_IV`: Initialization Vector (base64, 16 bytes)
- `SOCKET_PORT`: Server port (default `5000`)

PowerShell:
```powershell
$env:SOCKET_AES_KEY = "D8NMTh4bgaFtkjiuSgCPLRhn2bRrnOttIvMyjxLkJeg="
$env:SOCKET_AES_IV = "Xom+k4qwejPYhGUOeTjFLg=="
$env:SOCKET_PORT = "5000"
```

Command Prompt:
```cmd
setx SOCKET_AES_KEY "D8NMTh4bgaFtkjiuSgCPLRhn2bRrnOttIvMyjxLkJeg="
setx SOCKET_AES_IV "Xom+k4qwejPYhGUOeTjFLg=="
setx SOCKET_PORT "5000"
```

🔹 **Data Configuration**:
Create `data.json` in the root directory:
```json
{
  "SetA": { "One": 1, "Two": 2 },
  "SetB": { "Three": 3, "Four": 4 },
  "SetC": { "Five": 5, "Six": 6 },
  "SetD": { "Seven": 7, "Eight": 8 },
  "SetE": { "Nine": 9, "Ten": 10 }
}
```

## ▶️ Running the Application

🖥️ **Run the Server**:
```bash
cd SocketProgramApp
dotnet run
```

💻 **Run the Client**:
```bash
cd SocketProgramClient
dotnet run
```

## 🧪 Sample Input and Output

📥 **Input**: Client sends `SetA-One`  
- The server retrieves the subset for `SetA`, which is `[{"One":1,"Two":2}]`, and fetches the value `1`.  
- The server sends the current timestamp as a response **1 time**.

📤 **Output**: 
```
13-05-2025 11:52:09  
```

📥 **Input**: Client sends `SetA-Two`  
- The server retrieves the subset for `SetA`, which is `[{"One":1,"Two":2}]`, and fetches the value `2`.  
- The server sends the current timestamp as a response **2 times** with a 1-second interval.

📤 **Output**: 
```
13-05-2025 11:52:14  
13-05-2025 11:52:15  
```

📥 **Input**: Client sends `SetB-Three`  
- The server retrieves the subset for `SetB`, which is `[{"Three":3,"Four":4}]`, and fetches the value `3`.  
- The server sends the current timestamp as a response **3 times** with a 1-second interval.

📤 **Output**: 
```
13-05-2025 11:52:23  
13-05-2025 11:52:24  
13-05-2025 11:52:25  
```

📥 **Input**: Client sends any invalid key (e.g., `SetX`, `SetY`, etc.)  
- The server does not recognize the key and responds with `EMPTY`.

📤 **Output**: 
```
Received: EMPTY  
```

📥 **Input**: Client sends `SetZ`  
- The server does not recognize the key `SetZ`, as it is not present in the dataset.  
- The server responds with `EMPTY`.

📤 **Output**: 
```
Received: EMPTY  
```

## ⚙️ Asynchronous Model for Concurrency

Server uses async/await to handle multiple clients concurrently:
```csharp
public async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
{
    // Handle client communication
}
```

## 🔒 Security Measures

1. **Input Sanitization** 🛡️
2. **Cancellation Tokens** 🚦
3. **Error Handling** 🛠️

## 🛡️ Encryption and Decryption

- **Encryption**: Data encrypted before transmission 🔐
- **Decryption**: Received data decrypted securely 🔓

Keys & IV are configurable via environment variables for enhanced security 🔏.
