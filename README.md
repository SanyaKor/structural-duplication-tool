# Structural Duplication Tool

**Structural Duplication Tool** is a Roslyn-powered C# command-line utility that automates structural duplication of method signatures and parameters.  
It provides flag-based control for suffixes, comments, and file filtering — simplifying large-scale refactoring tasks.

---

## 🚀 Features

- Duplicate method parameters or entire method signatures
- Add custom suffixes to duplicated parameters
- Insert inline comments after modified signatures
- Filter processed files with patterns (e.g. `*Service.cs`)
- Analyze and rewrite C# code using the Roslyn API
- Output updated code to a custom directory

---

## ⚙️ Installation

Clone the repository and build:

```bash
git clone https://github.com/yourname/structural-duplication-tool.git
cd structural-duplication-tool
dotnet build
```

You can also run it directly with:

```bash
dotnet run --project structural-duplication-tool
```

---

## 🧩 Usage

Basic example:

```bash
dotnet run -- \\
  --input ./src \\
  --output ./out \\
  --duplicate-all \\
  --suffix "_dup" \\
  --filter *Service.cs
```

---

### ⚙️ CLI Flags and Defaults

| Flag | Description | Default | Example |
|------|--------------|----------|----------|
| `--input` | Path to source directory | `${projectRoot}/testing/Samples/` | `--input ./src` |
| `--output` | Output directory for modified files | `${projectRoot}/testing/Results/` | `--output ./out` |
| `--duplicate-all` | Duplicate all parameters (if omitted — duplicates only methods with one parameter) | `false` | `--duplicate-all` |
| `--suffix` | Suffix appended to duplicated parameters | `_duplicate` | `--suffix "_copy"` |
| `--comment` | Inline comment added after modified methods | `// duplicated parameter added` | `--comment "// dup"` |
| `--filter` | File name pattern to include | `*.cs` | `--filter *Service.cs` |

---

## 🧠 Example

### Input file (`UserService.cs`)

```csharp
public class UserService
{
    public void CreateUser(string name)
    {
        // logic
    }

    public int GetUser(int id)
    {
        return id;
    }
}
```

### After running:
```bash
dotnet run -- \\
  --input ./src \\
  --output ./out \\
  --duplicate-all \\
  --suffix "_dup" \\
```

### Output file (`UserService.cs`)

```csharp
public class UserService
{
    // duplicated parameter added
    public void CreateUser(string name, string name_dup)
    {
        // logic
    }

    // duplicated parameter added
    public int GetUser(int id, int id_dup)
    {
        return id;
    }
}
```

Console output:
```
[ParamsDuplicator] Processing 1 file
[ParamsDuplicator] Methods modified: 2
[ParamsDuplicator] Parameters duplicated: 2
[ParamsDuplicator] Saved to: ./out
```
---
