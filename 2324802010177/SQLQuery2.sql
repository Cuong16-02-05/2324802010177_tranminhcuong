USE [ASC];

-- Xóa bảng cũ nếu cấu trúc sai, tạo lại đúng
IF EXISTS (SELECT * FROM sysobjects WHERE name='ChatMessages' AND xtype='U')
    DROP TABLE [dbo].[ChatMessages];

CREATE TABLE [dbo].[ChatMessages] (
    [UniqueId]          NVARCHAR(450)  NOT NULL,
    [ServiceRequestId]  NVARCHAR(450)  NULL,
    [FromEmail]         NVARCHAR(MAX)  NULL,
    [FromDisplayName]   NVARCHAR(MAX)  NULL,
    [ToEmail]           NVARCHAR(MAX)  NULL,
    [Message]           NVARCHAR(MAX)  NOT NULL DEFAULT '',
    [SentDate]          DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    [IsRead]            BIT            NOT NULL DEFAULT 0,
    [SenderRole]        NVARCHAR(MAX)  NULL,
    [CreatedBy]         NVARCHAR(MAX)  NULL,
    [CreatedDate]       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy]         NVARCHAR(MAX)  NULL,
    [UpdatedDate]       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted]         BIT            NOT NULL DEFAULT 0,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([UniqueId])
);

CREATE INDEX [IX_ChatMessages_ServiceRequestId]
    ON [dbo].[ChatMessages] ([ServiceRequestId]);

PRINT '✓ Tạo lại bảng ChatMessages thành công';

-- Ghi migration history
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20260509000001_AddPriceAndChat'
)
INSERT INTO [__EFMigrationsHistory] ([MigrationId],[ProductVersion])
VALUES ('20260509000001_AddPriceAndChat','8.0.0');