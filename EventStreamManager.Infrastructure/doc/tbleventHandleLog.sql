CREATE TABLE tblEventHandleLog (
                                   Id INT IDENTITY(1,1) PRIMARY KEY,
                                   EventHandleId INT NOT NULL,
                                   EventId INT NOT NULL,
                                   ProcessorId NVARCHAR(100) NOT NULL,
                                   ProcessorName NVARCHAR(200) NOT NULL,
                                   HandleTimes INT NOT NULL,
                                   NeedToSend BIT NOT NULL,
                                   RequestData NVARCHAR(MAX) NULL,
                                   ResponseData NVARCHAR(MAX) NULL,
                                   SendSuccess BIT NULL,
                                   ExceptionMessage NVARCHAR(MAX) NULL,
                                   Status NVARCHAR(20) NOT NULL,
                                   ExecutionTimeMs BIGINT NOT NULL,
                                   HandleDatetime DATETIME NOT NULL
);