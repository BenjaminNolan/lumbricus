CREATE TABLE `Server` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Host` VARCHAR(128) NOT NULL,
    `Port` INTEGER(11) NOT NULL DEFAULT 6667,
    `BotNick` VARCHAR(32) NOT NULL,
    `BotNickPassword` VARCHAR(64) NOT NULL,
    `BotUserName` VARCHAR(32) NOT NULL,
    `BotRealName` VARCHAR(128) NOT NULL DEFAULT 'Lumbricus IRC Bot (https://github.com/TwoWholeWorms/lumbricus)',
    `NickServNick` VARCHAR(32) NOT NULL DEFAULT 'NickServ',
    `NickServHost` VARCHAR(128) NOT NULL DEFAULT 'NickServ!NickServ@services.',
    `AutoConnect` TINYINT(1) NOT NULL DEFAULT 1,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastModifiedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
/* Why the funny collation? Unicode emoji. */

CREATE TABLE `Channel` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `ServerId` BIGINT(20) UNSIGNED NOT NULL,
    `Name` VARCHAR(32) NOT NULL,
    `AutoJoin` TINYINT(1) NOT NULL DEFAULT 1,
    `AllowCommandsInChannel` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastModifiedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Account` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `ServerId` BIGINT(20) UNSIGNED NOT NULL,
    `Name` VARCHAR(32) NOT NULL,
    `DisplayName` VARCHAR(32) NOT NULL,
    `UserName` VARCHAR(32) NULL,
    `Host` VARCHAR(128) NULL,
    `MostRecentNickId` BIGINT(20) UNSIGNED NULL,
    `FirstSeenAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastSeenAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ChannelLastSeenInId` BIGINT(20) UNSIGNED NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastModifiedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `IsOp` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Nick` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `ServerId` BIGINT(20) UNSIGNED NOT NULL,
    `Name` VARCHAR(32) NOT NULL,
    `DisplayName` VARCHAR(32) NOT NULL,
    `UserName` VARCHAR(32) NULL,
    `Host` VARCHAR(128) NULL,
    `AccountId` BIGINT(20) UNSIGNED NULL,
    `FirstSeenAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastSeenAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ChannelLastSeenInId` BIGINT(20) UNSIGNED NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastModifiedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Log` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `ServerId` BIGINT(20) UNSIGNED NOT NULL,
    `NickId` BIGINT(20) UNSIGNED NULL,
    `AccountId` BIGINT(20) UNSIGNED NULL,
    `ChannelId` BIGINT(20) UNSIGNED NULL,
    `IrcCommand` INTEGER(11) UNSIGNED NOT NULL,
    `Trail` VARCHAR(512) NULL,
    `Line` VARCHAR(512) NOT NULL,
    `LoggedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Ban` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Mask` VARCHAR(128) NOT NULL,
    `ServerId` BIGINT(20) UNSIGNED NULL,
    `NickId` BIGINT(20) UNSIGNED NULL,
    `AccountId` BIGINT(20) UNSIGNED NULL,
    `ChannelId` BIGINT(20) UNSIGNED NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsMugshotBan` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `LastModifiedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `BannerAccountId` BIGINT(20) UNSIGNED NULL,
    `BanMessage` VARCHAR(512) NULL,
    `UnbannedAt` TIMESTAMP NULL,
    `UnbannerAccountId` BIGINT(20) UNSIGNED NULL,
    `UnbanMessage` VARCHAR(512) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE `Setting` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `Section` VARCHAR(32) NOT NULL,
    `Name` VARCHAR(32) NOT NULL,
    `Value` VARCHAR(512) NOT NULL,
    `DefaultValue` VARCHAR(512) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

ALTER TABLE `Channel`
    ADD FOREIGN KEY `FK_channels_ServerId` (`ServerId`) REFERENCES `Server` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `Account`
    ADD FOREIGN KEY `FK_accounts_ServerId` (`ServerId`) REFERENCES `Server` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_accounts_MostRecentNickId` (`MostRecentNickId`) REFERENCES `Nick` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_accounts_ChannelLastSeenInId` (`ChannelLastSeenInId`) REFERENCES `Channel` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE `Nick`
    ADD FOREIGN KEY `FK_nicks_ServerId` (`ServerId`) REFERENCES `Server` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_nicks_AccountId` (`AccountId`) REFERENCES `Account` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_nicks_ChannelLastSeenInId` (`ChannelLastSeenInId`) REFERENCES `Channel` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE `Log`
    ADD FOREIGN KEY `FK_logs_ServerId` (`ServerId`) REFERENCES `Server` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_ChannelId` (`ChannelId`) REFERENCES `Channel` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_AccountId` (`AccountId`) REFERENCES `Account` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_NickId` (`NickId`) REFERENCES `Nick` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE `Ban`
    ADD FOREIGN KEY `FK_logs_ServerId` (`ServerId`) REFERENCES `Server` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_ChannelId` (`ChannelId`) REFERENCES `Channel` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_AccountId` (`AccountId`) REFERENCES `Account` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_NickId` (`NickId`) REFERENCES `Nick` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_BannerAccountId` (`BannerAccountId`) REFERENCES `Account` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_UnbannerAccountId` (`UnbannerAccountId`) REFERENCES `Account` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

INSERT INTO `Setting` (`Section`, `Name`, `Value`, `DefaultValue`) VALUE ('help', 'uri', 'http://twowholeworms.com/lumbricus/help', 'http://twowholeworms.com/lumbricus/help');
