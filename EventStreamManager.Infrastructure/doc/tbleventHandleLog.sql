USE [RIS]
GO

/****** Object:  Table [dbo].[tblEventHandleLog]    Script Date: 2026/3/8 6:20:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tblEventHandleLog](
[Id] [int] IDENTITY(1,1) NOT NULL,
[EventHandleId] [int] NOT NULL,
[HandleTimes] [int] NOT NULL,
[RequestData] [text] NULL,
[ResponseData] [text] NULL,
[ExceptionMessage] [text] NULL,
[Status] [varchar](50) NOT NULL,
[HandleDatetime] [datetime] NOT NULL,
CONSTRAINT [PK_tblEventHandleLog] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[tblEventHandleLog] ADD  CONSTRAINT [DF_tblEventHandleLog_Status]  DEFAULT ('处理状态：Success （成功），Fail（失败，指没有发生异常，调用接口返回失败的信息），Exception（处理过程种，发生了异常）；') FOR [Status]
GO

ALTER TABLE [dbo].[tblEventHandleLog] ADD  CONSTRAINT [DF_tblEventHandleLog_HandleDatetime]  DEFAULT (getdate()) FOR [HandleDatetime]
GO

ALTER TABLE [dbo].[tblEventHandleLog]  WITH CHECK ADD  CONSTRAINT [FK_tblEventHandleLog_tblEventHandle] FOREIGN KEY([EventHandleId])
REFERENCES [dbo].[tblEventHandle] ([Id])
GO

ALTER TABLE [dbo].[tblEventHandleLog] CHECK CONSTRAINT [FK_tblEventHandleLog_tblEventHandle]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'外键，tblEventHandle.Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandleLog', @level2type=N'COLUMN',@level2name=N'EventHandleId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'处理次数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandleLog', @level2type=N'COLUMN',@level2name=N'HandleTimes'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'可空，请求信息，如果是中间表对接，就记录insert或update的sql语句' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandleLog', @level2type=N'COLUMN',@level2name=N'RequestData'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'可空，响应信息（一般接口对接才有）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandleLog', @level2type=N'COLUMN',@level2name=N'ResponseData'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'可空，异常信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandleLog', @level2type=N'COLUMN',@level2name=N'ExceptionMessage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态：Success （成功），Fail（失败，指没有发生异常，调用接口返回失败的信息），Exception（处理过程种，发生了异常）；' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblEventHandleLog', @level2type=N'COLUMN',@level2name=N'Status'
GO


