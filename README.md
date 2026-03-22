# PR Review Assistant

# Architecture:

```
PullRequests_Review_Assistant.sln
│
├── src/
│   ├── PullRequests_Review_Assistant.Domain/
│   │   ├── Enums/
│   │   │   ├── ReviewArea.cs
│   │   │   ├── PlatformType.cs
│   │   │   └── SubscriptionTier.cs
│   │   ├── ValueObjects/
│   │   │   ├── ReviewConfiguration.cs
│   │   │   └── ModelPreference.cs
│   │   ├── Entities/
│   │   │   ├── PullRequestFile.cs
│   │   │   └── ReviewComment.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuthStrategy.cs
│   │   │   ├── ISecretsProvider.cs
│   │   │   ├── IRepositoryPlatformService.cs
│   │   │   ├── ICodeReviewAgent.cs
│   │   │   ├── ILanguageAgent.cs
│   │   │   └── IModelConfigProvider.cs
│   │   └── Templates/
│   │       └── SystemPromptTemplates.cs
│   │
│   ├── PullRequests_Review_Assistant.Application/
│   │   ├── Builders/
│   │   │   ├── IReviewConfigurationBuilder.cs
│   │   │   └── ReviewConfigurationBuilder.cs
│   │   ├── Services/
│   │   │   └── CodeReviewOrchestrator.cs
│   │   └── Commands/
│   │       └── ConsoleCommandHandler.cs
│   │
│   ├── PullRequests_Review_Assistant.Infrastructure/
│   │   ├── Auth/
│   │   │   ├── GitHubAuthStrategy.cs
│   │   │   ├── GitLabAuthStrategy.cs
│   │   │   ├── BitbucketAuthStrategy.cs
│   │   │   └── AuthStrategyFactory.cs
│   │   ├── Secrets/
│   │   │   └── AzureKeyVaultSecretsProvider.cs
│   │   ├── Agents/
│   │   │   ├── CopilotCodeReviewAgent.cs
│   │   │   └── CopilotLanguageAgent.cs
│   │   ├── Platform/
│   │   │   ├── GitHubPlatformService.cs
│   │   │   ├── GitLabPlatformService.cs
│   │   │   └── BitbucketPlatformService.cs
│   │   └── Configuration/
│   │       └── ModelConfigProvider.cs
│   │
│   └── PullRequests_Review_Assistant.Console/
│       ├── Utilities/
│       │   └── ConsolePrompt.cs
│       └── Program.cs
```