-- FUNCTION
CREATE FUNCTION [dbo].[fn_seq](@SEQ bigint)RETURNS char(16)
AS
BEGIN
	RETURN CONVERT(VARCHAR, CONVERT(VARBINARY, @SEQ), 2)
END
GO

-- SEQUENCE
CREATE SEQUENCE [dbo].[seq] 
 AS [bigint]
 START WITH 1152921504606846976
 INCREMENT BY 1
 MINVALUE 1152921504606846976
 MAXVALUE 9223372036854775807
 CACHE 
GO

-- TABLE
CREATE TABLE [dbo].[table01](
	[row] [bigint] IDENTITY(1,1) NOT NULL,
	[pkno] [char](16) NOT NULL,
	[create_date] [datetime] NOT NULL,
	[create_user] [varchar](10) NOT NULL,
	[update_date] [datetime] NOT NULL,
	[update_user] [varchar](10) NOT NULL,
	[name] [varchar](10) NOT NULL,
 CONSTRAINT [pk__table01] PRIMARY KEY CLUSTERED 
(
	[row] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[table02](
	[row] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_pkno] [char](16) NOT NULL,
	[create_date] [datetime] NOT NULL,
	[create_user] [varchar](10) NOT NULL,
	[update_date] [datetime] NOT NULL,
	[update_user] [varchar](10) NOT NULL,
	[name] [varchar](10) NOT NULL,
 CONSTRAINT [pk__table02] PRIMARY KEY CLUSTERED 
(
	[row] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- INSERT
INSERT INTO [dbo].[table01]
	([pkno], [name], [create_date], [create_user] , [update_date], [update_user])
VALUES
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'A',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'B',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'C',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'D',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'E',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'F',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'G',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'H',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'I',GETDATE(),'source',GETDATE(),'source'),
	([dbo].[fn_seq](NEXT VALUE FOR [dbo].[seq]),'J',GETDATE(),'source',GETDATE(),'source')
GO
INSERT INTO [dbo].[table02]
([ma_pkno],[name],[create_date],[create_user],[update_date],[update_user])
SELECT
M.[pkno], M.[name] + R.[name], GETDATE(), 'source', GETDATE(), 'source'
FROM [dbo].[table01] M
JOIN (
	SELECT '1' AS [name]
	UNION
	SELECT '2'
) R ON 1 = 1
GO

-- select the current_value from sys.sequences
SELECT 
	[current_value],
	[dbo].[fn_seq](CAST([current_value] AS BIGINT))
FROM [sys].[sequences]
WHERE [name] = 'seq' ;
GO
