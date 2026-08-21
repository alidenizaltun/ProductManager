BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    ALTER TABLE [Product].[ProductPricingRules] ADD [SourceTemplateId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    ALTER TABLE [Product].[ProductPricingRules] ADD [SourceTemplateVersion] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE TABLE [Product].[PriceRevisions] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(64) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [AdjustmentType] int NOT NULL,
        [Value] decimal(18,4) NOT NULL,
        [RoundingMode] int NOT NULL,
        [RoundingStep] decimal(18,4) NULL,
        [CurrencyCode] nvarchar(3) NULL,
        [Status] int NOT NULL,
        [EffectiveDate] datetime2 NULL,
        [SubmittedAt] datetime2 NULL,
        [SubmittedByUserId] uniqueidentifier NULL,
        [ApprovedAt] datetime2 NULL,
        [ApprovedByUserId] uniqueidentifier NULL,
        [ApprovalNote] nvarchar(1000) NULL,
        [AppliedAt] datetime2 NULL,
        [AppliedByUserId] uniqueidentifier NULL,
        [RolledBackAt] datetime2 NULL,
        [RolledBackByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PriceRevisions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE TABLE [Product].[PricingTemplates] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(64) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [TemplateKind] int NOT NULL,
        [UnitDefinitionId] uniqueidentifier NULL,
        [CurrencyCode] nvarchar(3) NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [Version] int NOT NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PricingTemplates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PricingTemplates_UnitDefinitions_UnitDefinitionId] FOREIGN KEY ([UnitDefinitionId]) REFERENCES [Product].[UnitDefinitions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE TABLE [Product].[PriceRevisionLines] (
        [Id] uniqueidentifier NOT NULL,
        [PriceRevisionId] uniqueidentifier NOT NULL,
        [TargetType] int NOT NULL,
        [TargetId] uniqueidentifier NOT NULL,
        [TargetPath] nvarchar(128) NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [ProductName] nvarchar(250) NOT NULL,
        [TargetLabel] nvarchar(256) NOT NULL,
        [CurrencyCode] nvarchar(3) NOT NULL,
        [OldValue] decimal(18,4) NOT NULL,
        [NewValue] decimal(18,4) NOT NULL,
        [IsExcluded] bit NOT NULL,
        [IsApplied] bit NOT NULL,
        [SkipReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PriceRevisionLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceRevisionLines_PriceRevisions_PriceRevisionId] FOREIGN KEY ([PriceRevisionId]) REFERENCES [Product].[PriceRevisions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE TABLE [Product].[PriceRevisionScopes] (
        [Id] uniqueidentifier NOT NULL,
        [PriceRevisionId] uniqueidentifier NOT NULL,
        [ScopeType] int NOT NULL,
        [TargetId] uniqueidentifier NULL,
        [TargetValue] nvarchar(64) NULL,
        [IsExclude] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PriceRevisionScopes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceRevisionScopes_PriceRevisions_PriceRevisionId] FOREIGN KEY ([PriceRevisionId]) REFERENCES [Product].[PriceRevisions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE INDEX [IX_ProductPricingRules_SourceTemplateId] ON [Product].[ProductPricingRules] ([SourceTemplateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE INDEX [IX_PriceRevisionLines_Revision_Product] ON [Product].[PriceRevisionLines] ([PriceRevisionId], [ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PriceRevisionLines_Revision_Target] ON [Product].[PriceRevisionLines] ([PriceRevisionId], [TargetType], [TargetId], [TargetPath]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PriceRevisions_Code] ON [Product].[PriceRevisions] ([Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE INDEX [IX_PriceRevisions_Status] ON [Product].[PriceRevisions] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE INDEX [IX_PriceRevisionScopes_Revision_Type] ON [Product].[PriceRevisionScopes] ([PriceRevisionId], [ScopeType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PricingTemplates_Code] ON [Product].[PricingTemplates] ([Code]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE INDEX [IX_PricingTemplates_Kind_Active] ON [Product].[PricingTemplates] ([TemplateKind], [IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    CREATE INDEX [IX_PricingTemplates_UnitDefinitionId] ON [Product].[PricingTemplates] ([UnitDefinitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    ALTER TABLE [Product].[ProductPricingRules] ADD CONSTRAINT [FK_ProductPricingRules_PricingTemplates_SourceTemplateId] FOREIGN KEY ([SourceTemplateId]) REFERENCES [Product].[PricingTemplates] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821082307_AddPricingTemplatesAndPriceRevisions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821082307_AddPricingTemplatesAndPriceRevisions', N'10.0.5');
END;

COMMIT;
GO

