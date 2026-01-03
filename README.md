# Nightmare

Nightmare is a .NET 10.0 terminal UI for composing and executing HTTP requests from a single templated configuration file. A lightweight expression language lets you reuse variables, call helper functions, and chain requests/responses without writing code.
An ideal client with a versionable config and a simple workflow.

## Quick Start

### Installation

- Install the latest release from [Releases](https://github.com/LinkNexus/Nightmare/releases).
- or clone the repo and build from source.
    - For that, you'll need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10).
    - Run `dotnet publish -c Release` in the Nightmare dir of the repo.
    - The executable is in `Nightmare/bin/Release/net10.0/publish/`.

### Usage 

#### Create a new config

- Run `nightmare new` to create a new config file in the current directory.
- The following options are available:
  - `--file <path>` or `-f <path>`: specify a custom file name (defaults to `nightmare.json` in the current directory).

#### Launch Nightmare

- Run `nightmare` to launch the app.
- The following options are available:
  - `--config <path>` or `-c <path>`: specify a custom config file (defaults to `nightmare.json` in the current directory).
  - `--depth <n>` or `-d <n>`: search upward for a config file up to `n` directories (defaults to 5).
- Outputs a TUI with the selected profile (defaults to the first profile with default prop as true), requests tree, a request section that shows the details of the selected request and a response section that shows the details of the response of this request.
- Press `P` (when the focus is not in the request tree) to open the profile dialog (where you can switch profiles)
- There is hot reload for changes to the config file.

#### Simple Example

```json
{
  "name": "My API Requests",
  "profiles": {
    "dev": {
      "default": true,
      "data": {
        "base_url": "https://httpbin.org"
      }
    }
  },
  "requests": {
    "hello": {
      "url": "hello.com"
    }
  }
}
```

#### Functioning

- The request tree will be formed from the entries under `requests`. 
- Each entry can be a request definition or a nested group of requests.
- In order to make an entry a group of requests, add a `requests` property to the entry as following:
  ```json
    {
      "requests": {
        "request1": {
          "url": "hello.com"
        }
        "request2": {
          "requests": {
            "subrequest1": {
              "url": "hello.com"
            }
          }
        }
      }
    }
  ````
  
#### Request Definition

- The `url` property is required for all requests.
- The `method` property defaults to `GET`.
- Each request can have a `timeout` property (in milliseconds).
- The query object specifies the query params to be added to the URL. Note, in the TUI, the existing query params in the url are added with these from the query object. The values of the query object are serialized to strings in case of non-string values.
- The headers object specifies the headers to be sent with the request. As with query, the values are serialized to strings.
- The cookies object specifies the cookies to be sent with the request. Values are serialized to strings.
- The body property specifies the body of the request.
  - The value of the body can be a simple string, or a string with template expressions (explained later in these docs) that can evaluated to any value (non-string values will be serialized to strings). In this case, the type is considered `raw`. Can be used to send strings or files with the `{{ files() }}` function.
  - Or an object with following props
    - `type`: Can be of value `raw`, `text`, `json`, `form`, `multipart`. Defaults to `raw`.
    - `value`: The value of the body. The value depends on the type.

##### Requests Types

- `raw`: The body must be a string with or without template expressions that can evaluated to any value (non-string values will be serialized to strings).
- `text` and `json`: The body can be any value, from string with(out) template expressions to objects/arrays etc... They are serialized to strings. The content type is set accordingly.
- `form`: The body must either be an object or a string with template expressions that can evaluate to an object. The values are serialized to strings, but for arrays types, the values are appended to each key. Sent as form-urlencoded data.
- `multipart`: As in `form`, the value must evaluate to an object-like value. The values are serialized to strings, but for arrays types, the values are appended to each key. Sent as multipart/form-data. Files can be added by using the `file(...)` function.


#### Template Expressions

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

- Expressions are wrapped in double curly braces `{{ expr }}` inside strings.
- The expression is evaluated against the current context (profile data which serves as variables which can be used in expressions).
- The result of the expression is not necessarily evaluated to a string. For example: the {{ user }} in the example above evaluates to an object. This only works when the expression is the only thing in the json string. Things like `{{ user }} Hello` will call the ToString C# method on the object creating weird results like System.Dictionnary`2[System.String,System.Object] Hello`.
- The expression can be a simple identifier (e.g., `base_url`) or a member/index access expression (e.g., `user.username`) or a string litteral/number (e.g., `'hello'/1`), or a function call (e.g., `concat(...)`) or booleans (`true`/`false`) or `null`.
- Notice the single quotes around the string literal (Double quotes, even escaped ones, are not supported)
- Operations are permitted (like addition (+), subtraction (-), division (/), multiplication (*), modulo (%)), even exponential expression with `2e5` are supported.
- Even ternary expressions are supported (`condition ? trueVal : falseVal`).
- The expression can be wrapped in parentheses to force precedence.
- The expression can be a member access expression on a function call (e.g., `jsonDecode(jsonString).prop`). 
- The expression can be a function call with a variable number of arguments (e.g., `concat('hello', 'hi')`)
- You can `add` 2 string literals with the `+` operator. This will result in a string concatenation.
- Here is the list of the operators and their precedence: (precedence low → high): ternary `?:`; `||`; `&&`; `== !=`; `< <= > >=`; `+ -`; `* / %`; unary `! -`; postfix `. [] ()`.
- Template expressions can be embedded in strings like normal, for example: `{{ upper('h') + 'ello' }} World!`
- As shown, most functions can be used in the `profiles` section of the config.
- Each time the config file is modifiied and the app is reloaded, the selected profile data is reevaluated (This may be annoying when using the `prompt` or `filePrompt` functions).
- Property names cannot contain expressions. They will not be evaluated.

## Built-in Functions

All functions throw descriptive errors when argument types/counts are invalid. Signatures use `[]` for optional parameters and `...` for variadic args.

| Name           | Signature                                                                      | Description                                                                                                                                        |
| -------------- | ------------------------------------------------------------------------------ |----------------------------------------------------------------------------------------------------------------------------------------------------|
| `upper`        | upper(input: string)                                                           | Uppercase.                                                                                                                                         |
| `lower`        | lower(input: string)                                                           | Lowercase.                                                                                                                                         |
| `concat`       | concat(values: string...)                                                      | Concatenate all values.                                                                                                                            |
| `date`         | date(time?: string \| number = "now", format?: string = "yyyy-MM-dd HH:mm:ss") | Format unix seconds, `now`, or relative strings like `"5 days"`, `"-2 hours"`. The time identifier must always be in plural (not 1 day but 1 days) |
| `timestamp`    | timestamp()                                                                    | Current unix timestamp (seconds).                                                                                                                  |
| `env`          | env(name: string, defaultValue?: string \| null = null)                        | Environment variable fallback.                                                                                                                     |
| `hash`         | hash(input: string \| number)                                                  | SHA-256 hex string.                                                                                                                                |
| `ifElse`       | ifElse(condition: bool, ifVal: any, elseVal: any)                              | Ternary helper.                                                                                                                                    |
| `len`          | len(value: string \| array)                                                    | Length/count.                                                                                                                                      |
| `max`          | max(numbers: number...)                                                        | Maximum; at least one value required.                                                                                                              |
| `min`          | min(numbers: number...)                                                        | Minimum; at least one value required.                                                                                                              |
| `readFile`     | readFile(path: string)                                                         | File contents as text.                                                                                                                             |
| `file`         | file(path: string)                                                             | FileReference for multipart/raw bodies (adds path; name inferred on send).                                                                         |
| `filePrompt`   | filePrompt()                                                                   | Opens a file picker; returns selected path.                                                                                                        |
| `prompt`       | prompt(message?: string = "Enter the value: ")                                 | Text prompt dialog.                                                                                                                                |
| `uuid`         | uuid()                                                                         | New GUID string.                                                                                                                                   |
| `urlEncode`    | urlEncode(input: string)                                                       | URL-encode.                                                                                                                                        |
| `urlDecode`    | urlDecode(input: string)                                                       | URL-decode.                                                                                                                                        |
| `base64Encode` | base64Encode(input: string)                                                    | UTF-8 → Base64.                                                                                                                                    |
| `base64Decode` | base64Decode(input: string)                                                    | Base64 → UTF-8.                                                                                                                                    |
| `jsonEncode`   | jsonEncode(value: string \| number \| bool \| null \| array \| object)         | Pretty-print JSON.                                                                                                                                 |
| `jsonDecode`   | jsonDecode(jsonString: string, parseTemplates?: bool = false)                  | Parse JSON to objects/arrays; when `parseTemplates` is true, expressions inside strings are evaluated against the current context.                 |
| `req`          | req(requestId: string)                                                         | Fetch a request definition by id (`section.subsection.leaf`).                                                                                      |
| `res`          | res(requestId: string, trigger?: string = "10 minutes")                        | Return cached response or execute the request; stale cache re-executes.                                                                            |

## Tips

- Keep reusable values in `profiles.data`; they are exposed as identifiers everywhere expressions are allowed.
- Use `req(...)` to avoid duplicating request fragments and `res(...)` to chain workflows (e.g., login → reuse cookies/body in later calls).
- For file uploads, wrap paths with `file("/path/to/file")`; for interactive picking, call `filePrompt()` inside the expression.
