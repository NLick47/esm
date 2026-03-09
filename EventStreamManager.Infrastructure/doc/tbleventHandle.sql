CREATE TABLE tblEventHandle (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                EventId INT NOT NULL,
                                ProcessorId NVARCHAR(100) NOT NULL,
                                ProcessorName NVARCHAR(200) NOT NULL,
                                HandleTimes INT NOT NULL,
                                IsFinished BIT NOT NULL,
                                LastHandleStatus NVARCHAR(20) NULL,
                                LastHandleDatetime DATETIME NULL,
                                LastHandleLogId INT NULL,
                                CreateDatetime DATETIME NOT NULL
);
