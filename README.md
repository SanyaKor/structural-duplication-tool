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
dotnet run -- \
  --input ./src \
  --output ./out \
  --duplicate-all \
  --suffix _dup \
  --comment \
  --filter *Service.cs
```

### Options

| Flag | Description | Example |
|------|--------------|----------|
| `--input` | Path to source directory | `--input ./src` |
| `--output` | Output directory | `--output ./out` |
| `--duplicate-all` | Duplicate all parameters instead of only the first | `--duplicate-all` |
| `--suffix` | Suffix to append to duplicated parameters | `--suffix _copy` |
| `--comment` | Add comment after modified signatures | `--comment` |
| `--filter` | File name pattern filter | `--filter *Service.cs` |
| `--summary` | Show summary of modified methods and parameters | `--summary` |

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
dotnet run -- \
  --input ./src \
  --output ./out \
  --duplicate-all \
  --suffix _dup \
  --comment
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

## 📊 Example summary output

```
[ParamsDuplicator] Files processed: 12
[ParamsDuplicator] Methods modified: 45
[ParamsDuplicator] Parameters duplicated: 97
[ParamsDuplicator] Saved to: ./out
```

---

## 🧾 License

MIT License © 2025 Your Name
