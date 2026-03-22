# PR Review Assistant

# Architecture:

```
PullRequests_Review_Assistant.sln
│
├── src/
│   ├── PullRequests_Review_Assistant.Application/
│   │   ├── Builders/
│   │   │   ├── IReviewConfigurationBuilder.cs
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
│       │   ├── AuthStrategyFactory.cs
│       │   ├── BitbucketAuthStrategy.cs
│       │   ├── GitHubAuthStrategy.cs
│       │   └── GitLabAuthStrategy.cs
│       ├── Configuration/
│       │   └── ModelConfigProvider.cs
│       ├── Extensions/
│       │   └── StringExtensions.cs
│       ├── Platform/
│       │   ├── BitbucketPlatformService.cs
│       │   ├── GitHubPlatformService.cs
│       │   └── GitLabPlatformService.cs
│       └── Secrets/
│           └── AzureKeyVaultSecretsProvider.cs
```