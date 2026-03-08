USE [RIS]
GO

/****** Object:  Table [dbo].[tblEvent]    Script Date: 2026/3/8 6:18:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblEvent](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [intHospitalID] [bigint] NULL,
    [EventType] [varchar](50) NOT NULL,
    [strEventReferenceId] [varchar](50) NOT NULL,
    [EventName] [nvarchar](50) NOT NULL,
    [EventCode] [varchar](50) NOT NULL,
    [OperatorName] [nvarchar](50) NOT NULL,
    [OperatorCode] [varchar](50) NOT NULL,
    [CreateDatetime] [datetime] NOT NULL,
    [ExtenData] [nvarchar](4000) NULL,
    [CreateWay] [tinyint] NULL,
    CONSTRAINT [PK_tblEvent] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

ALTER TABLE [dbo].[tblEvent] ADD  CONSTRAINT [DF_tblEvent_CreateDatetime]  DEFAULT (getdate()) FOR [CreateDatetime]
    GO

ALTER TABLE [dbo].[tblEvent] ADD  CONSTRAINT [DF_tblEvent_CreateWay_1]  DEFAULT ((0)) FOR [CreateWay]
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主键，自增长' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'Id'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'医院编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'intHospitalID'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事件类型，例如：Examine' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'EventType'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事件关联id，如果EventType是Examine，那这个字段，tblExamine.strExamineId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'strEventReferenceId'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事件名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'EventName'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事件Code' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'EventCode'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'操作员名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'OperatorName'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'操作员工号（或id）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'OperatorCode'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间，不用赋值，默认getdate' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'CreateDatetime'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'扩展数据，视乎需要' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEvent', @level2type=N'COLUMN',@level2name=N'ExtenData'
    GO


