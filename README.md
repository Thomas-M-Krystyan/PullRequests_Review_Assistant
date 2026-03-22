# PR Review Assistant

---

## 2. How to Start

### GitHub

#### Prerequisites

Before running the application against a GitHub repository, ensure the following are in place:

- **Node.js & npm** — required to run the MCP GitHub server via `npx`

  <details>

  - Install via [https://nodejs.org](https://nodejs.org) *(LTS version recommended)*
  - or from the terminal:

    **Windows** (via [Winget](https://learn.microsoft.com/windows/package-manager/winget/)):

    ```sh
    winget install OpenJS.NodeJS.LTS
    ```

    **Windows** (via [Chocolatey](https://chocolatey.org/)):

    ```sh
    choco install nodejs-lts
    ```

    **macOS** (via [Homebrew](https://brew.sh/)):

    ```sh
    brew install node
    ```

    **Linux** (Debian/Ubuntu):

    ```sh
    sudo apt install nodejs npm
    ```

  - After installation, verify with:

    ```sh
    node --version
    npx --version
    ```

  > **NOTE:** `npx` is bundled with `npm` and does not need to be installed separately.
  
  > **Windows Troubleshooting:** If running `npx` in PowerShell produces an *"is not digitally signed"*
    error, run the following command once in PowerShell **as Administrator**, then retry:

  ```powershell
  Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
  ```
  </details>

- **GitHub Copilot subscription** — the application uses GitHub Copilot SDK agents

---

#### Create Personal Access Token

The application authenticates with GitHub using a **classic Personal Access Token (PAT)**.

1. Go to **GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)**
2. Click **Generate new token (classic)**
3. Fill in the form:
   - **Note:** `PR Review Assistant - Local` *(or any descriptive name)*
   - **Expiration:** choose an appropriate duration (e.g. `30 days`)
   - **Scopes:** tick only **`repo`** — this grants full access to private repositories, which is required to read PR files and post review comments
4. Click **Generate token**
5. **Copy the token immediately** — GitHub will not show it again

---

#### Store the Token (User Secrets)

The token is stored locally using .NET User Secrets, which keeps it outside the repository and prevents accidental commits.

In Visual Studio, right-click the `PullRequests_Review_Assistant.Console` project and select **Manage User Secrets**.

This opens the file at:

> `%APPDATA%\Microsoft\UserSecrets\pr-review-assistant-local\secrets.json`

Add your token:

```json
{
  "github-pat": "ghp_YourTokenHere"
}
```

---

## 3. Run the Application

Launch the application from Visual Studio (`Ctrl+F5`) or from the terminal:

```sh
dotnet run --project src/PullRequests_Review_Assistant.Console
```

When prompted:

| Prompt | Value |
|--------|-------|
| **Tier** | Your Copilot plan (e.g. `Free`, `Pro`, `Business`) |
| **Platform** | `GitHub` |

#### Tier prompt example

```sh
[Config] No tier specified. Falling back to interactive selection.
[Config] Select a Tier:
  [1] Free
  [2] Student
  [3] Pro
  [4] ProPlus
  [5] Business
  [6] Enterprise
Enter a number (1-6):
```

#### Platform prompt example

```sh
[Model] Using primary model: claude-opus-4.6
[Config] No platform specified. Falling back to interactive selection.
[Config] Select a Platform:
  [1] GitHub
  [2] GitLab
  [3] Bitbucket
Enter a number (1-3):
```

> **NOTE:** Both prompts can be skipped by passing arguments directly:

```sh
dotnet run --project src/PullRequests_Review_Assistant.Console -- --tier=Free --platform=GitHub
```

---

#### Review a Pull Request

Once at the `>` prompt, use the `review` command:

```sh
review <Platform> <Your-GitHub-Name> <Repository-Name> <Pull-Request-Id> [options]
```

**Minimal example** — core areas only (Performance, Architecture, Vulnerabilities, Code Smells):

```sh
review github <Your-GitHub-Name> <Repository-Name> <Pull-Request-Id> --lang=C# --areas=CoreReview
```

> **NOTE:** `CoreReview` is a shorthand for the main review areas: Performance, Architecture, Vulnerabilities, and Code Smells.

**Targeted example** — specific areas only:

```sh
review github <Your-GitHub-Name> <Repository-Name> <Pull-Request-Id> --lang=C# --docs --naming --errors
```

**Full example** — all areas and suggestions:

```sh
review github <Your-GitHub-Name> <Repository-Name> <Pull-Request-Id> --lang=C# --all
```

Review comments are automatically **posted back to the GitHub pull request** upon completion.

All review areas will be displayed in the console if you type:

`--help` or `-h` or `--h`

---

#### Exit the Application

- Type `exit` or `quit` at the prompt, **or**
- Press `Ctrl+C` for graceful cancellation

---

## 4. Architecture

```
PullRequests_Review_Assistant.sln
│
├── src/
│   ├── PullRequests_Review_Assistant.Application/
│   │   ├── Builders/
│   │   │   ├── Interface/
│   │   │   │   └── IReviewConfigurationBuilder.cs
│   │   │   └── ReviewConfigurationBuilder.cs
│   │   ├── Commands/
│   │   │   └── ConsoleCommandHandler.cs
│   │   ├── Services/
│   │   │   └── CodeReviewOrchestrator.cs
│   │   └── Utilities/
│   │       └── ConsolePrompt.cs
│   │
│   ├── PullRequests_Review_Assistant.Console/
│   │   └── Program.cs
│   │
│   ├── PullRequests_Review_Assistant.Domain/
│   │   ├── Entities/
│   │   │   ├── PullRequestFile.cs
│   │   │   └── ReviewComment.cs
│   │   ├── Enums/
│   │   │   ├── PlatformType.cs
│   │   │   ├── ReviewArea.cs
│   │   │   └── SubscriptionTier.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuthStrategy.cs
│   │   │   ├── ICodeReviewAgent.cs
│   │   │   ├── ILanguageAgent.cs
│   │   │   ├── IModelConfigProvider.cs
│   │   │   ├── IRepositoryPlatformService.cs
│   │   │   └── ISecretsProvider.cs
│   │   ├── Templates/
│   │   │   └── SystemPromptTemplates.cs
│   │   └── ValueObjects/
│   │       ├── ModelPreference.cs
│   │       └── ReviewConfiguration.cs
│   │
│   └── PullRequests_Review_Assistant.Infrastructure/
│       ├── Agents/
│       │   ├── CopilotCodeReviewAgent.cs
│       │   └── CopilotLanguageAgent.cs
│       ├── Auth/
│       │   └── Factory/
│       │   │   └── AuthStrategyFactory.cs
│       │   ├── BitbucketAuthStrategy.cs
│       │   ├── GitHubAuthStrategy.cs
│       │   └── GitLabAuthStrategy.cs
│       ├── Configuration/
│       │   └── ModelConfigProvider.cs
│       ├── Extensions/
│       │   └── StringExtensions.cs
│       ├── Platform/
│       │   ├── Parent/
│       │   │   └── McpPlatformServiceBase.cs 
│       │   ├── BitbucketPlatformService.cs
│       │   ├── GitHubPlatformService.cs
│       │   └── GitLabPlatformService.cs
│       └── Secrets/
│           └── AzureKeyVaultSecretsProvider.cs
```