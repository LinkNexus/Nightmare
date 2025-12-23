# Nightmare - Quick Reference

## 🚀 Quick Start

```bash
# Clone/navigate to project
cd Nightmare

# Build
dotnet build

# Run with example config
dotnet run --project Nightmare example-config.json

# Run expression demo
dotnet run --project Nightmare --demo

# Run tests
dotnet test

# AOT compile
dotnet publish -c Release -r linux-x64

# Run verification
./verify.sh
```

## 📝 Configuration Format

```json
{
  "profiles": {
    "profile_name": {
      "default": true,
      "data": {
        "variable": "value",
        "nested": { "key": "value" }
      }
    }
  },
  "requests": {
    "group_name": {
      "requests": {
        "request_name": {
          "url": "{{ base_url }}/path",
          "method": "POST",
          "headers": { "Header": "{{ value }}" },
          "cookies": { "name": "{{ value }}" },
          "ContentType": "application/json",
          "body": { "field": "{{ value }}" }
        }
      }
    }
  }
}
```

## 💬 Expression Syntax

### Variables
```
{{ variable }}
{{ object.property }}
{{ array[0] }}
{{ data['key'] }}
```

### Operators
```
{{ a + b }}        # Addition/concatenation
{{ a - b }}        # Subtraction
{{ a * b }}        # Multiplication
{{ a / b }}        # Division
{{ a % b }}        # Modulo
{{ a == b }}       # Equality
{{ a != b }}       # Inequality
{{ a < b }}        # Less than
{{ a <= b }}       # Less or equal
{{ a > b }}        # Greater than
{{ a >= b }}       # Greater or equal
{{ a && b }}       # Logical AND
{{ a || b }}       # Logical OR
{{ !a }}           # Logical NOT
```

### String Functions
```
{{ upper(text) }}                    # UPPERCASE
{{ lower(text) }}                    # lowercase
{{ trim(text) }}                     # Remove whitespace
{{ concat(a, b, c) }}                # Concatenate
{{ substring(text, start, length) }} # Extract substring
{{ replace(text, old, new) }}        # Replace text
{{ split(text, separator) }}         # Split to array
{{ join(separator, array) }}         # Join array
{{ indexOf(text, search) }}          # Find position
{{ contains(text, search) }}         # Check contains
{{ startsWith(text, prefix) }}       # Check prefix
{{ endsWith(text, suffix) }}         # Check suffix
{{ length(text) }}                   # Get length
```

### HTTP Functions
```
{{ prompt('Enter value: ') }}              # User input
{{ prompt('Enter:', 'default') }}          # With default
{{ filePrompt() }}                         # File picker
{{ filePrompt('Select file:') }}           # With message
{{ readFile(path) }}                       # Read file
{{ env('VARIABLE') }}                      # Environment var
{{ env('VAR', 'default') }}                # With default
{{ uuid() }}                               # Generate UUID
{{ timestamp() }}                          # Unix timestamp
{{ timestamp('yyyy-MM-dd') }}              # Formatted date
```

### Encoding Functions
```
{{ base64Encode(text) }}     # Encode to base64
{{ base64Decode(base64) }}   # Decode from base64
{{ urlEncode(text) }}        # URL encode
{{ urlDecode(encoded) }}     # URL decode
{{ jsonParse(json) }}        # Parse JSON
{{ jsonStringify(obj) }}     # Stringify to JSON
```

### Complex Expressions
```
{{ firstName + ' ' + lastName }}
{{ price * quantity * 1.1 }}
{{ age >= 18 && hasLicense }}
{{ upper(trim(user.name)) }}
{{ 'Bearer ' + env('API_TOKEN') }}
{{ readFile(filePrompt()) }}
```

## 🔧 Common Patterns

### Authorization
```json
{
  "headers": {
    "Authorization": "Bearer {{ env('API_TOKEN') }}"
  }
}
```

### Dynamic URLs
```json
{
  "url": "{{ base_url }}/api/{{ version }}/users/{{ userId }}"
}
```

### Request Bodies
```json
{
  "body": {
    "username": "{{ prompt('Username: ') }}",
    "password": "{{ prompt('Password: ') }}",
    "timestamp": "{{ timestamp() }}"
  }
}
```

### File Upload
```json
{
  "ContentType": "MultipartFormData",
  "body": {
    "file": {
      "file": true,
      "path": "{{ filePrompt() }}"
    }
  }
}
```

### Conditional Values
```json
{
  "premium": "{{ age >= 18 && hasSubscription }}",
  "discount": "{{ total > 100 ? 0.1 : 0 }}"
}
```

## 🐛 Error Messages

When an error occurs, you get precise location:

```
❌ JSON Parse Error at line 7, column 9:
   Expected ',' or '}' in object
```

## 📊 Project Structure

```
Nightmare/
├── Nightmare.Parser/      # Core library
├── Nightmare/             # Console app
├── Nightmare.Tests/       # Tests (143)
├── example-config.json    # Example
├── verify.sh             # Verification
└── *.md                  # Documentation
```

## 📚 Documentation Files

- **README.md** - Getting started
- **TECHNICAL.md** - Deep dive
- **EXTENDING.md** - Extension guide
- **SUMMARY.md** - Project summary
- **STATUS.md** - Project status
- **QUICK_REFERENCE.md** - This file

## ✅ Verification Checklist

```bash
./verify.sh
```

Should show:
- ✅ Build successful
- ✅ All 143 tests passed
- ✅ Configuration loads correctly
- ✅ Expression evaluation working
- ✅ Error reporting accurate
- ✅ AOT compilation successful
- ✅ AOT binary executes correctly

## 🎯 Key Metrics

- **Binary Size**: 1.9 MB
- **Tests**: 143 passing
- **Code**: 3,634 lines
- **Startup**: <100ms
- **Build**: ~3 seconds

## 💡 Tips

1. **Variables**: Define in profile `data` section
2. **Expressions**: Keep them simple and readable
3. **Errors**: Check line/column in error messages
4. **Functions**: See EXTENDING.md for custom functions
5. **AOT**: All features work in native binary

## 🔗 Useful Commands

```bash
# Help
dotnet run --project Nightmare

# Load config
dotnet run --project Nightmare config.json

# Demo
dotnet run --project Nightmare --demo

# Test
dotnet test

# AOT build
dotnet publish -c Release -r linux-x64

# Clean
dotnet clean

# Verify all
./verify.sh
```

---

**Need Help?** See full documentation in README.md, TECHNICAL.md, and EXTENDING.md
