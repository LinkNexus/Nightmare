# Nightmare

Nightmare is a .NET 10.0 terminal UI for composing and executing HTTP requests from a single templated configuration file. A lightweight expression language lets you reuse variables, call helper functions, and chain requests/responses without writing code.

## Quick Start

- **Prereqs**: .NET 10.0 SDK.
- **Create a template**: `dotnet run --project Nightmare -- new --file nightmare.json`.
- **Run the TUI**: `dotnet run --project Nightmare -- --config nightmare.json --depth 5` (the app searches upward for the file up to `depth`). Config changes are hot-reloaded.
- **Switch profiles**: press `P` in the TUI to pick a profile. Activate a request (Enter/double-click in the tree) to send it and view the request/response panels.

## Config File Layout

The config is JSON with template-aware strings (`"...{{ expr }}..."`). Property names cannot contain expressions. Root shape:

```json
{
	"name": "Collection title (template-enabled)",
	"profiles": { "<profileName>": { "default": true?, "data": { /* variables */ } } },
	"requests": { "<groupOrRequest>": { /* nested group or request */ } }
}
```

### Profiles

- `data` holds variables made available as identifiers inside expressions (e.g., `base_url`, `user.password`).
- If no profile is specified at launch, the one with `"default": true` is chosen; otherwise, the selected name is used.

### Requests

Requests are nested objects; a leaf request is a JSON object without an inner `requests` property. Supported fields:

- `url` (required): template string.
- `method`: template string, defaults to `GET`.
- `query`: object of key → value (template-evaluated to strings); merged with any query already present in `url`.
- `headers`: object where each value can be a string, array, object, or null. Arrays and objects are serialized; null entries are skipped.
- `cookies`: key → value (template-evaluated to string).
- `timeout`: number in milliseconds.
- `body`: see below.

### Body Shapes

- **Raw**: either a template string, or `{ "type": "raw", "value": "..." }`. If the expression evaluates to a `file(...)` reference, the file stream is sent; otherwise plain text.
- **Text/JSON**: `{ "type": "text" | "json", "value": any }` → serialized string with appropriate content type.
- **Form**: `{ "type": "form", "value": { key: value } | "{{ expr }}" }` where the expression must yield an object. Arrays of strings emit repeated keys.
- **Multipart**: `{ "type": "multipart", "value": { key: value } | "{{ expr }}" }`; values may be strings, arrays, or `FileReference` objects from `file(...)` (file name inferred when missing).

### Request/Response chaining

- `req(requestId)` returns a request definition by dot-separated path (e.g., `req('auth.login')`). Useful for reusing pieces of other requests.
- `res(requestId, trigger?)` executes or fetches a cached response. Cache freshness is controlled by `trigger` (default `"10 minutes"`). Returns an object with `statusCode`, `reasonPhrase`, `timestamp`, `headers`, `cookies`, `body`.

### Example (excerpt)

```json
{
  "name": "{{ upper('Nightmare') }} Requests Collection",
  "profiles": {
    "dev": {
      "default": true,
      "data": {
        "base_url": "https://httpbin.org",
        "user": {
          "username": "admin",
          "password": "{{ prompt('Enter a password for the dev profile:') }}"
        }
      }
    }
  },
  "requests": {
    "auth": {
      "requests": {
        "login": {
          "url": "{{ base_url }}/post",
          "method": "POST",
          "body": { "type": "json", "value": "{{ user }}" }
        }
      }
    }
  }
}
```

## Template Strings & Expression Language

- Delimiters: `{{ expression }}` inside JSON strings. If the string is only one expression, its evaluated value (which may be non-string) is used; otherwise, all parts are concatenated to a string.
- Literals: numbers, strings, booleans, `null`.
- Identifiers: variable names from the active profile.
- Member/index access: `obj.prop`, `arr[0]`.
- Calls: `func(arg1, arg2)`.
- Operators (precedence low → high): ternary `?:`; `||`; `&&`; `== !=`; `< <= > >=`; `+ -`; `* / %`; unary `! -`; postfix `. [] ()`.

## Built-in Functions

All functions throw descriptive errors when argument types/counts are invalid. Signatures use `[]` for optional parameters and `...` for variadic args.

| Name           | Signature                                                                      | Description                                                                                                                        |
| -------------- | ------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------- |
| `upper`        | upper(input: string)                                                           | Uppercase.                                                                                                                         |
| `lower`        | lower(input: string)                                                           | Lowercase.                                                                                                                         |
| `concat`       | concat(values: string...)                                                      | Concatenate all values.                                                                                                            |
| `date`         | date(time?: string \| number = "now", format?: string = "yyyy-MM-dd HH:mm:ss") | Format unix seconds, `now`, or relative strings like `"5 days"`, `"-2 hours"`.                                                     |
| `timestamp`    | timestamp()                                                                    | Current unix timestamp (seconds).                                                                                                  |
| `env`          | env(name: string, defaultValue?: string \| null = null)                        | Environment variable fallback.                                                                                                     |
| `hash`         | hash(input: string \| number)                                                  | SHA-256 hex string.                                                                                                                |
| `ifElse`       | ifElse(condition: bool, ifVal: any, elseVal: any)                              | Ternary helper.                                                                                                                    |
| `len`          | len(value: string \| array)                                                    | Length/count.                                                                                                                      |
| `max`          | max(numbers: number...)                                                        | Maximum; at least one value required.                                                                                              |
| `min`          | min(numbers: number...)                                                        | Minimum; at least one value required.                                                                                              |
| `readFile`     | readFile(path: string)                                                         | File contents as text.                                                                                                             |
| `file`         | file(path: string)                                                             | FileReference for multipart/raw bodies (adds path; name inferred on send).                                                         |
| `filePrompt`   | filePrompt()                                                                   | Opens a file picker; returns selected path.                                                                                        |
| `prompt`       | prompt(message?: string = "Enter the value: ")                                 | Text prompt dialog.                                                                                                                |
| `uuid`         | uuid()                                                                         | New GUID string.                                                                                                                   |
| `urlEncode`    | urlEncode(input: string)                                                       | URL-encode.                                                                                                                        |
| `urlDecode`    | urlDecode(input: string)                                                       | URL-decode.                                                                                                                        |
| `base64Encode` | base64Encode(input: string)                                                    | UTF-8 → Base64.                                                                                                                    |
| `base64Decode` | base64Decode(input: string)                                                    | Base64 → UTF-8.                                                                                                                    |
| `jsonEncode`   | jsonEncode(value: string \| number \| bool \| null \| array \| object)         | Pretty-print JSON.                                                                                                                 |
| `jsonDecode`   | jsonDecode(jsonString: string, parseTemplates?: bool = false)                  | Parse JSON to objects/arrays; when `parseTemplates` is true, expressions inside strings are evaluated against the current context. |
| `req`          | req(requestId: string)                                                         | Fetch a request definition by id (`section.subsection.leaf`).                                                                      |
| `res`          | res(requestId: string, trigger?: string = "10 minutes")                        | Return cached response or execute the request; stale cache re-executes.                                                            |

## TUI Workflow

- **Layout**: left tree (`Recipes`) with search, right side shows the selected Request (method/url, query/headers/cookies/body) and Response (status/time, headers/cookies/body).
- **Send a request**: activate a leaf node in Recipes; the app builds the request with the active profile, sends it, and fills Request/Response panels.
- **Switch profile**: press `P` to open the profile dialog; selection rebinds variables and future requests.
- **Live reload**: edits to the config file trigger automatic reload; parsing/evaluation errors appear in the top error bar with line/column info.

## Tips

- Keep reusable values in `profiles.data`; they are exposed as identifiers everywhere expressions are allowed.
- Use `req(...)` to avoid duplicating request fragments and `res(...)` to chain workflows (e.g., login → reuse cookies/body in later calls).
- For file uploads, wrap paths with `file("/path/to/file")`; for interactive picking, call `filePrompt()` inside the expression.
