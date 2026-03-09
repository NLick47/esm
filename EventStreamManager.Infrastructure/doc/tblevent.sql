CREATE TABLE tblEvent (
                          Id INT IDENTITY(1,1) PRIMARY KEY,
                          IntHospitalID BIGINT NOT NULL,
                          EventType NVARCHAR(50) NOT NULL,
                          StrEventReferenceId NVARCHAR(100) NOT NULL,
                          EventName NVARCHAR(200) NOT NULL,
                          EventCode NVARCHAR(50) NOT NULL,
                          OperatorName NVARCHAR(100) NOT NULL,
                          OperatorCode NVARCHAR(50) NOT NULL,
                          CreateDatetime DATETIME NOT NULL,
                          ExtenData NVARCHAR(MAX) NULL,
                          CreateWay TINYINT NOT NULL
);