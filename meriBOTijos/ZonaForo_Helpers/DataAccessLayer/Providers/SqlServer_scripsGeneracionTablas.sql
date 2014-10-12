-------------------------------------------------------------------------------------------
--- 1 / 3: Tabla de DATOS. El nombre por defecto es TwitterBOTZF_DATA ---------------------
                    
USE [PON_AQUI_EL_NOMBRE_DE_TU_BASE_DE_DATOS]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[TwitterBOTZF_DATA](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ForumID] [int] NOT NULL,
	[ForumDesc] [nvarchar](50) NOT NULL,
	[Topic_URL] [varchar](64) NOT NULL,
	[Titulo] [nvarchar](128) NOT NULL,
	[Forero] [nvarchar](32) NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[TweetERROR] [bit] NULL,
	[Tweet] [nvarchar](150) NOT NULL,
	[YoutubeLINK] [varchar](32) NULL,
	[ImagenLINK] [varchar](128) NULL,
 CONSTRAINT [PK_TwitterBOTZF_DATA] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

-------------------------------------------------------------------------------------------
--- 2 / 3: Tabla de Chequeo de indices. El nombre por defecto es TwitterBOTZF_LASTCHECK ---

USE [PON_AQUI_EL_NOMBRE_DE_TU_BASE_DE_DATOS]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TwitterBOTZF_LASTCHECK](
	[ForumID] [int] NOT NULL,
	[ForumDesc] [nvarchar](50) NOT NULL,
	[BOT_LastPostID_Tweet] [int] NULL,
	[BOT_LastDate_CHECK] [datetime] NOT NULL,
 CONSTRAINT [PK_TwitterBOTZF_LASTCHECK] PRIMARY KEY CLUSTERED 
(
	[ForumID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

-------------------------------------------------------------------------------------------
--- 3 / 3: Tabla de ERRORES. El nombre por defecto es TwitterBOTZF_ERROR ------------------

USE [PON_AQUI_EL_NOMBRE_DE_TU_BASE_DE_DATOS]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TwitterBOTZF_ERROR](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ForumID] [int] NOT NULL,
	[ForumDesc] [nvarchar](50) NOT NULL,
	[Date_Spain] [datetime] NOT NULL,
	[ErrorDesc] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_TwitterBOTZF_ERROR] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

