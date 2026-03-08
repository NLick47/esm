USE [RIS]
GO

/****** Object:  Table [dbo].[tblEventHandle]    Script Date: 2026/3/8 6:19:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblEventHandle](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [EventId] [int] NOT NULL,
    [HandleType] [varchar](50) NOT NULL,
    [HandleTypeDes] [nvarchar](500) NULL,
    [HandleTimes] [int] NOT NULL,
    [IsFinished] [bit] NOT NULL,
    [LastHandleStatus] [varchar](50) NULL,
    [LastHandleDatetime] [datetime] NULL,
    [LastHandleLogId] [int] NULL,
    CONSTRAINT [PK_tblEventHandle] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

ALTER TABLE [dbo].[tblEventHandle] ADD  CONSTRAINT [DF_tblEventHandle_HandleTimes]  DEFAULT ((0)) FOR [HandleTimes]
    GO

ALTER TABLE [dbo].[tblEventHandle] ADD  CONSTRAINT [DF_tblEventHandle_IsFinished]  DEFAULT ((0)) FOR [IsFinished]
    GO

ALTER TABLE [dbo].[tblEventHandle] ADD  CONSTRAINT [DF_tblEventHandle_LastHandleStatus]  DEFAULT ('UnHandled') FOR [LastHandleStatus]
    GO

ALTER TABLE [dbo].[tblEventHandle]  WITH CHECK ADD  CONSTRAINT [FK_tblEventHandle_tblEvent] FOREIGN KEY([EventId])
    REFERENCES [dbo].[tblEvent] ([Id])
    GO

ALTER TABLE [dbo].[tblEventHandle] CHECK CONSTRAINT [FK_tblEventHandle_tblEvent]
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主键' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'Id'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事件Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'EventId'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'处理类型，例如：状态回传，报告回传，危急值回传' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'HandleType'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'处理类型说明，可空，也可以写一些具体说明，例如参考的文档名称，接口介绍等' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'HandleTypeDes'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'处理次数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'HandleTimes'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否已完成' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'IsFinished'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'可空，最后一次处理的状态：Success （成功），Fail（失败，指没有发生异常，调用接口返回失败的信息），Exception（处理过程种，发生了异常）；未处理可空' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'LastHandleStatus'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'可空，最后一次处理时间，未处理为空' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'LastHandleDatetime'
    GO

    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'可空，最后一次处理日志记录id，未处理未空' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandle', @level2type=N'COLUMN',@level2name=N'LastHandleLogId'
    GO


