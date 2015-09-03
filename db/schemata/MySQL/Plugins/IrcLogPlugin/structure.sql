CREATE TABLE `Log` (
    `Id` BIGINT(20) UNSIGNED NOT NULL AUTO_INCREMENT,
    `ServerId` BIGINT(20) UNSIGNED NOT NULL,
    `NickId` BIGINT(20) UNSIGNED NULL,
    `AccountId` BIGINT(20) UNSIGNED NULL,
    `ChannelId` BIGINT(20) UNSIGNED NULL,
    `IrcCommand` INTEGER(11) UNSIGNED NOT NULL,
    `Trail` VARCHAR(1024) NULL,
    `Line` VARCHAR(1024) NULL,
    `LoggedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

ALTER TABLE `Log`
    ADD FOREIGN KEY `FK_logs_ServerId` (`ServerId`) REFERENCES `Server` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_ChannelId` (`ChannelId`) REFERENCES `Channel` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_AccountId` (`AccountId`) REFERENCES `Account` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
    ADD FOREIGN KEY `FK_logs_NickId` (`NickId`) REFERENCES `Nick` (`Id`) ON DELETE SET NULL ON UPDATE CASCADE;